using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// สคริปต์ติดที่ปุ่ม UI เพื่อเปลี่ยนสีเมื่อ PlayerRaycaster มองมา
/// </summary>
public class WorldButtonHover : MonoBehaviour
{
    [Header("การตั้งค่าสี")]
    public Color NormalColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
    public Color HoverColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);

    private Image buttonImage;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = NormalColor;
        }
    }

    // ฟังก์ชันนี้จะถูกเรียกโดย PlayerRaycaster เมื่อผู้เล่นหันมามอง
    public void OnLookAt()
    {
        if (buttonImage != null) buttonImage.color = HoverColor;
    }

    // ฟังก์ชันนี้จะถูกเรียกโดย PlayerRaycaster เมื่อผู้เล่นหันไปทางอื่น
    public void OnLookAway()
    {
        if (buttonImage != null) buttonImage.color = NormalColor;
    }
}