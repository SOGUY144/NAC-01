using UnityEngine;
using SOGUY.CallSystem.Core;
using SOGUY.CallSystem.Data;
using System.Collections;

namespace SOGUY.CallSystem.Enhancements
{
    [RequireComponent(typeof(Outline))]
    public class PhoneHighlight : MonoBehaviour
    {
        private Outline _outline;
        private bool _isSubscribed = false;

        private void Awake()
        {
            _outline = GetComponent<Outline>();
            // เริ่มเกมมา ปิดเส้นขอบไว้ก่อน
            _outline.enabled = false; 
        }

        private IEnumerator Start()
        {
            // รอจนกว่า CallManager จะพร้อมใช้งาน
            while (CallManager.Instance == null)
            {
                yield return null;
            }

            if (!_isSubscribed)
            {
                CallManager.Instance.OnCallRinging += HandleRinging;
                CallManager.Instance.OnCallAnswered += HandleAnswered;
                CallManager.Instance.OnCallEnded += HandleCallEnded;
                _isSubscribed = true;
            }
        }

        private void HandleRinging(CallScenario scenario)
        {
            // ถ้าสายที่โทรเข้ามาเป็น "โทรศัพท์" (ไม่ใช่วิทยุ) -> เปิดเส้นขอบเตือนผู้เล่น!
            if (scenario != null && scenario.Type == CallType.Telephone)
            {
                _outline.enabled = true;
                Debug.Log("📞 โทรศัพท์กำลังดัง! เปิดเส้นขอบ Outline");
            }
        }

        private void HandleAnswered(CallScenario scenario)
        {
            // พอผู้เล่นกดรับสายแล้ว -> ปิดเส้นขอบ
            _outline.enabled = false;
        }

        private void HandleCallEnded(CallOutcome outcome)
        {
            // สายตัด/วางสาย -> ปิดเส้นขอบ (เผื่อไว้)
            _outline.enabled = false;
        }

        private void OnDestroy()
        {
            if (CallManager.Instance != null && _isSubscribed)
            {
                CallManager.Instance.OnCallRinging -= HandleRinging;
                CallManager.Instance.OnCallAnswered -= HandleAnswered;
                CallManager.Instance.OnCallEnded -= HandleCallEnded;
            }
        }
    }
}
