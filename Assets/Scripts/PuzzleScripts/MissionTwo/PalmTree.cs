using UnityEngine;

public class PalmTree : MonoBehaviour
{
    public Color aliveColor = Color.white;
    public float grownScaleY = 1.6f;
    public float growSpeed = 1.5f;
    public AudioClip growAudioClip;

    private SpriteRenderer sr;
    private AudioSource audioSource;
    private float startScaleY;
    private float targetScaleY;
    private float localBottomY;
    private float bottomWorldY;
    private bool growing;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        startScaleY = transform.localScale.y;
        targetScaleY = startScaleY;
        localBottomY = sr.sprite.bounds.min.y;
        bottomWorldY = transform.position.y + localBottomY * startScaleY;
    }

    public void Revive()
    {
        if (growing) return;

        sr.color = aliveColor;
        targetScaleY = startScaleY * grownScaleY;
        growing = true;

        if (growAudioClip == null) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(growAudioClip);
        }
        else
        {
            AudioSource.PlayClipAtPoint(growAudioClip, transform.position);
        }
    }

    void Update()
    {
        if (!growing) return;

        float newScaleY = Mathf.MoveTowards(transform.localScale.y, targetScaleY, growSpeed * Time.deltaTime);
        transform.localScale = new Vector3(transform.localScale.x, newScaleY, transform.localScale.z);
        transform.position = new Vector3(transform.position.x, bottomWorldY - localBottomY * newScaleY, transform.position.z);

        if (Mathf.Approximately(newScaleY, targetScaleY))
        {
            growing = false;
        }
    }
}
