using System.Collections;
using UnityEngine;

public class PipeTile : MonoBehaviour
{
    public Sprite drySprite;
    public Sprite[] wetFrames;
    public AudioClip correctAudioClip;
    public bool symmetric180;
    public PipePuzzle puzzle;

    private Collider2D col;
    private SpriteRenderer sr;
    private PlayerMovement player;
    private AudioSource audioSource;
    private float correctZ;
    private int currentSteps;
    private Coroutine wetAnimationRoutine;
    private bool wetState;
    private bool wasCorrect;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        player = FindObjectOfType<PlayerMovement>();
        audioSource = GetComponent<AudioSource>();
        correctZ = transform.eulerAngles.z;
    }

    void Update()
    {
        if (!TapInput.TryGetWorldPoint(out Vector2 worldPos)) return;
        if (!col.OverlapPoint(worldPos)) return;

        Rotate();
    }

    public bool IsCorrect => currentSteps == 0 || (symmetric180 && currentSteps == 2);

    public void Randomize()
    {
        currentSteps = Random.Range(1, 4);
        ApplyRotation();
        SetWet(false);
    }

    public void SetWet(bool wet)
    {
        wetState = wet;

        if (wet && !wasCorrect && correctAudioClip != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(correctAudioClip);
            }
            else
            {
                AudioSource.PlayClipAtPoint(correctAudioClip, transform.position);
            }
        }

        wasCorrect = wet;

        if (wetAnimationRoutine != null)
        {
            StopCoroutine(wetAnimationRoutine);
            wetAnimationRoutine = null;
        }

        if (!wet)
        {
            if (drySprite != null)
            {
                sr.sprite = drySprite;
            }
            return;
        }

        if (wetFrames == null || wetFrames.Length == 0)
        {
            if (drySprite != null)
            {
                sr.sprite = drySprite;
            }
            return;
        }

        wetAnimationRoutine = StartCoroutine(PlayWetAnimation());
    }

    private IEnumerator PlayWetAnimation()
    {
        int frameIndex = 0;

        while (wetState)
        {
            if (wetFrames.Length > 0)
            {
                sr.sprite = wetFrames[frameIndex % wetFrames.Length];
                frameIndex++;
            }

            yield return new WaitForSeconds(0.15f);
        }
    }

    private void Rotate()
    {
        currentSteps = (currentSteps + 1) % 4;
        ApplyRotation();
        SetWet(IsCorrect);
        if (player != null) player.TriggerInteract();
        puzzle?.Recheck();
    }

    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, correctZ - 90f * currentSteps);
    }
}
