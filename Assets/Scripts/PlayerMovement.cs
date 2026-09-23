using UnityEngine;
using UnityEngine.InputSystem;

public enum Controls { mobile, pc }

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float playerSpeed = 5f;

    [Header("Jump")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public float jumpForce = 6.5f;
    public int maxJumps = 2;
    public float fallGravityMultiplier = 2.2f;
    public float lowJumpGravityMultiplier = 1.8f;

    [Header("Feel")]

    public float coyoteTime = 0.1f;

    public float jumpBufferTime = 0.1f;

    public float postJumpGroundIgnoreTime = 0.1f;

    [Header("Controls")]
    public Controls currentControls = Controls.pc;
    public bool autoDetectMobile = true;

    private Rigidbody2D rb;
    private Collider2D[] ownColliders;
    private readonly Collider2D[] groundHits = new Collider2D[12];

    private bool isGrounded;
    private int jumpsRemaining;

    private float coyoteTimer;
    private float jumpBufferTimer;
    private float postJumpIgnoreTimer;

    private Vector2 moveInput;
    private bool jumpQueued;

    // Mobile button state (set by UI events)
    private bool mobileLeftHeld;
    private bool mobileRightHeld;

    public bool isPaused = false;
    public Transform cameraTarget;

    public float cameraFollowSpeed = 8f;

    public float interactAnimDuration = 0.3f;
    private float interactTimer;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ownColliders = GetComponents<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        maxJumps = Mathf.Max(1, maxJumps);
        jumpsRemaining = maxJumps;

        animator.applyRootMotion = false;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (autoDetectMobile && IsMobileRuntime())
        {
            currentControls = Controls.mobile;
        }
    }

    private bool IsMobileRuntime()
    {
        // Application.isMobilePlatform misses mobile browsers running the WebGL build,

        if (Application.isMobilePlatform) return true;
        return Touchscreen.current != null && Mouse.current == null;
    }

    void Update()
    {
        if (isPaused)
        {
            moveInput = Vector2.zero;
            jumpQueued = false;
            return;
        }

        ReadControls();

        if (jumpQueued)
        {
            jumpBufferTimer = jumpBufferTime;
            jumpQueued = false;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }


        if (jumpBufferTimer > 0f && jumpsRemaining > 0)
        {
            TryJump();
            jumpBufferTimer = 0f;
        }

        if (interactTimer > 0f)
        {
            interactTimer -= Time.deltaTime;
        }

        UpdateFacing();
        UpdateAnimator();
    }

    private void UpdateFacing()
    {
        if (moveInput.x > 0.01f)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput.x < -0.01f)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void UpdateAnimator()
    {
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isWalking", isGrounded && Mathf.Abs(moveInput.x) > 0.01f);
        animator.SetBool("isInteracting", interactTimer > 0f);
        animator.SetFloat("y_velocity", rb.linearVelocity.y);
    }

    public void TriggerInteract()
    {
        interactTimer = interactAnimDuration;
    }

    void FixedUpdate()
    {
        if (isPaused)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        UpdateGroundedState();

        rb.linearVelocity = new Vector2(moveInput.x * playerSpeed, rb.linearVelocity.y);


        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !IsJumpHeld())
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    private void UpdateGroundedState()
    {
        if (postJumpIgnoreTimer > 0f)
        {
            postJumpIgnoreTimer -= Time.fixedDeltaTime;
            isGrounded = false;
        }
        else
        {
            isGrounded = CheckGroundOverlap();
        }

        if (isGrounded)
        {

            jumpsRemaining = maxJumps;
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.fixedDeltaTime;
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
            return;
        }


        if (Keyboard.current != null)
        {
            float x = 0f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;

            moveInput = new Vector2(Mathf.Clamp(x, -1f, 1f), 0f);

            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                jumpQueued = true;
            }
        }
    }

    private void TryJump()
    {

        bool usingCoyote = !isGrounded && coyoteTimer > 0f;
        if (jumpsRemaining <= 0)
        {
            return;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpsRemaining--;

        if (isGrounded || usingCoyote)
        {
            coyoteTimer = 0f;
        }

        postJumpIgnoreTimer = postJumpGroundIgnoreTime;
        isGrounded = false;
    }

    public void OnMove(InputValue value)
    {
        var v = value.Get<Vector2>();
        if (currentControls == Controls.pc)
        {
            moveInput = new Vector2(v.x, 0f);
        }
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpQueued = true;
        }
    }

    // Mobile UI
    public void MobileLeftDown() => mobileLeftHeld = true;
    public void MobileLeftUp() => mobileLeftHeld = false;
    public void MobileRightDown() => mobileRightHeld = true;
    public void MobileRightUp() => mobileRightHeld = false;
    public void MobileJump()
    {
        if (currentControls == Controls.mobile)
        {
            jumpQueued = true;
        }
    }

    private bool CheckGroundOverlap()
    {
        Vector2 checkPosition = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position;

        int hitCount = Physics2D.OverlapCircleNonAlloc(checkPosition, groundCheckRadius, groundHits, groundLayer);
        if (HasValidGroundHit(hitCount))
        {
            return true;
        }


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

    // Clear held touch buttons and queued jumps when dialogue takes or releases control.
    public void ClearInteractionInput()
    {
        moveInput = Vector2.zero;
        jumpQueued = false;
        jumpBufferTimer = 0f;
        mobileLeftHeld = false;
        mobileRightHeld = false;
        interactTimer = 0f;
    }
    void LateUpdate()
    {
        if (cameraTarget == null) return;

        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, cameraTarget.position.z);


        float t = 1f - Mathf.Exp(-cameraFollowSpeed * Time.deltaTime);
        cameraTarget.position = Vector3.Lerp(cameraTarget.position, targetPosition, t);
    }
}
