using UnityEngine;

public class TokenGate : MonoBehaviour
{
    public int requiredTokens = 6;
    public GameObject door;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (TokenManager.Instance.Collected < requiredTokens) return;

        door.SetActive(false);
    }
}
