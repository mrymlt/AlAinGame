using UnityEngine;

public class DoorUnlock : MonoBehaviour
{
    public Vector2 openOffset = new Vector2(0f, 3f);
    public float moveSpeed = 2f;

    private Collider2D col;
    private Vector2 targetPosition;
    private bool unlocking;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        targetPosition = transform.position;
    }

    public void Unlock()
    {
        if (unlocking) return;

        unlocking = true;
        col.enabled = false;
        targetPosition = (Vector2)transform.position + openOffset;
    }

    void Update()
    {
        if (!unlocking) return;

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
}
