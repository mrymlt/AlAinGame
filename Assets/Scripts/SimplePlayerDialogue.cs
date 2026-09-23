using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class DialogueLine
{
    public string speaker;
    [TextArea(2, 4)] public string text;
    public Sprite portrait;
    public DialogueLine(string name, string words) { speaker = name; text = words; }
}

// Click MainPlayer. Edit the words and optional portraits here in the Inspector.
public class SimplePlayerDialogue : MonoBehaviour, IPointerClickHandler
{
    public string characterName = "Noor";
    public Sprite dialogueImage;
    [TextArea(3, 10)] public string dialogueText = "Guide water to the palm.";
    [Header("Oasis conversation - tap to advance")]
    public DialogueLine[] dialogueLines;
    public DialogueLine[] hintLines;
    [Tooltip("Shown only when the falaj puzzle calls ShowCompletion().")]
    public DialogueLine[] completionLines;

    [HideInInspector] public GameObject dialogueBox;
    [HideInInspector] public TMP_Text nameLabel, textLabel, pageLabel, initialsLabel, objectiveLabel, nextLabel;
    [HideInInspector] public Image portraitImage;
    private PlayerMovement movement;
    private Rigidbody2D body;
    private bool isOpen, wasPaused, wasSimulated, completed;
    private Vector2 savedVelocity;
    private DialogueLine[] activeLines;
    private int index;
    public bool IsOpen => isOpen;
    public bool IsCompleted => completed;
    public int CurrentLine => index;

    private void Start()
    {
        CloseDialogue();
        UpdateObjective();
    }
    public void OnPointerClick(PointerEventData click)
    {
        if (click.button == PointerEventData.InputButton.Left) OpenDialogue();
    }
    public void OpenDialogue() => Open(completed ? completionLines : dialogueLines);
    public void ShowHint() => Open(completed ? completionLines : hintLines);
    // Connect the real puzzle's success event to this method (or OasisPuzzleHooks.CompletePuzzle).
    public void ShowCompletion()
    {
        completed = true;
        UpdateObjective();
        Open(completionLines);
    }
    private void Open(DialogueLine[] lines)
    {
        if (!isActiveAndEnabled || dialogueBox == null) return;
        if (!isOpen)
        {
            movement = GetComponent<PlayerMovement>();
            body = GetComponent<Rigidbody2D>();
            if (movement != null)
            {
                wasPaused = movement.isPaused;
                movement.ClearInteractionInput();
                movement.isPaused = true;
            }
            // Reading should not make Noor drift or fall off a platform.
            if (body != null)
            {
                wasSimulated = body.simulated;
                savedVelocity = body.linearVelocity;
                body.simulated = false;
            }
        }
        activeLines = lines != null && lines.Length > 0 ? lines : new[] { new DialogueLine(characterName, dialogueText) };
        index = 0;
        isOpen = true;
        dialogueBox.SetActive(true);
        ShowLine();
    }
    private void ShowLine()
    {
        DialogueLine line = activeLines[index] ?? new DialogueLine(characterName, "");
        nameLabel.text = string.IsNullOrWhiteSpace(line.speaker) ? characterName : line.speaker;
        textLabel.text = line.text;
        textLabel.maxVisibleCharacters = int.MaxValue;
        portraitImage.sprite = line.portrait != null ? line.portrait : dialogueImage;
        portraitImage.gameObject.SetActive(portraitImage.sprite != null);
        if (initialsLabel != null)
        {
            initialsLabel.text = nameLabel.text.StartsWith("Bu Rashid") ? "BR" : nameLabel.text.StartsWith("Noor") ? "N" : "AL";
            initialsLabel.gameObject.SetActive(portraitImage.sprite == null);
        }
        if (pageLabel != null) pageLabel.text = (index + 1) + " / " + activeLines.Length;
        if (nextLabel != null) nextLabel.text = index == activeLines.Length - 1 ? "Close" : "Next  >";
        Canvas.ForceUpdateCanvases();
        var scroll = textLabel.GetComponentInParent<ScrollRect>();
        if (scroll != null) scroll.verticalNormalizedPosition = 1;
    }
    public void NextLine()
    {
        if (!isOpen) return;
        if (++index >= activeLines.Length) { CloseDialogue(); return; }
        ShowLine();
    }
    private void UpdateObjective()
    {
        if (objectiveLabel != null)
            objectiveLabel.text = completed ? "Falaj restored. Continue toward Jebel Hafeet." : "AL AIN OASIS\nGuide water to the palm.";
    }
    public void CloseDialogue()
    {
        if (isOpen && movement != null)
        {
            movement.ClearInteractionInput();
            movement.isPaused = wasPaused;
        }
        if (isOpen && body != null)
        {
            body.simulated = wasSimulated;
            if (wasSimulated) body.linearVelocity = new Vector2(0, savedVelocity.y);
        }
        isOpen = false;
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }
    private void OnDisable() => CloseDialogue();
}
