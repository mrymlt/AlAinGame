using UnityEngine;
using UnityEngine.InputSystem;

public enum Controls { mobile, pc }

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float playerSpeed = 5f;
    public float runSpeedMultiplier = 1.6f;

    [Header("Jump")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public float jumpForce = 6.5f;
    public int maxJumps = 2;
    public float fallGravityMultiplier = 2.2f;
    public float lowJumpGravityMultiplier = 1.8f;

    [Header("Controls")]
    public Controls currentControls = Controls.pc;
    public bool autoDetectMobile = true;

    private Rigidbody2D rb;
    private Collider2D[] ownColliders;
    private readonly Collider2D[] groundHits = new Collider2D[12];
    private bool isGroundedBool;
    private bool wasGroundedLastFrame;
    private int jumpsRemaining;
    private Vector2 moveInput;
    private bool runHeld;
    private bool jumpQueued;

    // Mobile button state (set by UI events)
    private bool mobileLeftHeld;
    private bool mobileRightHeld;
    private bool mobileRunHeld;

    public bool isPaused = false;
    public GameObject camera;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ownColliders = GetComponents<Collider2D>();
        maxJumps = Mathf.Max(1, maxJumps);
        jumpsRemaining = maxJumps;

        if (autoDetectMobile && Application.isMobilePlatform)
        {
            currentControls = Controls.mobile;
        }
    }

    void Update()
    {
        if (isPaused)
        {
            moveInput = Vector2.zero;
            runHeld = false;
            jumpQueued = false;
            return;
        }

        ReadControls();
        isGroundedBool = IsGrounded();

        if (isGroundedBool && !wasGroundedLastFrame)
        {
            jumpsRemaining = maxJumps;
        }
        wasGroundedLastFrame = isGroundedBool;

        if (jumpQueued)
        {
            TryJump();
            jumpQueued = false;
        }
    }

    void FixedUpdate()
    {
        if (isPaused)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float speed = playerSpeed * (runHeld ? runSpeedMultiplier : 1f);
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);

        // Sharpen jump arc: stronger gravity while falling or when jump is released.
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !IsJumpHeld())
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    private void ReadControls()
    {
        if (currentControls == Controls.mobile)
        {
            float x = 0f;
            if (mobileLeftHeld) x -= 1f;
            if (mobileRightHeld) x += 1f;

            moveInput = new Vector2(Mathf.Clamp(x, -1f, 1f), 0f);
            runHeld = mobileRunHeld;
            return;
        }

        // Keyboard fallback for PC/Web builds.
        if (Keyboard.current != null)
        {
            float x = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;

            moveInput = new Vector2(Mathf.Clamp(x, -1f, 1f), 0f);
            runHeld = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;

            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                jumpQueued = true;
            }
        }
    }

    private void TryJump()
    {
        if (jumpsRemaining <= 0)
        {
            return;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpsRemaining--;
    }


    public void OnMove(InputValue value)
    {
        var v = value.Get<Vector2>();
        if (currentControls == Controls.pc)
        {
            moveInput = new Vector2(v.x, 0f);
        }
    }

    public void OnRun(InputValue value)
    {
        if (currentControls == Controls.pc)
        {
            runHeld = value.isPressed;
        }
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpQueued = true;
        }
    }

    // Mobile UI button hooks
    public void MobileLeftDown() => mobileLeftHeld = true;
    public void MobileLeftUp() => mobileLeftHeld = false;
    public void MobileRightDown() => mobileRightHeld = true;
    public void MobileRightUp() => mobileRightHeld = false;
    public void MobileRunDown() => mobileRunHeld = true;
    public void MobileRunUp() => mobileRunHeld = false;
    public void MobileJump()
    {
        if (currentControls == Controls.mobile)
        {
            jumpQueued = true;
        }
    }

    private bool IsGrounded()
    {
        Vector2 checkPosition = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position;

        // Primary check uses assigned ground layer.
        int hitCount = Physics2D.OverlapCircleNonAlloc(checkPosition, groundCheckRadius, groundHits, groundLayer);
        if (HasValidGroundHit(hitCount))
        {
            return true;
        }

        // Fallback when groundLayer is not configured: check any collider except self/trigger.
        if (groundLayer.value == 0)
        {
            hitCount = Physics2D.OverlapCircleNonAlloc(checkPosition, groundCheckRadius, groundHits);
            return HasValidGroundHit(hitCount);
        }

        return false;
    }

    private bool HasValidGroundHit(int hitCount)
    {
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = groundHits[i];
            if (hit == null || hit.isTrigger)
            {
                continue;
            }

            bool isOwnCollider = false;
            for (int c = 0; c < ownColliders.Length; c++)
            {
                if (hit == ownColliders[c])
                {
                    isOwnCollider = true;
                    break;
                }
            }

            if (!isOwnCollider)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsJumpHeld()
    {
        if (currentControls == Controls.mobile)
        {
            return false;
        }

        if (Keyboard.current == null)
        {
            return false;
        }

        return Keyboard.current.spaceKey.isPressed || Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed;
    }

    void LateUpdate()
    {
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, camera.transform.position.z);
        camera.transform.position = Vector3.Lerp(camera.transform.position, targetPosition, Time.deltaTime * 5f);
    }
}
