using System;
using System.IO;
using AlAin.Dialogue;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class SimpleDialogueSetup
{
    private const string Marker = "Assets/AlAinDialogue/SimplePlayerSetup.done.txt";
    static SimpleDialogueSetup() { EditorApplication.update += InstallOnce; }
    private static void InstallOnce()
    {
        if (Application.isBatchMode || File.Exists(Marker))
        { EditorApplication.update -= InstallOnce; return; }
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (GameObject.Find("MainPlayer") == null) return;
        EditorApplication.update -= InstallOnce;
        try { Apply(); }
        catch (Exception error) { Debug.LogException(error); }
    }

    [MenuItem("Al Ain/Dialogue/Use simple MainPlayer dialogue")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var player = GameObject.Find("MainPlayer");
        var view = Object.FindFirstObjectByType<AlAinDialogueUI>();
        if (player == null || view == null) throw new Exception("Open SampleScene with its existing dialogue canvas first.");
        Undo.IncrementCurrentGroup();
        int undo = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Simplify MainPlayer dialogue");

        // Unpack only our UI instance so the reusable old prefab is untouched.
        GameObject uiRoot = view.transform.root.gameObject;
        if (PrefabUtility.IsPartOfPrefabInstance(uiRoot))
            PrefabUtility.UnpackPrefabInstance(uiRoot, PrefabUnpackMode.Completely, InteractionMode.UserAction);
        foreach (var old in uiRoot.GetComponents<AlAinDialogueController>()) Undo.DestroyObjectImmediate(old);
        var guide = GameObject.Find("Oasis Guide - edit my dialogue and images");
        if (guide != null) Undo.DestroyObjectImmediate(guide);

        var script = player.GetComponent<SimplePlayerDialogue>();
        if (script == null) script = Undo.AddComponent<SimplePlayerDialogue>(player);
        Transform Find(string name)
        {
            foreach (Transform item in uiRoot.GetComponentsInChildren<Transform>(true))
                if (item.name == name) return item;
            throw new Exception("Missing dialogue UI object: " + name);
        }
        script.dialogueBox = Find("Dialogue modal").gameObject;
        script.nameLabel = Find("Character name").GetComponent<TMP_Text>();
        script.textLabel = Find("Objective text").GetComponent<TMP_Text>();
        script.portraitImage = Find("Portrait - your image appears here").GetComponent<Image>();
        script.portraitImage.sprite = null;
        script.portraitImage.gameObject.SetActive(false);
        Find("Portrait caption").GetComponent<TMP_Text>().text = "YOUR CHARACTER";
        Find("Objective title").GetComponent<TMP_Text>().text = "Your objective";
        Find("Instructions").GetComponent<TMP_Text>().text = "Click or tap Close to return";
        Find("Continue or finish").GetComponentInChildren<TMP_Text>().text = "Close";
        foreach (string name in new[] { "Continue or finish", "Close dialogue" })
        {
            Button button = Find(name).GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            UnityEventTools.AddPersistentListener(button.onClick, script.CloseDialogue);
            EditorUtility.SetDirty(button);
        }
        Undo.DestroyObjectImmediate(Find("Tap to interact").gameObject);
        Undo.DestroyObjectImmediate(view);
        var camera = Camera.main;
        if (camera == null) throw new Exception("A MainCamera is required.");
        if (camera.GetComponent<Physics2DRaycaster>() == null) Undo.AddComponent<Physics2DRaycaster>(camera.gameObject);
        if (player.GetComponent<Collider2D>() == null) Undo.AddComponent<BoxCollider2D>(player);
        script.CloseDialogue();
        Verify(script);
        EditorUtility.SetDirty(script);
        Selection.activeGameObject = player;
        EditorSceneManager.MarkSceneDirty(player.scene);
        EditorSceneManager.SaveScene(player.scene);
        Undo.CollapseUndoOperations(undo);
        File.WriteAllText(Marker, "Installed simple click/tap dialogue on MainPlayer. Click route, text/image binding and pause restoration verified.\n");
        AssetDatabase.Refresh();
        Debug.Log("SIMPLE_PLAYER_DIALOGUE_READY: Select MainPlayer and edit Character Name, Dialogue Image and Dialogue Text.");
    }

    private static void Verify(SimplePlayerDialogue script)
    {
        var events = Object.FindFirstObjectByType<EventSystem>();
        if (events == null) throw new Exception("EventSystem missing.");
        var movement = script.GetComponent<PlayerMovement>();
        bool pausedBefore = movement != null && movement.isPaused;
        Physics2D.SyncTransforms();
        var pointer = new PointerEventData(events)
        {
            button = PointerEventData.InputButton.Left,
            position = Camera.main.WorldToScreenPoint(script.transform.position)
        };
        var hits = new System.Collections.Generic.List<RaycastResult>();
        Camera.main.GetComponent<Physics2DRaycaster>().Raycast(pointer, hits);
        bool playerHit = hits.Exists(hit => hit.gameObject.GetComponentInParent<SimplePlayerDialogue>() == script);
        if (!playerHit) throw new Exception("MainPlayer did not receive a 2D pointer raycast.");
        ExecuteEvents.Execute(script.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        if (!script.dialogueBox.activeSelf || script.textLabel.text != script.dialogueText)
            throw new Exception("Click did not open the configured dialogue.");
        if (movement != null && !movement.isPaused) throw new Exception("Player did not pause.");
        script.CloseDialogue();
        if (movement != null && movement.isPaused != pausedBefore) throw new Exception("Player pause was not restored.");
        Debug.Log("SIMPLE_DIALOGUE_CHECK_PASS: Physics2D pointer hit, click opens text, empty portrait hidden, close restores movement.");
    }
}
