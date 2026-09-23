using UnityEngine;

public class CoinBehavior : MonoBehaviour
{

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
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.onCollectPoint();
            }
            Destroy(gameObject);
        }
    }
}
