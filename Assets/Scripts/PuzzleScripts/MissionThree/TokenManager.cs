using UnityEngine;

public class TokenManager : MonoBehaviour
{
    public static TokenManager Instance;

    public int Collected { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Collect()
    {
        Collected++;
    }
}
