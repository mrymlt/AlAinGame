using UnityEngine;
using UnityEngine.InputSystem;

namespace AlAin.Dialogue
{
    [DefaultExecutionOrder(-50)]
    [DisallowMultipleComponent]
    public sealed class AlAinDialogueController : MonoBehaviour
    {
        public PlayerMovement player;
        public AlAinDialogueUI view;
        [Min(0)] public float charactersPerSecond = 42f;
        public bool IsOpen => current != null;
        public AlAinDialogueTarget Nearest { get; private set; }
        private AlAinDialogueTarget current;
        private PlayerMovement pausedPlayer;
        private bool previousPause;
        private int lineIndex;
        private float visibleCharacters;
        private int totalCharacters;
        private int lastActionFrame = -1;

        private void Start()
        {
            if (player == null) player = FindFirstObjectByType<PlayerMovement>();
            if (view == null) view = GetComponentInChildren<AlAinDialogueUI>(true);
            if (view == null || player == null)
            {
                Debug.LogError("Al Ain dialogue needs a PlayerMovement and a Dialogue UI assigned.", this);
                enabled = false;
                return;
            }
            view.Bind(this);
            view.Hide();
        }
        private void Update()
        {
            if (view == null || player == null) { Close(); return; }
            // A destroyed or disabled speaker should never leave the player paused.
            if (pausedPlayer != null && (current == null || !current.isActiveAndEnabled || !player.isActiveAndEnabled))
                Close();

            if (IsOpen)
            {
                visibleCharacters += Time.unscaledDeltaTime * charactersPerSecond;
                view.SetVisibleCharacters(charactersPerSecond <= 0 ? totalCharacters : (int)visibleCharacters);
                if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Close();
                else if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) Interact();
                return;
            }

            Nearest = FindNearest();
            view.ShowPrompt(Nearest);
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) Interact();
        }
        private AlAinDialogueTarget FindNearest()
        {
            if (!player.isActiveAndEnabled || player.isPaused) return null;
            AlAinDialogueTarget nearest = null;
            float best = float.PositiveInfinity;
            foreach (AlAinDialogueTarget target in AlAinDialogueTarget.Active)
            {
                if (target == null || !target.isActiveAndEnabled || !target.HasDialogue) continue;
                float distance = ((Vector2)player.transform.position - target.InteractionPosition).sqrMagnitude;
                if (distance <= target.interactionDistance * target.interactionDistance && distance < best)
                {
                    nearest = target;
                    best = distance;
                }
            }
            return nearest;
        }
        // Both the E key and the on-screen buttons use exactly the same interaction path.
        public void Interact()
        {
            if (!isActiveAndEnabled || player == null || view == null || lastActionFrame == Time.frameCount) return;
            lastActionFrame = Time.frameCount;
            if (!IsOpen)
            {
                Nearest = FindNearest(); // Recheck range when a touch/click arrives.
                if (Nearest == null) return;
                current = Nearest;
                pausedPlayer = player;
                previousPause = player.isPaused;
                player.ClearInteractionInput();
                player.isPaused = true;
                lineIndex = -1;
                NextLine();
            }
            else if (charactersPerSecond > 0 && visibleCharacters < totalCharacters)
            {
                visibleCharacters = totalCharacters;
                view.SetVisibleCharacters(totalCharacters);
            }
            else NextLine();
        }
        private void NextLine()
        {
            string[] lines = current.lines;
            do { lineIndex++; }
            while (lines != null && lineIndex < lines.Length && string.IsNullOrWhiteSpace(lines[lineIndex]));
            if (lines == null || lineIndex >= lines.Length) { Close(); return; }

            bool last = true;
            for (int i = lineIndex + 1; i < lines.Length; i++)
                if (!string.IsNullOrWhiteSpace(lines[i])) { last = false; break; }
            totalCharacters = view.ShowLine(current, lines[lineIndex], last);
            visibleCharacters = charactersPerSecond <= 0 ? totalCharacters : 0;
            view.SetVisibleCharacters((int)visibleCharacters);
        }
        public void Close()
        {
            if (pausedPlayer != null)
            {
                pausedPlayer.ClearInteractionInput();
                pausedPlayer.isPaused = previousPause;
            }
            pausedPlayer = null;
            current = null;
            Nearest = null;
            if (view != null) view.Hide();
        }
        private void OnDisable() => Close();
    }
}
