using UnityEngine;

namespace AlAin.Dialogue
{
    public sealed class AlAinSafeArea : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            var rect = (RectTransform)transform;
            Vector2 screen = new Vector2(Screen.width, Screen.height);
            rect.anchorMin = Screen.safeArea.min / screen;
            rect.anchorMax = Screen.safeArea.max / screen;
        }
    }
}
