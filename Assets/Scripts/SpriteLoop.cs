using UnityEngine;

public class SpriteLoop : MonoBehaviour
{
    public Sprite[] frames;
    public float frameRate = 6f;
    public string sortingLayerName;
    public int sortingOrder;

    private SpriteRenderer sr;
    private int index;
    private float timer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (!string.IsNullOrEmpty(sortingLayerName)) sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = sortingOrder;
    }

    void Update()
    {
        if (frames.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= 1f / frameRate)
        {
            timer = 0f;
            index = (index + 1) % frames.Length;
            sr.sprite = frames[index];
        }
    }
}
