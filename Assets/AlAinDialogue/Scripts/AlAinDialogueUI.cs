using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AlAin.Dialogue
{
    [DisallowMultipleComponent]
    public sealed class AlAinDialogueUI : MonoBehaviour
    {
        [SerializeField] private RectTransform safeArea;
        [SerializeField] private RectTransform panel;
        [SerializeField] private GameObject modal;
        [SerializeField] private Button interactButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text promptLabel;
        [SerializeField] private TMP_Text speakerLabel;
        [SerializeField] private TMP_Text objectiveLabel;
        [SerializeField] private TMP_Text bodyLabel;
        [SerializeField] private TMP_Text nextLabel;
        [SerializeField] private Image portrait;
        [SerializeField] private Sprite defaultPortrait;
        [SerializeField] private ScrollRect bodyScroll;
        [SerializeField] private RectTransform portraitFrame;
        [SerializeField] private RectTransform textColumn;
        [SerializeField] private RectTransform footer;
        private AlAinDialogueController controller;
        private Rect lastSafeArea;
        private Vector2 lastScreenSize;
        private float lastScaleFactor;
        private static readonly Color Ink = new Color32(15, 47, 43, 255);
        private static readonly Color Sand = new Color32(244, 232, 201, 255);
        private static readonly Color Gold = new Color32(208, 171, 105, 255);

        public void Bind(AlAinDialogueController owner)
        {
            controller = owner;
            interactButton.onClick.AddListener(Interact);
            nextButton.onClick.AddListener(Interact);
            closeButton.onClick.AddListener(Close);
        }
        private void Interact() => controller.Interact();
        private void Close() => controller.Close();
        public void Hide()
        {
            if (modal != null) modal.SetActive(false);
            if (interactButton != null) interactButton.gameObject.SetActive(false);
        }
        public void ShowPrompt(AlAinDialogueTarget target)
        {
            interactButton.gameObject.SetActive(target != null);
            if (target != null) promptLabel.text = "[E]  /  TAP TO TALK\n" + target.characterName;
        }
        public int ShowLine(AlAinDialogueTarget target, string text, bool last)
        {
            interactButton.gameObject.SetActive(false);
            modal.SetActive(true);
            speakerLabel.text = target.characterName;
            objectiveLabel.text = target.objectiveTitle;
            portrait.sprite = target.portrait != null ? target.portrait : defaultPortrait;
            bodyLabel.text = text;
            bodyLabel.maxVisibleCharacters = int.MaxValue;
            nextLabel.text = last ? "Done  [E]  >" : "Continue  [E]  >";
            Canvas.ForceUpdateCanvases();
            bodyLabel.ForceMeshUpdate();
            bodyScroll.verticalNormalizedPosition = 1f;
            return bodyLabel.textInfo.characterCount;
        }
        public void SetVisibleCharacters(int count) => bodyLabel.maxVisibleCharacters = Mathf.Max(0, count);

        private void LateUpdate()
        {
            Vector2 screen = new Vector2(Screen.width, Screen.height);
            float scale = GetComponent<Canvas>().scaleFactor;
            if (screen == lastScreenSize && Screen.safeArea == lastSafeArea && Mathf.Approximately(scale, lastScaleFactor)) return;
            lastScaleFactor = scale;
            lastScreenSize = screen;
            lastSafeArea = Screen.safeArea;
            RefreshLayout(screen, lastSafeArea);
        }
        public void RefreshLayout(Vector2 screen, Rect usableArea)
        {
            if (screen.x <= 0 || screen.y <= 0 || safeArea == null) return;
            safeArea.anchorMin = usableArea.min / screen;
            safeArea.anchorMax = usableArea.max / screen;
            Canvas.ForceUpdateCanvases();
            bool compact = usableArea.width / Mathf.Max(0.01f, GetComponent<Canvas>().scaleFactor) < 850;
            float height = compact ? 420 : 320;
            panel.sizeDelta = new Vector2(-48, height);
            float portraitSize = compact ? 106 : 172;
            portraitFrame.sizeDelta = new Vector2(portraitSize, portraitSize + 36);
            textColumn.offsetMin = new Vector2(compact ? 156 : 228, 80);
            textColumn.offsetMax = new Vector2(-30, -66);
            footer.offsetMin = new Vector2(compact ? 26 : 228, 16);
        }

        // Called by the setup menu. The result is real, editable Canvas objects saved in the prefab.
        public void Build(Sprite placeholder)
        {
            defaultPortrait = placeholder;
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            safeArea = Rect("Safe Area", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            interactButton = Button("Tap to interact", safeArea, Ink, out promptLabel);
            RectTransform promptRect = (RectTransform)interactButton.transform;
            Place(promptRect, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-164, 34), new Vector2(164, 114));
            Border(interactButton.gameObject, Gold, 2);
            promptLabel.fontSize = 21;
            promptLabel.color = Sand;
            promptLabel.text = "[E]  /  TAP TO TALK\nOasis Guide";

            // Full-screen blocker prevents taps on movement controls while reading.
            modal = Rect("Dialogue modal", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            Image dim = modal.AddComponent<Image>();
            dim.color = new Color(0.015f, 0.04f, 0.035f, 0.18f);
            RectTransform modalSafe = Rect("Safe area panel", modal.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            // The panel uses its own safe-area component through the shared anchors in LateUpdate.
            modalSafe.gameObject.AddComponent<AlAinSafeArea>();
            panel = Rect("Oasis dialogue card", modalSafe, new Vector2(0, 1), Vector2.one, new Vector2(24, -344), new Vector2(-24, -24));
            panel.pivot = new Vector2(0.5f, 1);
            panel.anchoredPosition = new Vector2(0, -24);
            panel.sizeDelta = new Vector2(-48, 320);
            Image background = panel.gameObject.AddComponent<Image>();
            background.color = Ink;
            Border(panel.gameObject, Gold, 2);
            Shadow shadow = panel.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.35f);
            shadow.effectDistance = new Vector2(0, -9);

            // Gold crenellations echo the forts of Al Ain.
            for (int i = 0; i < 7; i++)
                Block("Fort crown", panel, Gold, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                    new Vector2(-46 + i * 14, -5), new Vector2(-38 + i * 14, 3));
            Label("Place", panel, "AL AIN   /   OASIS STORIES", 17, Gold,
                new Vector2(0, 1), Vector2.one, new Vector2(28, -49), new Vector2(-80, -14));
            closeButton = Button("Close dialogue", panel, Ink, out TMP_Text closeText);
            Place((RectTransform)closeButton.transform, Vector2.one, Vector2.one, new Vector2(-66, -58), new Vector2(-12, -8));
            closeText.text = "X";
            closeText.color = Sand;
            closeText.fontSize = 23;

            portraitFrame = Rect("Portrait frame", panel, new Vector2(0, 1), new Vector2(0, 1), new Vector2(28, -280), new Vector2(200, -72));
            portraitFrame.pivot = new Vector2(0, 1);
            portraitFrame.anchoredPosition = new Vector2(28, -72);
            portraitFrame.gameObject.AddComponent<Image>().color = new Color32(28, 68, 57, 255);
            Border(portraitFrame.gameObject, Gold, 1);
            RectTransform photo = Rect("Portrait - your image appears here", portraitFrame, Vector2.zero, Vector2.one, new Vector2(12, 42), new Vector2(-12, -12));
            portrait = photo.gameObject.AddComponent<Image>();
            portrait.sprite = placeholder;
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;
            TMP_Text badge = Label("Portrait caption", portraitFrame, "THE GARDEN CITY", 12, Gold,
                Vector2.zero, new Vector2(1, 0), new Vector2(4, 5), new Vector2(-4, 34));
            badge.alignment = TextAlignmentOptions.Center;

            textColumn = Rect("Dialogue content", panel, Vector2.zero, Vector2.one, new Vector2(228, 80), new Vector2(-30, -66));
            speakerLabel = Label("Character name", textColumn, "Oasis Guide", 31, Sand,
                new Vector2(0, 1), Vector2.one, new Vector2(0, -40), Vector2.zero);
            speakerLabel.fontStyle = FontStyles.Bold;
            speakerLabel.enableAutoSizing = true;
            speakerLabel.fontSizeMin = 19;
            speakerLabel.fontSizeMax = 31;
            objectiveLabel = Label("Objective title", textColumn, "Explore the oasis", 18, Gold,
                new Vector2(0, 1), Vector2.one, new Vector2(0, -70), new Vector2(0, -42));
            objectiveLabel.enableAutoSizing = true;
            objectiveLabel.fontSizeMin = 13;
            objectiveLabel.fontSizeMax = 18;

            RectTransform scroll = Rect("Scrollable dialogue", textColumn, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0, -82));
            scroll.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0);
            bodyScroll = scroll.gameObject.AddComponent<ScrollRect>();
            bodyScroll.horizontal = false;
            bodyScroll.movementType = ScrollRect.MovementType.Clamped;
            bodyScroll.scrollSensitivity = 28;
            RectTransform viewport = Rect("Viewport", scroll, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-12, 0));
            viewport.gameObject.AddComponent<RectMask2D>();
            bodyLabel = Label("Objective text", viewport, "Welcome to Al Ain!", 24, Sand,
                new Vector2(0, 1), Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform body = bodyLabel.rectTransform;
            body.pivot = new Vector2(0.5f, 1);
            bodyLabel.overflowMode = TextOverflowModes.Overflow;
            bodyLabel.raycastTarget = true;
            ContentSizeFitter fitter = body.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            bodyScroll.viewport = viewport;
            bodyScroll.content = body;
            RectTransform track = Rect("Scroll track", scroll, new Vector2(1, 0), Vector2.one, new Vector2(-5, 0), Vector2.zero);
            track.gameObject.AddComponent<Image>().color = new Color32(48, 77, 65, 255);
            Scrollbar scrollbar = track.gameObject.AddComponent<Scrollbar>();
            RectTransform handle = Rect("Handle", track, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color = Gold;
            scrollbar.handleRect = handle;
            scrollbar.targetGraphic = handleImage;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            bodyScroll.verticalScrollbar = scrollbar;
            bodyScroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

            footer = Rect("Footer", panel, Vector2.zero, new Vector2(1, 0), new Vector2(228, 16), new Vector2(-28, 64));
            Block("Fine divider", footer, new Color32(68, 96, 75, 255), new Vector2(0, 1), Vector2.one, Vector2.zero, new Vector2(0, 1));
            Label("Instructions", footer, "E / tap to reveal or continue", 14, new Color32(167, 192, 170, 255),
                Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-216, -6));
            nextButton = Button("Continue or finish", footer, Sand, out nextLabel);
            Place((RectTransform)nextButton.transform, new Vector2(1, 0), Vector2.one, new Vector2(-202, 1), new Vector2(0, -7));
            nextLabel.text = "Continue  [E]  >";
            nextLabel.fontSize = 18;
            nextLabel.fontStyle = FontStyles.Bold;
            nextLabel.color = Ink;
            Hide();
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            Place(rect, min, max, low, high);
            return rect;
        }
        private static void Place(RectTransform rect, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
        {
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = low; rect.offsetMax = high;
        }
        private static TMP_Text Label(string name, Transform parent, string text, float size, Color color,
            Vector2 min, Vector2 max, Vector2 low, Vector2 high)
        {
            var label = Rect(name, parent, min, max, low, high).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.text = text; label.fontSize = size; label.color = color;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.raycastTarget = false;
            label.richText = false;
            return label;
        }
        private static Button Button(string name, Transform parent, Color color, out TMP_Text label)
        {
            RectTransform rect = Rect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            label = Label("Label", rect, "", 22, Sand, Vector2.zero, Vector2.one, new Vector2(10, 2), new Vector2(-10, -2));
            label.alignment = TextAlignmentOptions.Center;
            return button;
        }
        private static void Block(string name, Transform parent, Color color, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
        {
            Image image = Rect(name, parent, min, max, low, high).gameObject.AddComponent<Image>();
            image.color = color; image.raycastTarget = false;
        }
        private static void Border(GameObject go, Color color, float thickness)
        {
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = color; outline.effectDistance = new Vector2(thickness, -thickness);
        }
    }
}
