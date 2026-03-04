using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float acceleration = 10f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("PlayerMovement ต้องการ Component CharacterController บนตัวละคร");
        }
    }

    private void Update()
    {
        if (controller == null) return;

        HandleGroundCheck();
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    private void HandleGroundCheck()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
        }
        else
        {
            // ถ้าไม่เซ็ต groundCheck จะใช้ controller.isGrounded แทน
            isGrounded = controller.isGrounded;
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // เล็กน้อยให้ติดพื้น
        }
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical);

        // เลือกสปีด เดิน/วิ่ง
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 move = Vector3.zero;

        if (inputDir.sqrMagnitude > 0.01f)
        {
            // ทิศทางการเดิน (บนระนาบโลก XZ)
            Vector3 moveDir = inputDir.normalized;

            // ปรับสปีดเข้าใกล้ targetSpeed เมื่อมีการกดเดิน
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            // หมุนตัวละครให้หันไปตามทิศทางการเดิน
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, acceleration * Time.deltaTime);

            move = moveDir * currentSpeed;
        }
        else
        {
            // ถ้าไม่กดเดิน ให้ค่อยๆ ลดสปีดลงจนหยุด
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, acceleration * Time.deltaTime);
        }

        controller.Move(move * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // คำนวณความเร็วกระโดดจากความสูงที่ต้องการ
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}

