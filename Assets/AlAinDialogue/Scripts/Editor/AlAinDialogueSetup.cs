using System.IO;
using AlAin.Dialogue;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public static class AlAinDialogueSetup
{
    private const string Root = "Assets/AlAinDialogue";

    [MenuItem("Al Ain/Dialogue/Add dialogue to current scene")]
    public static void AddToCurrentScene()
    {
        if (Application.isPlaying) return;
        Directory.CreateDirectory(Root + "/Art");
        Directory.CreateDirectory(Root + "/Prefabs");
        AssetDatabase.Refresh();
        Sprite sprite = MakePlaceholder();
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        if (player == null) throw new System.InvalidOperationException("Add a player with PlayerMovement first.");

        if (Object.FindFirstObjectByType<AlAinDialogueController>() == null)
        {
            var system = new GameObject("Al Ain Dialogue System");
            var controller = system.AddComponent<AlAinDialogueController>();
            var canvas = new GameObject("Oasis Dialogue Canvas", typeof(RectTransform));
            canvas.transform.SetParent(system.transform, false);
            var view = canvas.AddComponent<AlAinDialogueUI>();
            view.Build(sprite);
            controller.view = view;
            // Save a reusable prefab without a reference to a scene object.
            PrefabUtility.SaveAsPrefabAssetAndConnect(system, Root + "/Prefabs/Al Ain Dialogue System.prefab", InteractionMode.AutomatedAction);
            controller.player = player;
            PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
            Undo.RegisterCreatedObjectUndo(system, "Add Al Ain dialogue system");
        }
        if (Object.FindFirstObjectByType<AlAinDialogueTarget>() == null)
        {
            var targetObject = new GameObject("Oasis Guide - edit my dialogue and images");
            var target = targetObject.AddComponent<AlAinDialogueTarget>();
            var picture = new GameObject("World Image - replace from parent Inspector");
            picture.transform.SetParent(targetObject.transform, false);
            var renderer = picture.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 5;
            target.SetImageRenderer(renderer, sprite);
            target.lines = new[]
            {
                "Welcome to Al Ain! Follow the palm-lined path and look for the entrance to the oasis.",
                "Your objective: reach the oasis gate. Come back and speak to me whenever you need a reminder."
            };
            PrefabUtility.SaveAsPrefabAssetAndConnect(targetObject, Root + "/Prefabs/Al Ain Interactable.prefab", InteractionMode.AutomatedAction);
            targetObject.transform.position = player.transform.position + new Vector3(1.5f, 0, 0);
            var parent = GameObject.Find("InteractableItems");
            if (parent != null) targetObject.transform.SetParent(parent.transform, true);
            PrefabUtility.RecordPrefabInstancePropertyModifications(targetObject.transform);
            Undo.RegisterCreatedObjectUndo(targetObject, "Add oasis guide");
            Selection.activeGameObject = targetObject;
        }
        var events = Object.FindFirstObjectByType<EventSystem>();
        if (events == null)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(go, "Add UI input");
        }
        else if (events.GetComponent<InputSystemUIInputModule>() == null)
        {
            var old = events.GetComponent<StandaloneInputModule>();
            if (old != null) Undo.DestroyObjectImmediate(old);
            Undo.AddComponent<InputSystemUIInputModule>(events.gameObject);
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Al Ain dialogue is ready. Select the Oasis Guide to edit World Image, Portrait and Lines.");
    }

    public static void InstallInSampleScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        AddToCurrentScene();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("AL_AIN_INSTALL_SUCCESS");
    }

    private static Sprite MakePlaceholder()
    {
        string path = Root + "/Art/OasisFortPlaceholder.png";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;
        // Original code-drawn pixel illustration; replace this with your own character art whenever ready.
        const int size = 128;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        Color32 green = new Color32(23, 61, 49, 255);
        Color32 gold = new Color32(211, 173, 106, 255);
        Color32 sand = new Color32(245, 225, 178, 255);
        for (int i = 0; i < pixels.Length; i++) pixels[i] = green;
        void Box(int x, int y, int width, int height, Color32 color)
        {
            for (int py = y; py < y + height; py++)
                for (int px = x; px < x + width; px++)
                    if (px >= 0 && px < size && py >= 0 && py < size) pixels[py * size + px] = color;
        }
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if ((x - 91) * (x - 91) + (y - 99) * (y - 99) < 12 * 12) pixels[y * size + x] = gold;
        Box(14, 18, 100, 3, gold);
        Box(25, 23, 22, 53, gold); Box(81, 23, 22, 53, gold);
        Box(47, 23, 34, 37, sand);
        for (int x = 25; x < 104; x += 13) Box(x, x < 47 || x > 80 ? 76 : 60, 7, 8, sand);
        Box(58, 23, 13, 24, green); Box(61, 47, 7, 4, green);
        Box(32, 53, 6, 13, green); Box(89, 53, 6, 13, green);
        // Small palm beside the fort.
        Box(13, 24, 4, 46, gold);
        for (int i = 0; i < 14; i++)
        {
            Box(15 + i, 72 - i / 2, 3, 3, sand);
            Box(15 - i, 72 - i / 2, 3, 3, sand);
            Box(15 + i / 2, 72 + i / 2, 3, 3, sand);
            Box(15 - i / 2, 72 + i / 2, 3, 3, sand);
        }
        texture.SetPixels32(pixels); texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 128;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
