using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LeverTrigger : MonoBehaviour
{
    public UnityEvent onPress;
    public Sprite pressedSprite;
    public AudioClip pressAudioClip;
    public float cooldown = 0.3f;
    public float pressedDuration = 0.12f;

    private Collider2D col;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Sprite originalSprite;
    private Coroutine resetSpriteRoutine;
    private bool isInRange;
    private float lastPress = -10f;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
        }
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
        ShowPressedSprite();
        PlayPressSound();
        onPress.Invoke();
    }

    private void ShowPressedSprite()
    {
        if (spriteRenderer == null || pressedSprite == null)
        {
            return;
        }

        spriteRenderer.sprite = pressedSprite;

        if (resetSpriteRoutine != null)
        {
            StopCoroutine(resetSpriteRoutine);
        }

        resetSpriteRoutine = StartCoroutine(ResetSpriteAfterDelay());
    }

    private IEnumerator ResetSpriteAfterDelay()
    {
        yield return new WaitForSeconds(pressedDuration);

        if (spriteRenderer != null && originalSprite != null)
        {
            spriteRenderer.sprite = originalSprite;
        }

        resetSpriteRoutine = null;
    }

    private void PlayPressSound()
    {
        if (pressAudioClip == null)
        {
            return;
        }

        if (audioSource != null)
        {
            audioSource.PlayOneShot(pressAudioClip);
        }
        else
        {
            AudioSource.PlayClipAtPoint(pressAudioClip, transform.position);
        }
    }
}
