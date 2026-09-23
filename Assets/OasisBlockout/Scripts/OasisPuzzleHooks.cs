using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// Add the puzzle's success callback here; clicking the palm only shows a hint.
public class OasisPuzzleHooks : MonoBehaviour, IPointerClickHandler
{
    public SimplePlayerDialogue dialogue;
    public SpriteRenderer palm;
    public Sprite healthyPalm;
    [HideInInspector] public float healthyPalmVerticalOffset;
    public UnityEvent onCompleted;
    private bool solved;
    public void OnPointerClick(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left && dialogue != null) dialogue.ShowHint();
    }
    public void CompletePuzzle()
    {
        if (solved) return;
        solved = true;
        if (palm != null)
        {
            palm.sprite = healthyPalm;
            palm.color = Color.white;
            palm.transform.localPosition += Vector3.up * healthyPalmVerticalOffset;
        }
        onCompleted?.Invoke();
        if (dialogue != null) dialogue.ShowCompletion();
    }
}
