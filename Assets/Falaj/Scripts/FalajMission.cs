using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Used only in FalajScene; keeps the existing puzzle and shared dialogue scripts intact.
[DefaultExecutionOrder(-50)]
public class FalajMission : MonoBehaviour
{
    public PipePuzzle puzzle;
    public HiliaDialogue dialogue;
    public TMP_Text objectiveLabel, instructionLabel;
    public bool IsSolved { get; private set; }

    PipeTile[] allTiles;
    bool[] originalEnabled;
    readonly HashSet<PipeTile> interactive = new HashSet<PipeTile>();
    readonly List<RaycastResult> hits = new List<RaycastResult>();
    PointerEventData pointer;
    EventSystem pointerSystem;

    void Awake()
    {
        allTiles = puzzle.GetComponentsInChildren<PipeTile>(true);
        originalEnabled = new bool[allTiles.Length];
        for (int i = 0; i < allTiles.Length; i++) originalEnabled[i] = allTiles[i].enabled;
        foreach (var tile in puzzle.tiles) interactive.Add(tile);
    }

    void Update()
    {
        bool blocked = IsSolved || dialogue.IsOpen;
        bool pressed = TapInput.TryGetWorldPoint(out Vector2 worldPoint);
        PipeTile selected = null;
        if (pressed && !blocked)
        {
            blocked = PointerHitsInterface();
            float nearest = float.PositiveInfinity;
            if (!blocked)
                foreach (var tile in puzzle.tiles)
                {
                    var col = tile.GetComponent<Collider2D>();
                    if (!tile.gameObject.activeInHierarchy || col == null || !col.enabled || !col.OverlapPoint(worldPoint)) continue;
                    float distance = ((Vector2)col.bounds.center - worldPoint).sqrMagnitude;
                    if (distance < nearest) { nearest = distance; selected = tile; }
                }
        }
        // The basin is a fixed outlet. Overlapping stone colliders receive one turn per click.
        for (int i = 0; i < allTiles.Length; i++)
            allTiles[i].enabled = originalEnabled[i] && interactive.Contains(allTiles[i]) &&
                !blocked && (!pressed || allTiles[i] == selected);
    }

    bool PointerHitsInterface()
    {
        var events = EventSystem.current;
        if (events == null) return false;
        Vector2 screenPoint = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame
            ? Touchscreen.current.primaryTouch.position.ReadValue()
            : Mouse.current.position.ReadValue();
        if (pointer == null || pointerSystem != events)
        { pointer = new PointerEventData(events); pointerSystem = events; }
        pointer.position = screenPoint;
        hits.Clear(); events.RaycastAll(pointer, hits);
        foreach (var hit in hits)
            if (hit.module is GraphicRaycaster || hit.gameObject.GetComponentInParent<HiliaDialogue>() != null) return true;
        return false;
    }

    // Connected after the existing palm-growth and water-sound callbacks in PipePuzzle.
    public void Complete()
    {
        if (IsSolved) return;
        IsSolved = true;
        foreach (var tile in allTiles) { tile.SetWet(true); tile.enabled = false; }
        objectiveLabel.text = "AL AIN OASIS\nThe falaj is flowing again.";
        instructionLabel.text = "The palm has water. Collect the remaining coins.";
        dialogue.ShowSolved();
    }

    void OnDisable()
    {
        if (allTiles == null) return;
        for (int i = 0; i < allTiles.Length; i++)
            if (allTiles[i] != null) allTiles[i].enabled = originalEnabled[i];
    }
}
