using UnityEngine;

public class PatrolPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 1.5f;

    private Rigidbody2D rb;
    private Transform target;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        target = pointB;
    }

    void FixedUpdate()
    {
        Vector2 newPos = Vector2.MoveTowards(rb.position, target.position, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(newPos, target.position) < 0.05f)
        {
            target = target == pointA ? pointB : pointA;
        }
    }
}
