using System.Collections.Generic;
using UnityEngine;

namespace AlAin.Dialogue
{
    [DisallowMultipleComponent]
    public sealed class AlAinDialogueTarget : MonoBehaviour
    {
        internal static readonly List<AlAinDialogueTarget> Active = new List<AlAinDialogueTarget>();

        [Header("Dialogue content")]
        public string characterName = "Oasis Guide";
        public string objectiveTitle = "Explore the oasis";
        [TextArea(3, 8)] public string[] lines =
        {
            "Welcome to Al Ain! Your next objective is to follow the path through the palm trees and find the oasis gate."
        };
        [Tooltip("Optional picture shown inside the dialogue box. Leave empty for the fort illustration.")]
        public Sprite portrait;

        [Header("Image in the game world")]
        [Tooltip("Drop your character or objective sprite here. The placeholder is used until you add one.")]
        public Sprite worldImage;
        [SerializeField] private SpriteRenderer imageRenderer;
        [SerializeField] private Sprite placeholderImage;

        [Header("Interaction")]
        [Min(0.1f)] public float interactionDistance = 2f;
        public Vector2 interactionOffset;
        public bool HasDialogue
        {
            get
            {
                if (lines == null) return false;
                foreach (string line in lines)
                    if (!string.IsNullOrWhiteSpace(line)) return true;
                return false;
            }
        }
        public Vector2 InteractionPosition => (Vector2)transform.position + interactionOffset;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => Active.Clear();
        private void OnEnable()
        {
            if (!Active.Contains(this)) Active.Add(this);
            RefreshImage();
        }
        private void OnDisable() => Active.Remove(this);
        private void OnValidate()
        {
            interactionDistance = Mathf.Max(0.1f, interactionDistance);
            RefreshImage();
        }
        public void SetImageRenderer(SpriteRenderer renderer, Sprite placeholder)
        {
            imageRenderer = renderer;
            placeholderImage = placeholder;
            RefreshImage();
        }
        [ContextMenu("Refresh World Image")]
        public void RefreshImage()
        {
            if (imageRenderer != null)
                imageRenderer.sprite = worldImage != null ? worldImage : placeholderImage;
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.8f, 0.6f, 0.8f);
            Gizmos.DrawWireSphere(InteractionPosition, interactionDistance);
        }
    }
}
