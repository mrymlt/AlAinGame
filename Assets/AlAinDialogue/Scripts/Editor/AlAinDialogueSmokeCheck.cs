using System;
using System.Collections.Generic;
using System.IO;
using AlAin.Dialogue;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Editor-only integration check. Does not save its Play Mode changes.
[InitializeOnLoad]
public static class AlAinDialogueSmokeCheck
{
    private const string Key = "AlAinDialogueSmokeRunning";
    private static Queue<Action> steps;
    private static double nextTime;
    private static AlAinDialogueController controller;
    private static AlAinDialogueTarget target;
    private static AlAinDialogueTarget competing;
    private static PlayerMovement player;
    private static TMP_Text body;
    private static Keyboard keyboard;
    private static Touchscreen touch;
    private static Vector2 touchPosition;
    private static bool finished;
    private static InputSettings originalSettings;

    static AlAinDialogueSmokeCheck()
    {
        EditorApplication.update += Tick;
    }
    [MenuItem("Al Ain/Dialogue/Run Play Mode smoke check")]
    public static void Run()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        SessionState.SetBool(Key, true);
        steps = null;
        finished = false;
        EditorApplication.EnterPlaymode();
    }
    private static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        Debug.Log("DIALOGUE_CHECK_PASS: " + message);
    }
    private static void Keys(params UnityEngine.InputSystem.Key[] keys)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
    }
    private static void Touch(UnityEngine.InputSystem.TouchPhase phase)
    {
        InputSystem.QueueStateEvent(touch, new TouchState { touchId = 1, phase = phase, position = touchPosition });
    }
    private static Button ButtonNamed(string name)
    {
        foreach (Button button in controller.view.GetComponentsInChildren<Button>(true))
            if (button.name == name) return button;
        throw new Exception("Missing button " + name);
    }
    private static void Begin()
    {
        controller = Object.FindFirstObjectByType<AlAinDialogueController>();
        player = Object.FindFirstObjectByType<PlayerMovement>();
        target = Object.FindFirstObjectByType<AlAinDialogueTarget>();
        Check(controller != null && player != null && target != null, "Scene references are present");
        originalSettings = InputSystem.settings;
        InputSystem.settings = Object.Instantiate(originalSettings);
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        Application.runInBackground = true;
        keyboard = InputSystem.AddDevice<Keyboard>();
        touch = InputSystem.AddDevice<Touchscreen>();
        player.autoDetectMobile = false;
        player.currentControls = Controls.pc;
        // Keep the distance checks deterministic; only affects the temporary Play Mode instance.
        player.GetComponent<Rigidbody2D>().simulated = false;
        foreach (TMP_Text text in controller.view.GetComponentsInChildren<TMP_Text>(true))
            if (text.name == "Objective text") body = text;
        steps = new Queue<Action>(new Action[]
        {
            () => { player.transform.position = target.transform.position + Vector3.right * 20; },
            () => { Check(controller.Nearest == null, "No interaction outside radius"); Keys(UnityEngine.InputSystem.Key.E); },
            () => { Check(!controller.IsOpen, "E outside radius does nothing"); Keys(); player.transform.position = target.transform.position; },
            () => { Check(controller.Nearest == target, "Nearby target is selected"); Keys(UnityEngine.InputSystem.Key.E); },
            () => { Check(controller.IsOpen && player.isPaused, "E opens dialogue and pauses movement"); Keys(); },
            () => { Keys(UnityEngine.InputSystem.Key.E); },
            () => { Check(body.maxVisibleCharacters >= body.textInfo.characterCount, "E reveals the current line"); Keys(); },
            () => { Keys(UnityEngine.InputSystem.Key.E); },
            () => { Check(body.text == target.lines[1], "E advances to the next line"); Keys(); },
            () => { Keys(UnityEngine.InputSystem.Key.Escape); },
            () => { Check(!controller.IsOpen && !player.isPaused, "Escape closes and restores movement"); Keys(); },
            () => {
                Button button = ButtonNamed("Tap to interact");
                Check(button.gameObject.activeInHierarchy, "Touch prompt is visible in range");
                touchPosition = RectTransformUtility.WorldToScreenPoint(null, button.transform.position);
                Touch(UnityEngine.InputSystem.TouchPhase.Began);
            },
            () => Touch(UnityEngine.InputSystem.TouchPhase.Ended),
            () => { Check(controller.IsOpen, "Actual touch input opens dialogue via the UI module"); controller.Close(); },
            () => {
                competing = new GameObject("Temporary closer target").AddComponent<AlAinDialogueTarget>();
                competing.transform.position = player.transform.position;
                target.transform.position += Vector3.right;
            },
            () => { Check(controller.Nearest == competing, "Overlapping ranges select the nearest target"); competing.lines = new[] { " ", "" }; },
            () => { Check(controller.Nearest == target, "Empty dialogue is skipped"); Object.Destroy(competing.gameObject); controller.Interact(); },
            () => { target.gameObject.SetActive(false); },
            () => { Check(!controller.IsOpen && !player.isPaused, "Disabling a speaker restores control"); target.gameObject.SetActive(true); },
            () => { controller.charactersPerSecond = 0; controller.Interact(); },
            () => { Check(body.maxVisibleCharacters >= body.textInfo.characterCount, "Instant text mode works"); controller.Interact(); },
            () => { controller.Interact(); },
            () => { Check(!controller.IsOpen && !player.isPaused, "Finishing the last line restores control"); controller.Interact(); },
            () => { controller.enabled = false; Check(!player.isPaused, "Disabling the dialogue system restores control"); controller.enabled = true; },
            () => {
                player.transform.position = target.transform.position;
                controller.Interact();
            },
            () => {
                Canvas.ForceUpdateCanvases();
                Check(body.rectTransform.rect.width > 100 && body.rectTransform.rect.height > 20, "Dialogue text has usable layout");
                string output = OutputDirectory();
                Directory.CreateDirectory(output);
                ScreenCapture.CaptureScreenshot(Path.Combine(output, "al-ain-dialogue-preview.png"));
                Debug.Log("DIALOGUE_SCREEN_SIZE: " + Screen.width + "x" + Screen.height);
            },
            () => { },
            () => {
                Debug.Log("AL_AIN_SMOKE_SUCCESS");
                finished = true;
                InputSystem.settings = originalSettings;
                SessionState.SetBool(Key, false);
                if (Application.isBatchMode) EditorApplication.Exit(0);
                else EditorApplication.ExitPlaymode();
            }
        });
    }
    private static string OutputDirectory()
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == "-dialogueTestOutput") return args[i + 1];
        return Path.Combine(Application.temporaryCachePath, "AlAinDialogueChecks");
    }
    public static void RenderPreviews()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        var view = Object.FindFirstObjectByType<AlAinDialogueUI>();
        var speaker = Object.FindFirstObjectByType<AlAinDialogueTarget>();
        var canvas = view.GetComponent<Canvas>();
        canvas.gameObject.layer = 5;
        view.GetComponent<CanvasScaler>().enabled = false;
        var cameraObject = new GameObject("Temporary UI preview camera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(51, 76, 65, 255);
        camera.cullingMask = 1 << 5;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1;
        Directory.CreateDirectory(OutputDirectory());
        void Render(int width, int height, string name)
        {
            var render = new RenderTexture(width, height, 24);
            camera.targetTexture = render;
            canvas.scaleFactor = Mathf.Sqrt((width / 1280f) * (height / 720f));
            Canvas.ForceUpdateCanvases();
            view.RefreshLayout(new Vector2(width, height), new Rect(0, 0, width, height));
            view.ShowLine(speaker, speaker.lines[0], false);
            view.SetVisibleCharacters(int.MaxValue);
            Canvas.ForceUpdateCanvases();
            camera.Render();
            // Camera-backed canvas dimensions settle on the first render in batch Edit Mode.
            view.RefreshLayout(new Vector2(width, height), new Rect(0, 0, width, height));
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = render;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            var colors = texture.GetPixels32();
            int changed = 0;
            for (int i = 1; i < colors.Length; i++)
                if (!colors[i].Equals(colors[0])) changed++;
            Check(changed > 1000, "Preview contains rendered UI: " + name);
            File.WriteAllBytes(Path.Combine(OutputDirectory(), name), texture.EncodeToPNG());
            RenderTexture.active = previous;
            camera.targetTexture = null;
            Object.DestroyImmediate(texture);
            Object.DestroyImmediate(render);
        }
        Render(1280, 720, "al-ain-dialogue-preview.png");
        Render(720, 1280, "al-ain-dialogue-phone.png");
        Debug.Log("AL_AIN_PREVIEW_SUCCESS");
    }
    private static void Tick()
    {
        if (!SessionState.GetBool(Key, false) || !EditorApplication.isPlaying || EditorApplication.isCompiling || finished) return;
        if (Time.frameCount < 8 || EditorApplication.timeSinceStartup < nextTime) return;
        nextTime = EditorApplication.timeSinceStartup + 0.4;
        try
        {
            if (steps == null) Begin();
            else if (steps.Count > 0) steps.Dequeue()();
        }
        catch (Exception exception)
        {
            SessionState.SetBool(Key, false);
            if (originalSettings != null) InputSystem.settings = originalSettings;
            Debug.LogException(exception);
            if (Application.isBatchMode) EditorApplication.Exit(1);
            else EditorApplication.ExitPlaymode();
        }
    }
}
