using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Position")]
    public float distance = 12f;        // ระยะห่างจากตัวละคร
    public float height = 10f;          // ความสูงกล้อง
    public float angle = 45f;           // มุมก้ม (องศา) — 45 = สไตล์ Diablo

    [Header("Smooth Follow")]
    public float followSpeed = 8f;      // ความเร็วกล้องตาม (ยิ่งมากยิ่งเร็ว)

    [Header("Zoom (Scroll Wheel)")]
    public float zoomSpeed = 3f;
    public float minDistance = 5f;
    public float maxDistance = 20f;

    private Vector3 offset;

    void Start()
    {
        CalculateOffset();
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleZoom();

        // Smooth follow
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Fixed rotation — look at a point slightly above the target's feet
        Vector3 lookTarget = target.position + Vector3.up * 1f;
        Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, followSpeed * Time.deltaTime);
    }

    void HandleZoom()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * zoomSpeed * Time.deltaTime;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            CalculateOffset();
        }
    }

    void CalculateOffset()
    {
        // Calculate offset from angle and distance
        float angleRad = angle * Mathf.Deg2Rad;
        float offsetY = Mathf.Sin(angleRad) * distance;
        float offsetZ = -Mathf.Cos(angleRad) * distance;
        offset = new Vector3(0f, offsetY, offsetZ);
    }
}
