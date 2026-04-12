using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SOGUY.CallSystem.Data;

namespace SOGUY.CallSystem.Core
{
    public class CallSystemUI : MonoBehaviour
    {
        [Header("Main Panel")]
        public GameObject UIPanel;

        [Header("Caller Info Display")]
        public TextMeshProUGUI CallerNameText;
        public TextMeshProUGUI LocationText;
        public TextMeshProUGUI CallerDialogueText;

        [Header("Choices Area")]
        public RectTransform ChoicesContainer;
        public GameObject ChoiceButtonPrefab;

        [Header("QTE Timer (หลอดเวลาจับเวลา)")]
        public Slider TimerSlider;       // ตัวหลอดเวลา
        public GameObject TimerPanel;    // กรอบของเวลา (เผื่อไว้เปิด/ปิดทั้งกรอบ)

        private List<GameObject> activeChoiceButtons = new List<GameObject>();
        private CallScenario activeScenario;
        private bool isSubscribed = false;

        private void Start()
        {
            HideUI();
            StartCoroutine(WaitAndSubscribe());
        }

        private IEnumerator WaitAndSubscribe()
        {
            while (CallManager.Instance == null)
            {
                yield return null;
            }
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (isSubscribed || CallManager.Instance == null) return;
            isSubscribed = true;

            CallManager.Instance.OnCallAnswered += HandleCallAnswered;
            CallManager.Instance.OnNodeReached += HandleNodeReached;
            CallManager.Instance.OnChoicesReady += HandleChoicesReady;
            CallManager.Instance.OnCallEnded += HandleCallEnded;
            CallManager.Instance.OnTimerUpdate += HandleTimerUpdate;
            CallManager.Instance.OnTimerEnd += HandleTimerEnd;
            Debug.Log("[CallSystemUI] เชื่อมต่อระบบโทรศัพท์สำเร็จ!");
        }

        private void OnDestroy()
        {
            if (CallManager.Instance != null)
            {
                CallManager.Instance.OnCallAnswered -= HandleCallAnswered;
                CallManager.Instance.OnNodeReached -= HandleNodeReached;
                CallManager.Instance.OnChoicesReady -= HandleChoicesReady;
                CallManager.Instance.OnCallEnded -= HandleCallEnded;
                CallManager.Instance.OnTimerUpdate -= HandleTimerUpdate;
                CallManager.Instance.OnTimerEnd -= HandleTimerEnd;
            }
        }

        private void HandleCallAnswered(CallScenario scenario)
        {
            activeScenario = scenario;
            
            // ถ้าสายนี้ไม่ใช่วิทยุคอม (Telephone) ให้ซ่อน UI นี้ทิ้งซะ แล้วปล่อยให้ RadioSystemUI ทำงานแทน
            if (activeScenario == null || activeScenario.Type != CallType.Telephone)
            {
                HideUI();
                return;
            }

            ShowUI();
            UpdateCallerInfo(scenario.CallerName, scenario.LocationEstimate);
            UpdateCallerText("");
            ClearChoices();
            HideTimer();
        }

        private void HandleNodeReached(DialogueNode node)
        {
            if (activeScenario == null || activeScenario.Type != CallType.Telephone) return;

            ClearChoices();
            HideTimer();
            UpdateCallerText(node.CallerText);
            
            // อัพเดทชื่อคนพูดแบบ Real-time (ถ้ามีการเว้นว่างไว้ จะกลับไปใช้ชื่อดั้งเดิมของปก CallScenario)
            string speaker = string.IsNullOrEmpty(node.SpeakerName) ? (activeScenario != null ? activeScenario.CallerName : "UNKNOWN") : node.SpeakerName;
            string loc = activeScenario != null ? activeScenario.LocationEstimate : "Acquiring...";
            UpdateCallerInfo(speaker, loc);
        }

        private void HandleChoicesReady(DialogueNode node)
        {
            if (activeScenario == null || activeScenario.Type != CallType.Telephone) return;
            DisplayChoices(node.Choices);
        }

        private void HandleCallEnded(CallOutcome outcome)
        {
            HideUI();
        }

        private void HandleTimerUpdate(float remaining, float max)
        {
            if (activeScenario == null || activeScenario.Type != CallType.Telephone) return;

            if (TimerSlider != null)
            {
                if (!TimerSlider.gameObject.activeSelf) TimerSlider.gameObject.SetActive(true);
                TimerSlider.value = remaining / max; // ปรับหลอดให้เป็นค่าระหว่าง 0-1
            }
            if (TimerPanel != null && !TimerPanel.activeSelf) TimerPanel.SetActive(true);
        }

        private void HandleTimerEnd()
        {
            HideTimer();
        }

        private void HideTimer()
        {
            if (TimerSlider != null) TimerSlider.gameObject.SetActive(false);
            if (TimerPanel != null) TimerPanel.SetActive(false);
        }

        private void ShowUI()
        {
            if (UIPanel != null) UIPanel.SetActive(true);
        }

        private void HideUI()
        {
            if (UIPanel != null) UIPanel.SetActive(false);
            ClearChoices();
            HideTimer();
            UpdateCallerText("");
            UpdateCallerInfo("UNKNOWN", "Acquiring...");
        }

        private void UpdateCallerInfo(string name, string location)
        {
            if (CallerNameText != null) CallerNameText.text = name;
            if (LocationText != null) LocationText.text = $"Location: {location}";
        }

        private void UpdateCallerText(string text)
        {
            if (CallerDialogueText != null) CallerDialogueText.text = text;
        }

        private void ClearChoices()
        {
            foreach (var btn in activeChoiceButtons)
            {
                if (btn != null) Destroy(btn);
            }
            activeChoiceButtons.Clear();
        }

        private void DisplayChoices(List<Choice> choices)
        {
            ClearChoices();

            if (choices == null || choices.Count == 0) return;

            for (int i = 0; i < choices.Count; i++)
            {
                int choiceIndex = i; // capture index for lambda callback
                Choice choice = choices[i];

                // ให้ instantiateInWorldSpace = false เพื่อป้องกันบัค Scale เพี้ยนเมื่ออยู่ใน World Canvas
                GameObject btnObj = Instantiate(ChoiceButtonPrefab, ChoicesContainer, false);
                
                // [แก้ไขด่วน] บังคับล้างค่าตำแหน่งและการหมุนที่อาจจะติดมาจากหน้าต่าง Prefab ให้กลายเป็น 0 ทั้งหมด!
                btnObj.transform.localPosition = Vector3.zero;
                btnObj.transform.localRotation = Quaternion.identity;
                
                activeChoiceButtons.Add(btnObj);

                TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = choice.ChoiceText;

                Button btn = btnObj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => 
                    {
                        CallManager.Instance.MakeChoice(choiceIndex);
                    });
                }
            }
        }
    }
}
