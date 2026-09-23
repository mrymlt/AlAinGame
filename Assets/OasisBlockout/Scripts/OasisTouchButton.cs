using UnityEngine;
using UnityEngine.EventSystems;

public class OasisTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum Action { Left, Right, Jump }
    public PlayerMovement player;
    public Action action;
    public void OnPointerDown(PointerEventData data)
    {
        if (player == null || player.isPaused) return;
        player.currentControls = Controls.mobile;
        if (action == Action.Left) player.MobileLeftDown();
        else if (action == Action.Right) player.MobileRightDown();
        else player.MobileJump();
    }
    public void OnPointerUp(PointerEventData data) => Release();
    public void OnPointerExit(PointerEventData data) => Release();
    private void OnDisable() => Release();
    private void Release()
    {
        if (player == null) return;
        if (action == Action.Left) player.MobileLeftUp();
        if (action == Action.Right) player.MobileRightUp();
    }
}
