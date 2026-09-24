using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// One-time reconnection for the replacement Shamma player. Does not rebuild the map.
[InitializeOnLoad]
public static class ShammaOasisDialogueSetup
{
    const string ScenePath="Assets/Scenes/SaraScene.unity";
    const string Art="Assets/OasisBlockout/Art/Portraits/";
    const string Marker="Assets/OasisBlockout/Editor/ShammaDialogueInstalled.txt";
    static ShammaOasisDialogueSetup(){EditorApplication.update+=Auto;}
    static void Auto()
    {
        if(Application.isBatchMode || File.Exists(Marker)){EditorApplication.update-=Auto;return;}
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)return;
        if(SceneManager.GetActiveScene().path!=ScenePath)return;
        EditorApplication.update-=Auto;
        try{Apply();}
        catch(Exception error){Debug.LogException(error);File.WriteAllText("Assets/OasisBlockout/ShammaDialogueError.txt",error.ToString());}
    }
    public static void ApplyInBatch(){EditorSceneManager.OpenScene(ScenePath);Apply();}
    [MenuItem("Al Ain/Oasis/Connect Shamma and Abu Rashid dialogue")]
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().path!=ScenePath)return;
        if(File.Exists(Marker))return;
        Sprite shamma=ImportPortrait(Art+"ShammaPortrait.png",true);
        Sprite rashid=ImportPortrait(Art+"AbuRashidPortrait.png",false);
        var player=GameObject.Find("MainPlayer").GetComponent<PlayerMovement>();
        var dialogue=player.GetComponent<SimplePlayerDialogue>();
        var layout=UnityEngine.Object.FindFirstObjectByType<OasisPhoneLayout>();
        if(dialogue==null || layout==null || shamma==null || rashid==null)throw new Exception("Missing player, phone UI or portrait assets.");
        var nodes=layout.GetComponentsInChildren<Transform>(true);
        Transform Node(string name)=>nodes.Single(t=>t.name==name);
        TMP_Text Label(string name)=>Node(name).GetComponent<TMP_Text>();
        Undo.RecordObject(dialogue,"Connect Shamma Oasis dialogue");
        dialogue.characterName="Shamma";
        // Every spoken page has an explicit portrait. Token text uses the neutral initials badge.
        dialogue.dialogueImage=null;
        dialogue.dialogueText="Guide water to the palm.";
        DialogueLine Line(string who,string text,Sprite portrait)=>new DialogueLine(who,text){portrait=portrait};
        dialogue.dialogueLines=new[]{
            Line("Abu Rashid (walkie)","Shamma, the farmers in the oasis are calling. The falaj channel is blocked.",rashid),
            Line("Shamma","What is a falaj?",shamma),
            Line("Abu Rashid (walkie)","A water channel. It carries water from deep underground to the palms.",rashid),
            Line("Abu Rashid (walkie)","No pumps, only the slope of the land. Water always flows downhill.",rashid),
            Line("Shamma","Then I just need to give it a path.",shamma)
        };
        dialogue.hintLines=new[]{Line("Abu Rashid (hint)","Tap a stone to turn it. Start from the spring, and follow the water down.",rashid)};
        dialogue.completionLines=new[]{
            Line("Shamma (solved)","Look! The palm is drinking!",shamma),
            Line("Shamma (solved)","For thousands of years, families here shared the falaj.",shamma),
            Line("Shamma (solved)","That is how an oasis survives: everyone gets their share.",shamma),
            Line("Token","Falaj Drop earned.",null)
        };
        dialogue.dialogueBox=Node("Dialogue modal").gameObject;
        dialogue.nameLabel=Label("Speaker");dialogue.textLabel=Label("Dialogue words");
        dialogue.pageLabel=Label("Page");dialogue.initialsLabel=Label("Portrait initials");
        dialogue.objectiveLabel=Label("Objective");
        dialogue.nextLabel=Node("Next dialogue").GetComponentInChildren<TMP_Text>(true);
        dialogue.portraitImage=Node("Dialogue portrait").GetComponent<Image>();
        dialogue.portraitImage.preserveAspect=true;
        Connect(Node("Oasis dialogue bar").GetComponent<Button>(),dialogue.NextLine);
        Connect(Node("Next dialogue").GetComponent<Button>(),dialogue.NextLine);
        Connect(Node("Dialogue hint").GetComponent<Button>(),dialogue.ShowHint);
        Connect(Node("Close dialogue").GetComponent<Button>(),dialogue.CloseDialogue);
        Undo.RecordObject(layout,"Reconnect Shamma touch controls");layout.player=player;EditorUtility.SetDirty(layout);
        foreach(var touch in layout.GetComponentsInChildren<OasisTouchButton>(true))
        {Undo.RecordObject(touch,"Reconnect Shamma touch control");touch.player=player;EditorUtility.SetDirty(touch);}
        foreach(var hook in UnityEngine.Object.FindObjectsByType<OasisPuzzleHooks>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {Undo.RecordObject(hook,"Reconnect Shamma puzzle dialogue");hook.dialogue=dialogue;EditorUtility.SetDirty(hook);}
        if(Camera.main.GetComponent<Physics2DRaycaster>()==null)Undo.AddComponent<Physics2DRaycaster>(Camera.main.gameObject);
        // Make the portraits readable without changing the user's overall UI layout.
        var frame=(RectTransform)Node("Portrait frame");frame.sizeDelta=new Vector2(56,56);frame.anchoredPosition=new Vector2(14,-8);
        dialogue.initialsLabel.rectTransform.sizeDelta=new Vector2(56,56);
        foreach(string name in new[]{"Place","Speaker"})
        {var rect=(RectTransform)Node(name);rect.anchoredPosition=new Vector2(82,rect.anchoredPosition.y);rect.sizeDelta=new Vector2(146,rect.sizeDelta.y);}
        dialogue.CloseDialogue();
        dialogue.nameLabel.text="Abu Rashid (walkie)";dialogue.textLabel.text=dialogue.dialogueLines[0].text;
        dialogue.portraitImage.sprite=rashid;dialogue.portraitImage.gameObject.SetActive(true);dialogue.initialsLabel.gameObject.SetActive(false);
        dialogue.pageLabel.text="1 / 5";dialogue.nextLabel.text="Next  >";
        dialogue.objectiveLabel.text="AL AIN OASIS\nGuide water to the palm.";
        EditorUtility.SetDirty(dialogue);
        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
        EditorSceneManager.SaveScene(player.gameObject.scene);AssetDatabase.SaveAssets();
        File.WriteAllText(Marker,"Shamma and Abu Rashid portraits, source dialogue, UI and player references connected.\n");
        AssetDatabase.Refresh();Selection.activeGameObject=player.gameObject;
        Debug.Log("SHAMMA_DIALOGUE_READY: existing scene preserved; dialogue and touch controls reconnected.");
    }
    static void Connect(Button button,UnityAction callback)
    {
        Undo.RecordObject(button,"Reconnect dialogue button");
        for(int i=button.onClick.GetPersistentEventCount()-1;i>=0;i--)UnityEventTools.RemovePersistentListener(button.onClick,i);
        UnityEventTools.AddPersistentListener(button.onClick,callback);EditorUtility.SetDirty(button);
    }
    static Sprite ImportPortrait(string path,bool isShamma)
    {
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
        importer.maxTextureSize=2048;importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit=100;
        if(isShamma)
        {
            // A Unity sprite slice of the original pixels; the character/animation source is untouched.
            importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.spritesheet=new[]{new SpriteMetaData{name="ShammaPortrait",rect=new Rect(500,1090,800,920),pivot=new Vector2(.5f,.5f),alignment=(int)SpriteAlignment.Center}};
        }
        else importer.spriteImportMode=SpriteImportMode.Single;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
    }
}
