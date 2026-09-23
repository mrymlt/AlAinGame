using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class OasisPhoneLayout : MonoBehaviour
{
    public RectTransform safeArea, card, objective;
    public PlayerMovement player;
    private void Update()
    {
        var key = Keyboard.current;
        if (player != null && player.currentControls == Controls.mobile && key != null &&
            (key.aKey.isPressed || key.dKey.isPressed || key.leftArrowKey.isPressed || key.rightArrowKey.isPressed ||
             key.spaceKey.wasPressedThisFrame || key.wKey.wasPressedThisFrame || key.upArrowKey.wasPressedThisFrame))
        {
            player.ClearInteractionInput();
            player.currentControls = Controls.pc;
        }
    }
    private void LateUpdate()
    {
        if (Screen.width <= 0 || Screen.height <= 0) return;
        var screen = new Vector2(Screen.width, Screen.height);
        safeArea.anchorMin = Screen.safeArea.min / screen;
        safeArea.anchorMax = Screen.safeArea.max / screen;
        Fit(Screen.safeArea.width / GetComponent<Canvas>().scaleFactor);
    }
    public void Fit(float width)
    {
        card.sizeDelta = new Vector2(Mathf.Clamp(width - 24, 240, 640), 218);
        objective.sizeDelta = new Vector2(Mathf.Clamp(width - 24, 240, 640), 52);
    }
}
