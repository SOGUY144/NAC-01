using UnityEngine;

// ==================================================
// PlayerController.cs (v3 - Cleaned up & Integrated with CallManager)
// - หมุนกล้องด้วยเมาส์ (ซ้าย-ขวา อิสระ, บน-ล่าง ล็อกมุม)
// - Raycast ตรงกลางจอเพื่อกดรับสายโทรศัพท์ (คลิกซ้าย)
// - ระงับการหันหน้าตอนอยู่ใน Choice Mode
// ==================================================

public class PlayerController : MonoBehaviour
{
    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public float maxPitch = 60f; // มุมก้มเงยสูงสุด (90 องศาบางทีดูแปลก 60 กำลังดี)

    [Header("Interaction Settings")]
    public float interactRange = 3f; // ระยะเอื้อมมือ
    public Camera playerCamera;

    [Header("UI References")]
    public GameObject interactPrompt;
    public TMPro.TextMeshProUGUI interactPromptText;

    // State
    private float currentPitch = 0f;
    private WorldButtonInteract currentHoveredButton;

    // ลบการใช้ IsInChoiceMode ออกไปเลย เพื่อให้ผู้เล่นหันหน้าและเอากากบาทไปชี้ UI แบบ 3D ได้ตลอดเวลา
    // public static bool IsInChoiceMode = false;

    void Start()
    {
        LockCursor(true);
        if (playerCamera == null) playerCamera = Camera.main;
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    void Update()
    {
        // 1. จัดการการหันมุมกล้อง (หันได้ตลอดเวลาเพื่อเล็ง UI 3D)
        HandleMouseLook();

        // 2. จัดการการยิง Raycast หาของที่กดได้
        HandleInteraction();
        
        // ปลดล็อกเมาส์ฉุกเฉิน
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LockCursor(false);
        }
    }

    void HandleMouseLook()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // แก้ไขบัคเก่า: หมุนตัวผู้เล่นซ้าย-ขวา แทนการใช้ startRotation ฝืนๆ
        transform.Rotate(Vector3.up * mx);

        // หมุนกล้องก้ม-เงย (หมุนแกน X ของกล้อง) และทำการ Clamp จำกัดมุมเพื่อไม่ให้คอหัก
        currentPitch = Mathf.Clamp(currentPitch - my, -maxPitch, maxPitch);
        
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        }
    }

    void HandleInteraction()
    {
        if (playerCamera == null) return;

        // ยิงเลเซอร์จากกึ่งกลางหน้าจอ 
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        bool hitSomething = false;

        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red);

        // ยิง Raycast ทะลวงหาของ
        if (Physics.Raycast(ray, out RaycastHit hitInfo, interactRange))
        {
            GameObject hitObj = hitInfo.collider.gameObject;

            // ----------------------------------------------------
            // 1. ตรวจจับโทรศัพท์ (ระบบ CallManager)
            // ----------------------------------------------------
            if (hitObj.CompareTag("Phone") || hitObj.name.ToLower().Contains("phone"))
            {
                if (SOGUY.CallSystem.Core.CallManager.Instance != null && SOGUY.CallSystem.Core.CallManager.Instance.IsRinging)
                {
                    hitSomething = true;
                    ShowPrompt("[คลิกซ้าย] รับสาย 911");
                    
                    if (Input.GetMouseButtonDown(0))
                    {
                        // ตอบรับสาย
                        SOGUY.CallSystem.Core.CallManager.Instance.AnswerCall();
                        
                        // [แก้ไขด่วน] กรณีคุณใส่ AudioSource ไว้ที่ตัวโมเดลโทรศัพท์ ให้สั่ง Stop ตรงนี้เลย เสียงจะได้ตัดฉับ!
                        AudioSource phoneAudio = hitObj.GetComponent<AudioSource>();
                        if (phoneAudio != null)
                        {
                            phoneAudio.Stop();
                        }
                    }
                }
                else
                {
                    hitSomething = true;
                    ShowPrompt("ระบบสแตนด์บาย");
                }
            }

            // ----------------------------------------------------
            // 2. ตรวจจับ WorldButtonInteract (ปุ่มลอยในฉาก / ปุ่ม 3D UI)
            // ----------------------------------------------------
            var worldButton = hitObj.GetComponent<WorldButtonInteract>();
            if (worldButton != null && !hitSomething)
            {
                hitSomething = true;
                
                // จัดการเรื่อง Hover Enter/Exit สีเปลี่ยน
                if (currentHoveredButton != worldButton)
                {
                    ClearHoverState();
                    currentHoveredButton = worldButton;
                    currentHoveredButton.OnHoverEnter();
                }

                ShowPrompt("[คลิกซ้าย] เลือกตอบ");
                
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
                {
                    worldButton.OnInteract();
                }
            }
        }

        // ถ้าเลเซอร์ไม่โดนอะไรที่กดได้เลย หรือหันหน้าหนีปุ่ม
        if (!hitSomething)
        {
            ClearHoverState();
            HidePrompt();
        }
    }

    private void ClearHoverState()
    {
        if (currentHoveredButton != null)
        {
            currentHoveredButton.OnHoverExit();
            currentHoveredButton = null;
        }
    }

    void ShowPrompt(string msg)
    {
        if (interactPrompt == null) return;
        interactPrompt.SetActive(true);
        if (interactPromptText != null)
            interactPromptText.text = msg;
    }

    void HidePrompt()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    public static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}