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

    private Rigidbody rb;
    private Animator animator;

    private Vector3 moveDirection;
    private float currentSpeed;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        rb.freezeRotation = true;

        // กัน Animator จำค่าเก่า
        if (animator != null)
        {
            animator.SetBool("jump", false);
            
        }
    }

    void Update()
    {
        isGrounded = IsOnGround();

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        float moveX = 0f;
        float moveZ = 0f;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveZ += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveZ -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveX += 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveX -= 1f;

        moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        bool isSprinting = keyboard.leftShiftKey.isPressed;
        currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // ----- Jump -----
        if (keyboard.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // ----- Rotation -----
        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // ----- Animator Control -----
        if (animator != null)
        {
            

            // สำคัญ: ใช้สถานะพื้นล้วน ๆ
            animator.SetBool("jump", !isGrounded);
        }
    }

    void FixedUpdate()
    {
        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    bool IsOnGround()
    {
        CapsuleCollider col = GetComponent<CapsuleCollider>();

        float rayLength = 0.1f;
        float bottomOfCapsule = transform.position.y - (col.height / 2f) + col.radius;

        Vector3 rayOrigin = new Vector3(
            transform.position.x,
            bottomOfCapsule + 0.05f,
            transform.position.z
        );

        return Physics.Raycast(rayOrigin, Vector3.down, rayLength);

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 origin = transform.position + Vector3.up * 0.15f;
        Gizmos.DrawLine(origin, origin + Vector3.down * 0.18f);
    }
}