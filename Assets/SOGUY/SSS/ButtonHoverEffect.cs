using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// สคริปต์สำหรับเปลี่ยนสีของปุ่ม UI เมื่อเคอร์เซอร์วางเมาส์เหนือ
/// เหมาะสำหรับ Unity 6 โดยใช้ Unity UI (uGUI)
/// </summary>
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // --- การตั้งค่าใน Inspector ---
    [Header("การตั้งค่าสี")]
    [Tooltip("สีของปุ่มเมื่อเคอร์เซอร์ไม่ได้อยู่เหนือ (ปกติ)")]
    public Color NormalColor = new Color(0.2f, 0.2f, 0.2f, 1.0f); // สีเทาเข้ม

    [Tooltip("สีของปุ่มเมื่อเคอร์เซอร์วางเมาส์เหนือ (ไฮไลท์)")]
    public Color HoverColor = new Color(0.8f, 0.8f, 0.8f, 1.0f); // สีเทาอ่อน (เหมือนในรูป)

    // --- ส่วนประกอบที่จำเป็น ---
    private Image buttonImage; // ข้อมูลอ้างอิงถึง Image Component ของปุ่ม

    void Awake()
    {
        // ค้นหา Image Component ที่อยู่บน GameObject เดียวกัน
        buttonImage = GetComponent<Image>();

        if (buttonImage == null)
        {
            Debug.LogError("Error: ไม่พบ Image Component บน " + gameObject.name + ". สคริปต์นี้จำเป็นต้องแนบกับวัตถุที่มี Image!");
            enabled = false; // ปิดใช้งานสคริปต์เพื่อป้องกันข้อผิดพลาด
        }
        else
        {
            // ตั้งค่าสีเริ่มต้นเป็นสีปกติ
            buttonImage.color = NormalColor;
        }
    }

    /// <summary>
    /// เรียกใช้เมื่อเคอร์เซอร์เมาส์เข้าสู่พื้นที่ของปุ่ม (IPointerEnterHandler)
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            // เปลี่ยนสีเป็นสีไฮไลท์
            buttonImage.color = HoverColor;
        }
    }

    /// <summary>
    /// เรียกใช้เมื่อเคอร์เซอร์เมาส์ออกจากพื้นที่ของปุ่ม (IPointerExitHandler)
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            // เปลี่ยนสีกลับเป็นสีปกติ
            buttonImage.color = NormalColor;
        }
    }
}