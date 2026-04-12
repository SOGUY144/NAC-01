using UnityEngine;
using System.Collections;
using SOGUY.CallSystem.Data;
using SOGUY.CallSystem.Core;

// ==================================================
// StoryDirector.cs (v2 - Branching Support)
// ผู้กำกับเรื่องราว - จัดคิวลำดับเหตุการณ์ + แตกกิ่ง
//
// ตัวอย่างการแตกกิ่ง:
//   Event 0: PhoneCall C1
//     ถ้าจบดี (Success)  → ไป Event 1
//     ถ้าจบแย่ (Failure) → ไป Event 3
//   Event 1: RadioManual ...
//   Event 3: PhoneCall สายฉุกเฉิน!
// ==================================================

public class StoryDirector : MonoBehaviour
{
    [System.Serializable]
    public class StoryEvent
    {
        public enum EventType 
        { 
            PhoneCall,      // สายโทรศัพท์ (รอกดรับ)
            RadioManual,    // วิทยุ Manual (รอกด E)
            RadioAuto       // วิทยุ Auto (เริ่มเลยทันที!)
        }

        [Header("ประเภทเหตุการณ์")]
        public EventType Type = EventType.PhoneCall;

        [Header("แฟ้มคดีที่จะเปิด")]
        public CallScenario Scenario;

        [Header("รอกี่วินาทีก่อนเริ่มเหตุการณ์นี้")]
        public float DelayBeforeStart = 3f;

        [Header("เส้นทางแตกกิ่ง (ใส่เลข Event ที่จะไปต่อ)")]
        [Tooltip("-1 = ไปอันถัดไปตามลำดับปกติ, -99 = จบเรื่อง")]
        public int NextOnSuccess = -1;   // จบดี -> ไป Event ไหน?

        [Tooltip("-1 = ไปอันถัดไปตามลำดับปกติ, -99 = จบเรื่อง")]
        public int NextOnFailure = -1;   // จบแย่ -> ไป Event ไหน?

        [Tooltip("-1 = ไปอันถัดไปตามลำดับปกติ, -99 = จบเรื่อง")]
        public int NextOnNeutral = -1;   // ไม่ดีไม่แย่ -> ไป Event ไหน?
    }

    public const int NEXT_DEFAULT = -1;  // ไปอันถัดไปตามลำดับ
    public const int NEXT_END = -99;     // จบเรื่อง

    [Header("ลำดับเหตุการณ์ (เรียงจากบนลงล่าง)")]
    public StoryEvent[] Events;

    [Header("อ้างอิงวิทยุในฉาก")]
    [Tooltip("ลากโมเดลวิทยุ (ที่มี RadioInteract) มาใส่")]
    public RadioInteract RadioObject;

    [Header("เริ่มอัตโนมัติตอนกด Play?")]
    public bool AutoStart = true;

    [Header("สถานะปัจจุบัน (อ่านอย่างเดียว)")]
    [SerializeField] private int currentEventIndex = 0;
    [SerializeField] private bool isRunning = false;

    void Start()
    {
        if (CallManager.Instance != null)
        {
            CallManager.Instance.OnCallEnded += OnAnyCallEnded;
        }
        else
        {
            StartCoroutine(WaitForCallManager());
        }

        if (AutoStart && Events != null && Events.Length > 0)
        {
            StartStory();
        }
    }

    private IEnumerator WaitForCallManager()
    {
        while (CallManager.Instance == null) yield return null;
        CallManager.Instance.OnCallEnded += OnAnyCallEnded;
    }

    public void StartStory()
    {
        currentEventIndex = 0;
        isRunning = true;
        StartCoroutine(ExecuteEvent(currentEventIndex));
    }

    private void OnAnyCallEnded(CallOutcome outcome)
    {
        if (!isRunning) return;
        if (currentEventIndex < 0 || currentEventIndex >= Events.Length) return;

        StoryEvent current = Events[currentEventIndex];

        // ดูว่า Outcome เป็นอะไร แล้วเลือกเส้นทาง
        int nextIndex;
        switch (outcome)
        {
            case CallOutcome.Success:
                nextIndex = current.NextOnSuccess;
                Debug.Log($"[StoryDirector] ผลลัพธ์: ✅ Success -> ไป Event {nextIndex}");
                break;
            case CallOutcome.Failure:
                nextIndex = current.NextOnFailure;
                Debug.Log($"[StoryDirector] ผลลัพธ์: ❌ Failure -> ไป Event {nextIndex}");
                break;
            default: // Neutral
                nextIndex = current.NextOnNeutral;
                Debug.Log($"[StoryDirector] ผลลัพธ์: ⚪ Neutral -> ไป Event {nextIndex}");
                break;
        }

        // แปลค่า -1 = ไปอันถัดไป, -99 = จบเรื่อง
        if (nextIndex == NEXT_END)
        {
            isRunning = false;
            Debug.Log("[StoryDirector] 🏁 จบเรื่องราว!");
            return;
        }

        if (nextIndex == NEXT_DEFAULT)
        {
            nextIndex = currentEventIndex + 1; // ไปอันถัดไปตามลำดับ
        }

        // เช็คว่ายังอยู่ในช่วง
        if (nextIndex >= 0 && nextIndex < Events.Length)
        {
            currentEventIndex = nextIndex;
            StartCoroutine(ExecuteEvent(currentEventIndex));
        }
        else
        {
            isRunning = false;
            Debug.Log("[StoryDirector] 🏁 จบเรื่องราวทั้งหมดแล้ว!");
        }
    }

    private IEnumerator ExecuteEvent(int index)
    {
        StoryEvent evt = Events[index];

        Debug.Log($"[StoryDirector] ▶️ Event {index}: รอ {evt.DelayBeforeStart}s -> {evt.Scenario.name}");
        yield return new WaitForSeconds(evt.DelayBeforeStart);

        switch (evt.Type)
        {
            case StoryEvent.EventType.PhoneCall:
                Debug.Log($"[StoryDirector] 📞 โทรศัพท์ดัง! {evt.Scenario.name}");
                CallManager.Instance.ReceiveIncomingCall(evt.Scenario);
                break;

            case StoryEvent.EventType.RadioManual:
                Debug.Log($"[StoryDirector] 📻 วิทยุพร้อม (Manual)! {evt.Scenario.name}");
                if (RadioObject != null)
                    RadioObject.EnableRadio(evt.Scenario);
                break;

            case StoryEvent.EventType.RadioAuto:
                Debug.Log($"[StoryDirector] 📻 วิทยุ Auto! {evt.Scenario.name}");
                CallManager.Instance.ReceiveIncomingCall(evt.Scenario);
                CallManager.Instance.AnswerCall();
                break;
        }
    }

    private void OnDestroy()
    {
        if (CallManager.Instance != null)
        {
            CallManager.Instance.OnCallEnded -= OnAnyCallEnded;
        }
    }
}
