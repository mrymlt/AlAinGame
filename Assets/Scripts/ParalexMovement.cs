using UnityEngine;

public class ParalexMovement : MonoBehaviour
{
    [Header("Background Mountains")]
    [SerializeField] private Transform farMountain;
    [SerializeField] private Transform nearMountain;

    [Header("Parallax Strength")]
    [SerializeField, Range(0f, 1f)] private float farParallax = 0.15f;
    [SerializeField, Range(0f, 1f)] private float nearParallax = 0.35f;

    private Vector3 lastPlayerPosition;

    void Start()
    {
        lastPlayerPosition = transform.position;
    }

    void LateUpdate()
    {
        float deltaX = transform.position.x - lastPlayerPosition.x;

        if (farMountain != null)
        {
            farMountain.position += new Vector3(deltaX * farParallax, 0f, 0f);
        }

        if (nearMountain != null)
        {
            nearMountain.position += new Vector3(deltaX * nearParallax, 0f, 0f);
        }

        lastPlayerPosition = transform.position;
    }
}
