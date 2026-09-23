using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LevelTrigger : MonoBehaviour
{
    public UnityEvent onPress;
    public float cooldown = 0.3f;

    private Collider2D col;
    private bool isInRange;
    private float lastPress = -10f;

    void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isInRange = false;
    }

    void Update()
    {
        if (!isInRange) return;
        if (Time.time - lastPress < cooldown) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Press();
            return;
        }

        if (WasTappedThisFrame())
        {
            Press();
        }
    }

    private bool WasTappedThisFrame()
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
            return false;
        }

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        return col.OverlapPoint(worldPos);
    }

    private void Press()
    {
        lastPress = Time.time;
        onPress.Invoke();
    }
}
