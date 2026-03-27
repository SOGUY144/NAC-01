using UnityEngine;
using SOGUY.CallSystem.Core;
using SOGUY.CallSystem.Data;

public class CallUIHandler : MonoBehaviour
{
    [Header("ใส่ไฟล์ Scenario ตรงนี้เพื่อทดสอบ")]
    public CallScenario testScenario;

    private void Start()
    {
        // ติดตาม Event ต่างๆ ของระบบรวมถึง Event ใหม่ตอนสายรอรับ
        CallManager.Instance.OnCallRinging += ShowRingingUI;
        CallManager.Instance.OnCallAnswered += HideRingingUI;
        CallManager.Instance.OnNodeReached += ShowSubtitleText;
        CallManager.Instance.OnChoicesReady += SpawnChoiceButtons;

        if (testScenario != null)
        {
            // เปลี่ยนจาก StartCall ทันที เป็นสั่งให้ "มีสายเข้า (เสียงเริ่มดัง)" แทน
            CallManager.Instance.ReceiveIncomingCall(testScenario);
        }
    }

    private void Update()
    {
        // โค้ดชั่วคราว เอาไว้ทดสอบกด "Spacebar" เพื่อรับสาย
        // (ในอนาคตคุณค่อยใช้วิกดที่ปุ่มโทรศัพท์หน้าจอคอมในฉากแล้วเรียก CallManager.Instance.AnswerCall() แทนได้เลย)
        if (CallManager.Instance != null && CallManager.Instance.IsRinging)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CallManager.Instance.AnswerCall();
            }
        }
    }

    private void OnDestroy()
    {
        if (CallManager.Instance != null) {
            CallManager.Instance.OnCallRinging -= ShowRingingUI;
            CallManager.Instance.OnCallAnswered -= HideRingingUI;
            CallManager.Instance.OnNodeReached -= ShowSubtitleText;
            CallManager.Instance.OnChoicesReady -= SpawnChoiceButtons;
        }
    }

    // --- ฟังก์ชัน UI ---

    private void ShowRingingUI(CallScenario scenario) {
        Debug.Log($"[ระบบ] โทรศัพท์กำลังดัง กรุ่งกริ๊ง... สายจาก: {scenario.CallerName} --- (กด Spacebar อย่างระมัดระวังเพื่อรับสาย!)");
    }

    private void HideRingingUI(CallScenario scenario) {
        Debug.Log("[ระบบ] รับสายแล้ว กำลังเตรียมเชื่อมต่อบทสนทนา...");
    }

    private void ShowSubtitleText(SOGUY.CallSystem.Data.DialogueNode node) {
        Debug.Log("คนโทรพูดว่า: " + node.CallerText);
    }

    private void SpawnChoiceButtons(SOGUY.CallSystem.Data.DialogueNode node) {
        Debug.Log("เสียงพูดจบแล้ว โชว์ปุ่มได้! มีตัวเลือกให้ " + node.Choices.Count + " ข้อ");
    }
}
