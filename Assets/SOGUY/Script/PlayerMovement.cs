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
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Ground Check")]
    public Transform groundCheck;       // ลาก Empty GameObject ที่อยู่ใต้เท้าตัวละครมาใส่

    [Header("References")]
    private Rigidbody rb;
    private Animator animator;

    // Internal
    private Vector2 inputVector;
    private Vector3 moveDirection;
    private float currentSpeed;
    private bool isSprinting;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Lock Rigidbody rotation so physics doesn't rotate the player
        rb.freezeRotation = true;

        // If no groundCheck assigned, create one at feet position
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.parent = transform;
            gc.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            groundCheck = gc.transform;
        }

        // If no ground layer set, default to everything except player
        if (groundLayer == 0)
        {
            groundLayer = ~LayerMask.GetMask("Player");
        }
    }

    void Update()
    {
        // --- Ground Check ---
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // --- Get Input (New Input System) ---
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        float moveX = 0f;
        float moveZ = 0f;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveZ += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveZ -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveX += 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveX -= 1f;

        inputVector = new Vector2(moveX, moveZ);

        // Build movement direction and normalize to prevent faster diagonal movement
        moveDirection = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

        // --- Sprint ---
        isSprinting = keyboard.leftShiftKey.isPressed;
        currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // --- Jump (Space) ---
        if (keyboard.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // --- Rotation: face the direction of movement ---
        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // --- Update Animator ---
        if (animator != null)
        {
            float speedPercent = moveDirection.magnitude * (isSprinting ? 1f : 0.5f);
            animator.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VerticalSpeed", rb.linearVelocity.y);
        }
    }

    void FixedUpdate()
    {
        // --- Apply Movement via Rigidbody ---
        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = rb.linearVelocity.y; // preserve gravity / jump

        rb.linearVelocity = velocity;
    }

    // Draw ground check sphere in editor for debugging
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
