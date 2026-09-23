using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Attach to MainPlayer. A Physics2DRaycaster on the camera handles mouse and touch.
public class SimplePlayerDialogue : MonoBehaviour, IPointerClickHandler
{
    public string characterName = "My Character";
    public Sprite dialogueImage;
    [TextArea(3, 10)] public string dialogueText = "Welcome to Al Ain! Find the entrance to the oasis.";

    // These are connected automatically by the setup tool.
    [HideInInspector] public GameObject dialogueBox;
    [HideInInspector] public TMP_Text nameLabel;
    [HideInInspector] public TMP_Text textLabel;
    [HideInInspector] public Image portraitImage;

    private PlayerMovement movement;
    private bool isOpen;
    private bool wasPaused;

    private void Start() => CloseDialogue();

    public void OnPointerClick(PointerEventData click)
    {
        if (click.button == PointerEventData.InputButton.Left) OpenDialogue();
    }

    public void OpenDialogue()
    {
        if (isOpen || !isActiveAndEnabled || dialogueBox == null) return;
        nameLabel.text = characterName;
        textLabel.text = dialogueText;
        textLabel.maxVisibleCharacters = int.MaxValue;
        portraitImage.sprite = dialogueImage;
        portraitImage.gameObject.SetActive(dialogueImage != null);

        movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            wasPaused = movement.isPaused;
            movement.ClearInteractionInput();
            movement.isPaused = true;
        }
        isOpen = true;
        dialogueBox.SetActive(true);
        Canvas.ForceUpdateCanvases();
        ScrollRect scroll = textLabel.GetComponentInParent<ScrollRect>();
        if (scroll != null) scroll.verticalNormalizedPosition = 1;
    }

    public void CloseDialogue()
    {
        if (isOpen && movement != null)
        {
            movement.ClearInteractionInput();
            movement.isPaused = wasPaused;
        }
        isOpen = false;
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }

    private void OnDisable() => CloseDialogue();
}
