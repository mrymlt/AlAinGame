using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Editor-only construction tool. The finished environment is ordinary editable scene objects.
[InitializeOnLoad]
public static class SaraOasisBlockoutBuilder
{
    private const string Root = "Assets/OasisBlockout";
    private const string Art = Root + "/Art/";
    private const string ScenePath = "Assets/Scenes/SaraScene.unity";
    private const string Marker = Root + "/Editor/SaraOasisInstalled.txt";
    private static readonly List<Rect> Platforms = new List<Rect>();
    private static Material material;
    private static Transform terrain, plants, scenery, water, markers;
    private static Sprite[] palms;

    static SaraOasisBlockoutBuilder() { EditorApplication.update += InstallWhenReady; }
    public static void BuildForBatchValidation()
    {
        EditorSceneManager.OpenScene(ScenePath);
        Build();
    }
    private static void InstallWhenReady()
    {
        if (Application.isBatchMode || (File.Exists(Marker) && File.ReadAllText(Marker).Contains("oasis-v2")))
        { EditorApplication.update -= InstallWhenReady; return; }
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (SceneManager.GetActiveScene().path != ScenePath) return;
        EditorApplication.update -= InstallWhenReady;
        try
        {
            if (File.Exists(Marker))
            {
                var tower = GameObject.Find("Hili lookout landmark");
                if (tower != null && Mathf.Abs(tower.transform.position.y - 14.6f) < 0.01f)
                {
                    Undo.RecordObject(tower.transform, "Align lookout artwork with platform");
                    tower.transform.position = new Vector3(12.9f, 14.28f, 0);
                    EditorSceneManager.MarkSceneDirty(tower.scene);
                    EditorSceneManager.SaveScene(tower.scene);
                }
                ExportPreviews();
                File.WriteAllText(Marker, "oasis-v2: SaraScene oasis blockout installed and previewed.\n");
            }
            else Build();
        }
        catch (Exception error) { Debug.LogException(error); File.WriteAllText(Path.GetFullPath(Root + "/BuildError.txt"), error.ToString()); }
    }

    [MenuItem("Al Ain/Oasis/Build SaraScene blockout")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (SceneManager.GetActiveScene().path != ScenePath)
            throw new InvalidOperationException("Open SaraScene first. This tool edits only SaraScene.");
        if (GameObject.Find("SARA - AL AIN OASIS BLOCKOUT") != null)
        { Debug.Log("Oasis blockout already exists. Edit its platform groups directly in the Hierarchy."); return; }

        ImportArt();
        material = AssetDatabase.LoadAssetAtPath<Material>(Root + "/OasisSprite.mat");
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            AssetDatabase.CreateAsset(material, Root + "/OasisSprite.mat");
        }
        palms = AssetDatabase.LoadAllAssetsAtPath(Art + "Date_Palm_Soft_Mobile_Atlas_3x2_1536.png").OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (palms.Length != 6) throw new Exception("Expected six palm sprites from the supplied atlas.");

        var scene = SceneManager.GetActiveScene();
        Undo.IncrementCurrentGroup();
        int undo = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Build Sara oasis environment");
        var archive = Group("Previous layout - disabled backup", null);
        foreach (string name in new[] { "GroundObjects", "MissionOne", "InteractableItems" })
        {
            var old = GameObject.Find(name);
            if (old != null) Undo.SetTransformParent(old.transform, archive, "Preserve previous environment");
        }
        archive.gameObject.SetActive(false);

        var root = Group("SARA - AL AIN OASIS BLOCKOUT", null);
        terrain = Group("01 Walkable terrain - move platform groups", root);
        water = Group("02 Falaj water - decorative for blockout", root);
        plants = Group("03 Date palms and oasis planting", root);
        scenery = Group("04 Background and landmarks", root);
        markers = Group("05 Design markers - editor only", root);
        markers.tag = "EditorOnly";
        Platforms.Clear();

        // A solid lower shelf lets players recover from the small channel gaps.
        Platform("00 Recovery floor", -25, -7, 52, 3);
        Platform("01 Palm entrance", -24, -4, 10, 4);
        Platform("02 First channel stepping stone", -12, -2, 2, 3);
        Platform("03 Date grove terrace", -8, -4, 8, 6);
        Platform("04 Central stepping terrace", 2, -4, 3, 8);
        Platform("05 Falaj garden terrace", 7, -4, 7, 9);
        Platform("06 Fort approach", 16, -4, 3, 10);
        Platform("07 Fort destination terrace", 21, -4, 6, 12);

        // Secondary loop: short climbs replace the reference's ladders for this blockout.
        Platform("08 West climb - step one", -24, 1, 2, 1);
        Platform("09 West climb - step two", -22, 3, 2, 1);
        Platform("10 Upper palm ledge", -19, 5, 4, 1);
        Platform("11 Upper stepping stone", -13, 7, 3, 1);
        Platform("12 Upper garden", -8, 8, 5, 1);
        Platform("13 Ruin ledge", -1, 10, 4, 1);
        Platform("14 High falaj ledge", 5, 11, 4, 1);
        Platform("15 Lookout ledge", 11, 12, 4, 1);
        Platform("16 East descent ledge", 17, 11, 3, 1);
        Platform("17 Falaj A recovery bed", -14, -4, 6, 2);
        Platform("18 Falaj B recovery bed", 0, -4, 2, 4);
        Platform("19 Falaj C recovery bed", 5, -4, 2, 6);
        Platform("20 Falaj D recovery bed", 14, -4, 2, 7);
        Platform("21 Falaj E recovery bed", 19, -4, 2, 8);
        var limits = Group("06 Map edge colliders", root);
        foreach (float edge in new[] { -25.5f, 27.5f })
        {
            var wall = Group(edge < 0 ? "Left edge" : "Right edge", limits);
            wall.position = new Vector3(edge, 4, 0);
            wall.gameObject.layer = 3;
            wall.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1, 24);
        }

        // No new hazard, swimming or puzzle code: these are visual environment markers.
        Water("Falaj basin A", -14, -1.95f, 6, 0.85f);
        Water("Falaj basin B", 0, 0.05f, 2, 0.85f);
        Water("Falaj basin C", 5, 2.05f, 2, 0.85f);
        Water("Falaj basin D", 14, 3.05f, 2, 0.85f);
        Water("Falaj basin E", 19, 4.05f, 2, 0.85f);
        Water("Raised falaj strip", 7.3f, 4.84f, 6.4f, 0.18f);
        Water("Channel spill - visual only", 13.65f, 3.2f, 0.32f, 1.6f);

        Sprite background = Sprite("Al_Ain_Abstract_Maze_Background_1536x1024.png");
        Picture("Soft oasis sky", background, scenery, new Vector2(1, 6), new Vector2(94, 58), -100, new Color(0.94f, 0.96f, 0.88f));
        Picture("Jebel Hafeet - distant silhouette", Sprite("Landmark_Jebel_Hafeet_Mobile.png"), scenery,
            new Vector2(1, 6.1f), new Vector2(48, 15), -90, new Color(0.70f, 0.78f, 0.69f, 0.40f));
        Picture("Al Jahili inspired fort - destination", Sprite("Al_Jahili_Fort_Standalone_Mobile.png"), scenery,
            new Vector2(23.5f, 9.55f), new Vector2(6.8f, 3.4f), -8, Color.white);
        Picture("Hili lookout landmark", Sprite("Landmark_Hili_Tower_Front_Mobile.png"), scenery,
            new Vector2(12.9f, 14.28f), new Vector2(2.1f, 3.15f), -8, new Color(0.93f, 0.90f, 0.80f));

        Palm(-22.6f, 0, 4.8f, 0); Palm(-16.0f, 0, 5.8f, 1);
        Palm(-6.7f, 2, 4.6f, 2); Palm(-2.0f, 2, 5.2f, 0);
        Palm(8.5f, 5, 4.6f, 4); Palm(12.5f, 5, 5.2f, 1);
        Palm(17.4f, 6, 3.8f, 2); Palm(25.7f, 8, 4.4f, 0);
        Palm(-17.1f, 6, 3.6f, 3); Palm(-5.5f, 9, 3.9f, 4); Palm(6.6f, 12, 3.6f, 2);
        for (int i = 0; i < 8; i++)
            Picture("Distant palm " + i, palms[i % 6], scenery, new Vector2(-21 + i * 6.5f, -0.2f),
                new Vector2(3.1f, 4.8f), -75, new Color(0.65f, 0.76f, 0.62f, 0.32f));

        Shrub(-20, 0, 1.4f, "03_Sage_Bush_Low_512.png");
        Shrub(-14.9f, 0, 1.1f, "06_Pebble_Grass_Cluster_512.png");
        Shrub(-7.2f, 2, 1.3f, "05_Bush_Grass_Mix_512.png");
        Shrub(-0.7f, 2, 1.1f, "02_Dry_Grass_Pair_512.png");
        Shrub(3.4f, 4, 1.0f, "01_Dry_Grass_Small_512.png");
        Shrub(10.3f, 5, 1.3f, "04_Desert_Bush_Medium_512.png");
        Shrub(16.5f, 6, 1.0f, "06_Pebble_Grass_Cluster_512.png");
        Shrub(21.2f, 8, 1.1f, "05_Bush_Grass_Mix_512.png");
        Shrub(-18.5f, 6, 1.1f, "01_Dry_Grass_Small_512.png");
        Shrub(-7.0f, 9, 1.1f, "03_Sage_Bush_Low_512.png");
        Shrub(1.2f, 11, 1.0f, "06_Pebble_Grass_Cluster_512.png");

        Mark("START - Palm gate", new Vector2(-21, 0.7f));
        Mark("AREA 01 - Palm grove", new Vector2(-18, 0));
        Mark("AREA 02 - Falaj crossing", new Vector2(-4, 2));
        Mark("FUTURE PUZZLE - leave space here", new Vector2(10, 5));
        Mark("OPTIONAL ROUTE - upper garden ledges", new Vector2(-5, 9));
        Mark("END - Fort courtyard", new Vector2(23.5f, 8));

        var player = GameObject.Find("MainPlayer");
        if (player != null)
        {
            Undo.RecordObject(player.transform, "Place player at oasis entrance");
            player.transform.position = new Vector3(-21, 0.65f, 0);
            // The existing movement, dialogue and animation work remains on the same object.
            var renderer = player.GetComponent<SpriteRenderer>();
            if (renderer != null) { Undo.RecordObject(renderer, "Player above environment"); renderer.sortingOrder = 20; }
        }
        if (Camera.main != null)
        {
            Undo.RecordObject(Camera.main, "Oasis gameplay framing");
            Undo.RecordObject(Camera.main.transform, "Oasis camera start");
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 6;
            Camera.main.backgroundColor = new Color32(218, 226, 199, 255);
            Camera.main.transform.position = new Vector3(-21, 0.65f, -10);
        }
        // The user renamed SampleScene while keeping its GUID; update that existing build entry.
        var buildScenes = EditorBuildSettings.scenes;
        foreach (var entry in buildScenes)
            if (entry.path == "Assets/Scenes/SampleScene.unity" && !File.Exists(entry.path)) entry.path = ScenePath;
        EditorBuildSettings.scenes = buildScenes;

        Validate(root, player);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Undo.CollapseUndoOperations(undo);
        Selection.activeGameObject = root.gameObject;
        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.LookAt(new Vector3(1, 5, 0), Quaternion.identity, 29, true, true);
        File.WriteAllText(Marker, "oasis-v2: SaraScene oasis blockout installed. Art from the supplied ZIP only.\n");
        ExportPreviews();
        AssetDatabase.Refresh();
        Debug.Log("SARA_OASIS_READY: 22 platform groups including recovery beds, five falaj basins, palms and landmark destination. No animation code changed.");
    }

    private static void ImportArt()
    {
        foreach (string path in Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories))
        {
            string normalized = path.Replace('\\', '/');
            var importer = AssetImporter.GetAtPath(normalized) as TextureImporter;
            if (importer == null) { AssetDatabase.ImportAsset(normalized); importer = (TextureImporter)AssetImporter.GetAtPath(normalized); }
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 512;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.maxTextureSize = 2048;
            if (normalized.EndsWith("Date_Palm_Soft_Mobile_Atlas_3x2_1536.png"))
            {
                importer.spriteImportMode = SpriteImportMode.Multiple;
                var sprites = new SpriteMetaData[6];
                for (int i = 0; i < 6; i++) sprites[i] = new SpriteMetaData
                { name = "DatePalm_" + i, rect = new Rect((i % 3) * 512, (1 - i / 3) * 768, 512, 768), alignment = 0, pivot = new Vector2(0.5f, 0.5f) };
#pragma warning disable 0618
                importer.spritesheet = sprites;
#pragma warning restore 0618
            }
            importer.SaveAndReimport();
        }
    }
    private static Sprite Sprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + path);
    private static Transform Group(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(go, "Create oasis group");
        return go.transform;
    }
    private static SpriteRenderer Picture(string name, Sprite sprite, Transform parent, Vector2 position, Vector2 size, int order, Color color)
    {
        if (sprite == null) throw new Exception("Missing supplied sprite for " + name);
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(position.x, position.y, 0);
        go.transform.localScale = new Vector3(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite; renderer.sortingOrder = order; renderer.color = color; renderer.sharedMaterial = material;
        return renderer;
    }
    private static void Platform(string name, int x, int bottom, int width, int height)
    {
        var parent = Group(name, terrain);
        parent.position = new Vector3(x, bottom, 0);
        parent.gameObject.layer = 3;
        var box = parent.gameObject.AddComponent<BoxCollider2D>();
        box.size = new Vector2(width, height);
        box.offset = new Vector2(width * 0.5f, height * 0.5f);
        for (int row = 0; row < height; row++)
        for (int col = 0; col < width; col++)
        {
            bool top = row == height - 1;
            string file = top ? (col == 0 ? "05_Sand_Top_Left_512.png" : col == width - 1 ? "08_Sand_Top_Right_512.png" : col % 3 == 0 ? "07_Sand_Top_Centre_Pebbles_512.png" : "06_Sand_Top_Centre_A_512.png")
                : ((row + col) % 3 == 0 ? "10_Stone_Fill_B_512.png" : "09_Stone_Fill_A_512.png");
            Picture("Tile " + col + "," + row, Sprite("Core Tile Set_512/" + file), parent,
                new Vector2(x + col + 0.5f, bottom + row + 0.5f), new Vector2(1.008f, 1.008f), 0, Color.white);
        }
        Platforms.Add(new Rect(x, bottom, width, height));
    }
    private static void Water(string name, float x, float y, float width, float height)
    {
        var renderer = Picture(name, Sprite("Falaj_TopDown_Connection_Puzzle_Tiles_Transparent_Unity6_3/Precision_Source/Water_Texture_Seamless_256.png"),
            water, new Vector2(x + width / 2, y + height / 2), new Vector2(width, height), 3, new Color(0.7f, 0.96f, 0.93f, 0.85f));
        renderer.gameObject.layer = 4;
    }
    private static void Palm(float x, float ground, float height, int variant)
    {
        Picture("Date palm " + variant, palms[variant], plants, new Vector2(x, ground + height / 2 - 0.04f), new Vector2(height * 2 / 3, height), -5, Color.white);
    }
    private static void Shrub(float x, float ground, float width, string file)
    {
        Picture("Planting - " + file, Sprite("512 Vegitation/" + file), plants, new Vector2(x, ground + width * 0.20f), new Vector2(width, width), 5, Color.white);
    }
    private static void Mark(string name, Vector2 position)
    {
        var marker = Group(name, markers);
        marker.position = position;
        marker.tag = "EditorOnly";
    }
    private static void Validate(Transform root, GameObject player)
    {
        Physics2D.SyncTransforms();
        if (terrain.GetComponentsInChildren<BoxCollider2D>().Length != 22) throw new Exception("Unexpected platform collider count.");
        if (player == null) throw new Exception("MainPlayer missing from SaraScene.");
        var hit = Physics2D.Raycast(player.transform.position, Vector2.down, 2f, 1 << 3);
        if (hit.collider == null) throw new Exception("Player start is not above solid ground.");
        foreach (var sprite in root.GetComponentsInChildren<SpriteRenderer>())
            if (sprite.sprite == null || sprite.sharedMaterial == null) throw new Exception("Unassigned environment sprite/material.");
        // The eight primary platforms are deliberately close enough for the existing double jump.
        for (int i = 1; i < 7; i++)
        {
            Rect from = Platforms[i], to = Platforms[i + 1];
            if (to.xMin - from.xMax > 2.1f || to.yMax - from.yMax > 2.1f)
                throw new Exception("Primary route has an oversized blockout gap.");
        }
        Debug.Log("SARA_OASIS_CHECK_PASS: 22 solid platforms, valid player spawn, assigned artwork, primary gaps and rises <= 2 units.");
    }

    [MenuItem("Al Ain/Oasis/Export overview preview")]
    public static void ExportPreviews()
    {
        string output = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Logs/OasisPreviews");
        Directory.CreateDirectory(output);
        var preview = new GameObject("Temporary oasis preview camera");
        var camera = preview.AddComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(218, 226, 199, 255);
        camera.cullingMask = ~(1 << 5);
        void Render(string name, Vector2 center, float halfHeight, int width, int height)
        {
            camera.transform.position = new Vector3(center.x, center.y, -20);
            camera.orthographicSize = halfHeight;
            var render = new RenderTexture(width, height, 24);
            camera.targetTexture = render;
            camera.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = render;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            File.WriteAllBytes(Path.Combine(output, name), texture.EncodeToPNG());
            RenderTexture.active = previous;
            camera.targetTexture = null;
            Object.DestroyImmediate(texture);
            Object.DestroyImmediate(render);
        }
        Render("SaraScene-Oasis-Overview.png", new Vector2(1, 5), 16, 1920, 1080);
        Render("SaraScene-Oasis-Entrance.png", new Vector2(-16, 3), 7, 1440, 900);
        Object.DestroyImmediate(preview);
        Debug.Log("SARA_OASIS_PREVIEWS: " + output);
    }
}
