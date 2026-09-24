using UnityEngine;

public class CrumblePlatform : MonoBehaviour
{
    public float delay = 0.4f;
    public float respawnTime = 2f;

    private Collider2D col;
    private SpriteRenderer sr;
    private bool triggered;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (triggered) return;
        if (!collision.collider.CompareTag("Player")) return;

        triggered = true;
        Invoke(nameof(Crumble), delay);
    }

    void Crumble()
    {
        col.enabled = false;
        sr.enabled = false;
        Invoke(nameof(Respawn), respawnTime);
    }

    void Respawn()
    {
        col.enabled = true;
        sr.enabled = true;
        triggered = false;
    }
}
