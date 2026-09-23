using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
public enum Controls { mobile,pc }

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed = 5f; 
    public LayerMask groundLayer; 
    public Transform groundCheck;

    private Rigidbody2D rb;
    private bool isGroundedBool = false;
    private bool canDoubleJump = false;
    private Animator animator;
    public bool isPaused = false;
    public GameObject camera;

    private PlayerInput playerInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        playerInput = gameObject.GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        isGroundedBool = IsGrounded();
 

    }


    public void OnMove(InputValue value)
    {
        var v = value.Get<Vector2>();
        rb.linearVelocity = new Vector2(v.x * playerSpeed, rb.linearVelocity.y);
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGroundedBool)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, playerSpeed);
            canDoubleJump = true;
        }
        else if (value.isPressed && canDoubleJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, playerSpeed);
            canDoubleJump = false;
        }
    }

    private bool IsGrounded()
    {
        float rayLength = 0.25f;
        Vector2 rayOrigin = new Vector2(groundCheck.transform.position.x, groundCheck.transform.position.y - 0.1f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, groundLayer);
        return hit.collider != null;
    }

    void LateUpdate()
    {
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, camera.transform.position.z);
        camera.transform.position = Vector3.Lerp(camera.transform.position, targetPosition, Time.deltaTime * 5f);
    }
}
