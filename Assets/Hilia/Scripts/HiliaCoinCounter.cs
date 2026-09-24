using TMPro;
using UnityEngine;

// Displays this visit's coins. The existing TokenManager keeps the shared total.
public class HiliaCoinCounter : MonoBehaviour
{
    [Tooltip("The parent containing all collectible coin prefab instances in Hili.")]
    public Transform coins;
    public TMP_Text label;

    public int Total { get; private set; }
    public int Collected { get; private set; }
    int startingTokens;
    int displayed = -1;

    void Start()
    {
        Total = coins != null ? coins.GetComponentsInChildren<CoinBehavior>().Length : 0;
        startingTokens = TokenManager.Instance != null ? TokenManager.Instance.Collected : 0;
        Refresh();
    }

    void LateUpdate() => Refresh();

    void Refresh()
    {
        if (TokenManager.Instance == null || label == null) return;
        Collected = Mathf.Clamp(TokenManager.Instance.Collected - startingTokens, 0, Total);
        if (displayed == Collected) return;
        displayed = Collected;
        label.text = $"COINS  {Collected} / {Total}";
    }
}
