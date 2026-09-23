using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class OasisBriefUpgrade
{
    private const string Root = "Assets/OasisBlockout";
    private const string Done = Root + "/Editor/OasisBriefInstalled.txt";
    private const string ScenePath = "Assets/Scenes/SaraScene.unity";
    private static readonly Color Ink = new Color32(15, 47, 43, 255);
    private static readonly Color Sand = new Color32(244, 232, 201, 255);
    private static readonly Color Gold = new Color32(208, 171, 105, 255);
    public static readonly Rect[] Route =
    {
        new Rect(-25,-7,52,3), new Rect(-24,-4,10,4), new Rect(-12.75f,-.5f,2.75f,1.5f),
        new Rect(-8.75f,-4,8.75f,6), new Rect(1.25f,-4,3.75f,7), new Rect(6.25f,-4,7.75f,8),
        new Rect(15.25f,-4,3.75f,9), new Rect(20.25f,-4,6.75f,10),
        new Rect(-24.5f,.4f,2.5f,.8f), new Rect(-20.75f,1.6f,2.5f,.8f), new Rect(-17f,2.8f,4f,.8f),
        new Rect(-11.75f,4,3.75f,.8f), new Rect(-6.75f,5.2f,3.75f,.8f), new Rect(-1.75f,6.4f,4.75f,.8f),
        new Rect(4.25f,7.6f,4.75f,.8f), new Rect(10.25f,8.8f,4.75f,.8f), new Rect(16.25f,7.6f,3.75f,.8f),
        new Rect(-14,-4,5.25f,3.5f), new Rect(0,-4,1.25f,5), new Rect(5,-4,1.25f,6),
        new Rect(14,-4,1.25f,7), new Rect(19,-4,1.25f,8)
    };
    static OasisBriefUpgrade() { EditorApplication.update += Auto; }
    private static void Auto()
    {
        if (Application.isBatchMode || File.Exists(Done)) { EditorApplication.update -= Auto; return; }
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (SceneManager.GetActiveScene().path != ScenePath) return;
        EditorApplication.update -= Auto;
        try { Apply(); }
        catch (Exception e) { Debug.LogException(e); File.WriteAllText(Root + "/OasisBriefError.txt", e.ToString()); }
    }
    public static void ApplyInBatch()
    {
        EditorSceneManager.OpenScene(ScenePath);
        Apply();
    }
    [MenuItem("Al Ain/Oasis/Apply Oasis brief and safer platforms")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().path != ScenePath) return;
        if (File.Exists(Done)) return;
        var player = GameObject.Find("MainPlayer").GetComponent<PlayerMovement>();
        var dialogue = player.GetComponent<SimplePlayerDialogue>();
        dialogue.CloseDialogue();
        Undo.RecordObject(dialogue, "Set Oasis dialogue from brief");
        dialogue.characterName = "Noor";
        dialogue.dialogueText = "Guide water to the palm.";
        dialogue.dialogueLines = new[]
        {
            new DialogueLine("Bu Rashid (walkie)", "Noor, the farmers in the oasis are calling. The falaj channel is blocked."),
            new DialogueLine("Noor", "What is a falaj?"),
            new DialogueLine("Bu Rashid (walkie)", "A water channel. It carries water from deep underground to the palms."),
            new DialogueLine("Bu Rashid (walkie)", "No pumps, only the slope of the land. Water always flows downhill."),
            new DialogueLine("Noor", "Then I just need to give it a path.")
        };
        dialogue.hintLines = new[]
        {
            new DialogueLine("Bu Rashid (hint)", "Tap a stone to turn it. Start from the spring, and follow the water down.")
        };
        dialogue.completionLines = new[]
        {
            new DialogueLine("Noor", "Look! The palm is drinking!"),
            new DialogueLine("Bu Rashid", "For thousands of years, families here shared the falaj."),
            new DialogueLine("Bu Rashid", "That is how an oasis survives: everyone gets their share."),
            new DialogueLine("Memory restored", "Falaj Drop earned."),
            new DialogueLine("Visit Al Ain Oasis", "Explore shaded paths, palms and falaj channels telling the story of water.")
        };

        var environment = GameObject.Find("SARA - AL AIN OASIS BLOCKOUT").transform;
        Transform terrain = environment.Find("01 Walkable terrain - move platform groups");
        var smooth = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(Root + "/SmoothPlatformContact.physicsMaterial2D");
        if (smooth == null)
        {
            smooth = new PhysicsMaterial2D("Smooth Platform Contact") { friction = 0, bounciness = 0 };
            AssetDatabase.CreateAsset(smooth, Root + "/SmoothPlatformContact.physicsMaterial2D");
        }
        foreach (Transform platform in terrain)
        {
            if (!int.TryParse(platform.name.Substring(0, 2), out int number) || number >= Route.Length) continue;
            ResizePlatform(platform, Route[number]);
        }
        foreach (Collider2D c in environment.GetComponentsInChildren<Collider2D>()) c.sharedMaterial = smooth;
        var body = player.GetComponent<Rigidbody2D>();
        Undo.RecordObject(body, "Stable platformer collisions");
        body.constraints |= RigidbodyConstraints2D.FreezeRotation;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        player.transform.position = new Vector3(-21.4f,.65f,0);
        foreach (var col in player.GetComponentsInChildren<Collider2D>())
        {
            Undo.RecordObject(col, "Smooth player collision");
            col.sharedMaterial = smooth;
            if (player.groundCheck != null && col.transform == player.groundCheck) col.enabled = false;
        }

        Transform planting = environment.Find("03 Date palms and oasis planting");
        foreach (Transform item in planting)
        {
            float x = item.position.x, shift = 0;
            if (Near(x, 8.5f) || Near(x, 12.5f) || Near(x, 17.4f) || Near(x, 10.3f) || Near(x, 16.5f) || Near(x, 3.4f)) shift = -1;
            if (Near(x,25.7f) || Near(x,21.2f)) shift = -2;
            if (Near(x,-17.1f) || Near(x,-18.5f)) shift = -2.4f;
            if (Near(x,-5.5f) || Near(x,-7.0f)) shift = -3;
            if (Near(x,6.6f)) shift = -3.6f;
            if (Near(x,1.2f)) shift = -3.8f;
            item.position += Vector3.up * shift;
            if (Near(x,-17.1f)) item.position += Vector3.right * 1.3f;
            if (Near(x,-18.5f)) item.position += Vector3.right * 1.8f;
            if (Near(x,-7f)) item.position += Vector3.right * .6f;
        }
        foreach (SpriteRenderer palm in planting.GetComponentsInChildren<SpriteRenderer>())
        {
            if (!palm.sprite.name.StartsWith("DatePalm_")) continue;
            float x = palm.transform.position.x;
            float ground = Near(x,-22.6f) || Near(x,-16f) ? 0 :
                Near(x,-6.7f) || Near(x,-2f) ? 2 : Near(x,8.5f) || Near(x,12.5f) ? 4 :
                Near(x,17.4f) ? 5 : Near(x,25.7f) || Near(x,-5.5f) ? 6 : Near(x,-15.8f) ? 3.6f : 8.4f;
            GroundPalm(palm,ground);
        }
        var fort = GameObject.Find("Al Jahili inspired fort - destination");
        if (fort != null) fort.SetActive(false);
        var tower = GameObject.Find("Hili lookout landmark");
        if (tower != null) tower.SetActive(false);
        terrain.GetChild(7).name = "07 Oasis exit toward Jebel Hafeet";
        var markers = environment.Find("05 Design markers - editor only");
        foreach (Transform item in markers)
        {
            if (item.name.StartsWith("START")) item.position = new Vector3(-21.4f,0,0);
            if (item.name.StartsWith("END")) { item.name = "NEXT CHAPTER - Jebel Hafeet connection"; item.position = new Vector3(23.5f, 6, 0); }
            if (item.name.StartsWith("FUTURE")) { item.name = "FALAJ PUZZLE - connect success to dry palm hooks"; item.position = new Vector3(9.5f,4,0); }
        }
        SetWater("Falaj basin A", -14,-.45f,5.25f,.45f);
        SetWater("Falaj basin B", 0,1.05f,1.25f,.4f);
        SetWater("Falaj basin C", 5,2.05f,1.25f,.4f);
        SetWater("Falaj basin D", 14,3.05f,1.25f,.4f);
        SetWater("Falaj basin E", 19,4.05f,1.25f,.4f);
        var stream = GameObject.Find("Raised falaj strip");
        var spill = GameObject.Find("Channel spill - visual only");
        if (stream != null) { stream.transform.position += Vector3.down; stream.SetActive(false); }
        if (spill != null) { spill.transform.position += Vector3.down; spill.SetActive(false); }

        SpriteRenderer dry = planting.GetComponentsInChildren<SpriteRenderer>().First(p => Near(p.transform.position.x, 12.5f));
        dry.gameObject.name = "Dry palm - falaj puzzle completion hook";
        var hook = dry.gameObject.AddComponent<OasisPuzzleHooks>();
        hook.dialogue = dialogue; hook.palm = dry; hook.healthyPalm = dry.sprite;
        float healthyY = dry.transform.localPosition.y;
        hook.onCompleted = new UnityEngine.Events.UnityEvent();
        if (stream != null) UnityEventTools.AddBoolPersistentListener(hook.onCompleted, stream.SetActive, true);
        if (spill != null) UnityEventTools.AddBoolPersistentListener(hook.onCompleted, spill.SetActive, true);
        var palmSprites = AssetDatabase.LoadAllAssetsAtPath(Root + "/Art/Date_Palm_Soft_Mobile_Atlas_3x2_1536.png").OfType<Sprite>().OrderBy(p => p.name).ToArray();
        dry.sprite = palmSprites[5];
        dry.color = new Color(.70f,.60f,.40f,1);
        GroundPalm(dry,4);
        hook.healthyPalmVerticalOffset = healthyY - dry.transform.localPosition.y;
        var click = dry.gameObject.AddComponent<BoxCollider2D>();
        click.isTrigger = true; click.size = dry.sprite.bounds.size; click.offset = dry.sprite.bounds.center;

        var old = GameObject.Find("Al Ain Dialogue System");
        if (old != null) old.SetActive(false);
        var score = GameObject.Find("Canvas");
        if (score != null && score.transform.parent == null) score.SetActive(false);
        BuildPhoneUI(player, dialogue);
        Validate(player, dialogue, terrain);
        EditorUtility.SetDirty(dialogue);
        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
        EditorSceneManager.SaveScene(player.gameObject.scene);
        AssetDatabase.SaveAssets();
        File.WriteAllText(Done, "Oasis dialogue and safer platform layout installed. Intro/hint active; success lines require real puzzle completion.\n");
        SaraOasisBlockoutBuilder.ExportPreviews();
        AssetDatabase.Refresh();
        Selection.activeGameObject = player.gameObject;
        Debug.Log("OASIS_BRIEF_READY");
    }
    private static bool Near(float a,float b) => Mathf.Abs(a-b)<.02f;
    private static void GroundPalm(SpriteRenderer palm,float surface)
    {
        // Atlas cells contain different transparent margins. Align visible roots, not cell edges.
        var sprite=palm.sprite;
        var texture=new Texture2D(2,2);
        texture.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(sprite)));
        var pixels=texture.GetPixels32();
        Rect cell=sprite.rect;
        int bottom=(int)cell.yMin;
        bool found=false;
        for(int y=(int)cell.yMin;y<(int)cell.yMax && !found;y++)
            for(int x=(int)cell.xMin;x<(int)cell.xMax;x++)
                if(pixels[y*texture.width+x].a>32){bottom=y;found=true;break;}
        float localBottom=(bottom-cell.yMin-sprite.pivot.y)/sprite.pixelsPerUnit;
        var position=palm.transform.position;
        position.y=surface-.035f-localBottom*palm.transform.lossyScale.y;
        palm.transform.position=position;
        Object.DestroyImmediate(texture);
    }
    private static void ResizePlatform(Transform platform, Rect shape)
    {
        for (int i=platform.childCount-1;i>=0;i--) Object.DestroyImmediate(platform.GetChild(i).gameObject);
        platform.position = new Vector3(shape.x,shape.y,0);
        var box = platform.GetComponent<BoxCollider2D>(); box.size=shape.size; box.offset=shape.size*.5f;
        var mat = AssetDatabase.LoadAssetAtPath<Material>(Root + "/OasisSprite.mat");
        int rows=Mathf.CeilToInt(shape.height), cols=Mathf.CeilToInt(shape.width);
        for(int y=0;y<rows;y++) for(int x=0;x<cols;x++)
        {
            float w=Mathf.Min(1,shape.width-x), h=Mathf.Min(1,shape.height-y);
            string file=y==rows-1 ? (x==0 ? "05_Sand_Top_Left_512.png" : x==cols-1 ? "08_Sand_Top_Right_512.png" : "06_Sand_Top_Centre_A_512.png") : ((x+y)%3==0?"10_Stone_Fill_B_512.png":"09_Stone_Fill_A_512.png");
            Sprite sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Core Tile Set_512/"+file);
            var tile=new GameObject("Tile "+x+","+y,typeof(SpriteRenderer)); tile.transform.SetParent(platform,false);
            tile.transform.localPosition=new Vector3(x+w*.5f,y+h*.5f,0);
            tile.transform.localScale=new Vector3((w+.006f)/sprite.bounds.size.x,(h+.006f)/sprite.bounds.size.y,1);
            var renderer=tile.GetComponent<SpriteRenderer>(); renderer.sprite=sprite; renderer.sharedMaterial=mat;
        }
    }
    private static void SetWater(string name,float x,float y,float w,float h)
    {
        var go=GameObject.Find(name); if(go==null)return;
        var sprite=go.GetComponent<SpriteRenderer>().sprite;
        go.transform.position=new Vector3(x+w*.5f,y+h*.5f,0);
        go.transform.localScale=new Vector3(w/sprite.bounds.size.x,h/sprite.bounds.size.y,1);
    }
    private static RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 pivot,Vector2 position,Vector2 size)
    {
        var go=new GameObject(name,typeof(RectTransform)); go.layer=5;
        var r=(RectTransform)go.transform; r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;r.anchoredPosition=position;r.sizeDelta=size;return r;
    }
    private static RectTransform Stretch(string name,Transform parent)
    {
        var r=Rect(name,parent,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero);r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;
    }
    private static Image Paint(RectTransform rect,Color color,bool hit=false)
    {
        var i=rect.gameObject.AddComponent<Image>();i.color=color;i.raycastTarget=hit;return i;
    }
    private static TMP_Text Text(string name,Transform parent,string words,int size,Vector2 position,Vector2 dimensions)
    {
        var r=Rect(name,parent,new Vector2(0,1),new Vector2(0,1),position,dimensions);
        var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=TMP_Settings.defaultFontAsset;t.fontSize=size;t.text=words;t.color=Sand;t.raycastTarget=false;t.richText=false;
        t.alignment=TextAlignmentOptions.MidlineLeft;return t;
    }
    private static Button Button(string name,Transform parent,string words,Vector2 anchor,Vector2 pos,Vector2 size,out TMP_Text label)
    {
        var r=Rect(name,parent,anchor,anchor,pos,size);var i=Paint(r,Sand,true);
        var b=r.gameObject.AddComponent<Button>();b.targetGraphic=i;b.navigation=new Navigation{mode=Navigation.Mode.None};
        label=Text("Label",r,words,16,Vector2.zero,size); label.alignment=TextAlignmentOptions.Center;label.color=Ink;
        return b;
    }
    private static void BuildPhoneUI(PlayerMovement player,SimplePlayerDialogue dialogue)
    {
        var go=new GameObject("Oasis Phone UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));go.layer=5;
        var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=200;
        var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(360,640);scaler.matchWidthOrHeight=1;
        var safe=Stretch("Safe area",go.transform);
        var layout=go.AddComponent<OasisPhoneLayout>();layout.safeArea=safe;layout.player=player;
        var objective=Rect("Oasis objective",safe,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-14),new Vector2(336,52));Paint(objective,Ink);
        var objectiveText=Text("Objective",objective,"AL AIN OASIS\nGuide water to the palm.",15,new Vector2(14,-5),new Vector2(308,42));
        objectiveText.rectTransform.anchorMax=new Vector2(1,1);objectiveText.rectTransform.sizeDelta=new Vector2(-28,42);
        dialogue.objectiveLabel=objectiveText;layout.objective=objective;
        foreach(var action in new[]{OasisTouchButton.Action.Left,OasisTouchButton.Action.Right,OasisTouchButton.Action.Jump})
        {
            bool jump=action==OasisTouchButton.Action.Jump;
            var button=Button(action.ToString()+" touch control",safe,jump?"JUMP":action==OasisTouchButton.Action.Left?"<":">",
                jump?new Vector2(1,0):Vector2.zero,new Vector2(jump?-14:action==OasisTouchButton.Action.Left?14:80,18),new Vector2(jump?68:56,58),out TMP_Text label);
            label.fontSize=jump?16:28;
            var touch=button.gameObject.AddComponent<OasisTouchButton>();touch.player=player;touch.action=action;
        }
        var modal=Stretch("Dialogue modal",safe);Paint(modal,new Color(0,0,0,.08f),true);
        var card=Rect("Oasis dialogue bar",modal,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,96),new Vector2(336,218));Paint(card,Ink,true);
        var border=card.gameObject.AddComponent<Outline>();border.effectColor=Gold;border.effectDistance=new Vector2(1,-1);
        var cardTap=card.gameObject.AddComponent<Button>();cardTap.targetGraphic=card.GetComponent<Image>();cardTap.navigation=new Navigation{mode=Navigation.Mode.None};
        UnityEventTools.AddPersistentListener(cardTap.onClick,dialogue.NextLine);
        var avatar=Rect("Portrait frame",card,new Vector2(0,1),new Vector2(0,1),new Vector2(14,-12),new Vector2(44,44));Paint(avatar,new Color32(39,79,64,255));
        dialogue.initialsLabel=Text("Portrait initials",avatar,"BR",20,Vector2.zero,new Vector2(44,44));dialogue.initialsLabel.alignment=TextAlignmentOptions.Center;dialogue.initialsLabel.color=Gold;
        var portrait=Stretch("Dialogue portrait",avatar);dialogue.portraitImage=Paint(portrait,Color.white);dialogue.portraitImage.preserveAspect=true;portrait.gameObject.SetActive(false);
        Text("Place",card,"AL AIN OASIS",10,new Vector2(70,-10),new Vector2(142,16)).color=Gold;
        dialogue.nameLabel=Text("Speaker",card,"Bu Rashid (walkie)",15,new Vector2(70,-26),new Vector2(158,27));
        dialogue.nameLabel.enableAutoSizing=true;dialogue.nameLabel.fontSizeMin=11;dialogue.nameLabel.fontSizeMax=15;
        var hint=Button("Dialogue hint",card,"Hint",Vector2.one,new Vector2(-57,-10),new Vector2(47,44),out _);
        UnityEventTools.AddPersistentListener(hint.onClick,dialogue.ShowHint);
        var close=Button("Close dialogue",card,"X",Vector2.one,new Vector2(-10,-10),new Vector2(40,44),out _);
        UnityEventTools.AddPersistentListener(close.onClick,dialogue.CloseDialogue);
        var scroll=Rect("Text viewport",card,new Vector2(0,1),new Vector2(0,1),new Vector2(16,-70),new Vector2(-32,80));scroll.anchorMax=Vector2.one;
        Paint(scroll,Color.clear,true);scroll.gameObject.AddComponent<RectMask2D>();var scroller=scroll.gameObject.AddComponent<ScrollRect>();scroller.horizontal=false;scroller.movementType=ScrollRect.MovementType.Clamped;
        var words=Text("Dialogue words",scroll,"",17,Vector2.zero,new Vector2(0,80));words.rectTransform.anchorMax=Vector2.one;words.rectTransform.pivot=new Vector2(.5f,1);words.alignment=TextAlignmentOptions.TopLeft;
        words.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        scroller.viewport=scroll;scroller.content=words.rectTransform;dialogue.textLabel=words;
        dialogue.pageLabel=Text("Page",card,"1 / 5",14,new Vector2(16,-176),new Vector2(72,28));dialogue.pageLabel.color=Gold;
        var next=Button("Next dialogue",card,"Next  >",new Vector2(1,0),new Vector2(-14,12),new Vector2(116,44),out TMP_Text nextText);
        dialogue.nextLabel=nextText;UnityEventTools.AddPersistentListener(next.onClick,dialogue.NextLine);
        dialogue.dialogueBox=modal.gameObject;layout.card=card;dialogue.CloseDialogue();
    }
    private static void Validate(PlayerMovement player,SimplePlayerDialogue dialogue,Transform terrain)
    {
        if(terrain.childCount!=22)throw new Exception("Platform groups missing.");
        for(int i=1;i<7;i++) if(Route[i+1].xMin-Route[i].xMax>1.251f || Route[i+1].yMax-Route[i].yMax>1.01f)throw new Exception("Main route gap too large.");
        foreach(var line in dialogue.dialogueLines.Concat(dialogue.hintLines).Concat(dialogue.completionLines))
            if(line.text.Length>90)throw new Exception("Dialogue line exceeds phone length target.");
        Physics2D.SyncTransforms();
        if(!Physics2D.Raycast(player.transform.position,Vector2.down,2,1<<3))throw new Exception("Player spawn has no floor.");
        if(dialogue.dialogueBox==null || dialogue.textLabel==null)throw new Exception("Dialogue references missing.");
        Debug.Log("OASIS_BRIEF_CHECK_PASS: scene references, short source dialogue, main-route rises <= 1 and gaps <= 1.25.");
    }
}
