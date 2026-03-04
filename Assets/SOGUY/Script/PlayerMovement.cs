using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float rotationSpeed = 10f;

    [Header("Jump Settings")]
    public float jumpForce = 7f;

    [Header("References")]
    private Rigidbody rb;
    private Animator animator;

    // Internal
    private Vector3 moveDirection;
    private float currentSpeed;
    private bool isSprinting;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // --- Ground Check: simple and reliable ---
        isGrounded = IsOnGround();

        // --- Get Input (New Input System) ---
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        float moveX = 0f;
        float moveZ = 0f;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveZ += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveZ -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveX += 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveX -= 1f;

        moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        // --- Sprint ---
        isSprinting = keyboard.leftShiftKey.isPressed;
        currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // --- Jump (Space) ---
        if (keyboard.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Debug.Log("JUMP!");

            if (animator != null)
                animator.SetBool("jump", true);
        }

        // Reset jump animation when landed
        if (isGrounded && rb.linearVelocity.y <= 0.1f)
        {
            if (animator != null)
                animator.SetBool("jump", false);
        }

        // --- Rotation ---
        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // --- Animator: MO = is moving ---
        if (animator != null)
        {
            animator.SetBool("MO", moveDirection.magnitude >= 0.1f);
        }
    }

    void FixedUpdate()
    {
        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    // Reliable ground check using multiple short raycasts
    bool IsOnGround()
    {
        float checkDistance = 0.3f;
        Vector3 origin = transform.position + Vector3.up * 0.15f;

        // Cast 5 rays: center + 4 edges
        bool center = Physics.Raycast(origin, Vector3.down, checkDistance);
        bool front  = Physics.Raycast(origin + Vector3.forward * 0.2f, Vector3.down, checkDistance);
        bool back   = Physics.Raycast(origin - Vector3.forward * 0.2f, Vector3.down, checkDistance);
        bool left   = Physics.Raycast(origin - Vector3.right * 0.2f, Vector3.down, checkDistance);
        bool right  = Physics.Raycast(origin + Vector3.right * 0.2f, Vector3.down, checkDistance);

        return center || front || back || left || right;
    }

    void OnDrawGizmosSelected()
    {
        // Draw ground check rays in Scene view
        Gizmos.color = Color.green;
        Vector3 origin = transform.position + Vector3.up * 0.15f;
        Gizmos.DrawLine(origin, origin + Vector3.down * 0.3f);
    }
}
