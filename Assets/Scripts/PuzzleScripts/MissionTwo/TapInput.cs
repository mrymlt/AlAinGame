using UnityEngine;
using UnityEngine.InputSystem;

public static class TapInput
{
    public static bool TryGetWorldPoint(out Vector2 worldPos)
    {
        Vector2 screenPos;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPos = Mouse.current.position.ReadValue();
        }
        else
        {
            worldPos = default;
            return false;
        }

        worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        return true;
    }
}
