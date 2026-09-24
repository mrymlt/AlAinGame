using UnityEngine;

public class CoinBehavior : MonoBehaviour
{
    [SerializeField] private AudioClip collectSound;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float newY = transform.position.y + Mathf.Sin(Time.time * 2f) * 0.5f * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

      private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collectSound != null)
            {
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(collectSound);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(collectSound, transform.position);
                }
            }

            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                TokenManager.Instance.Collect();
               // gameManager.onCollectPoint();
            }
            Destroy(gameObject);
        }
    }
}
