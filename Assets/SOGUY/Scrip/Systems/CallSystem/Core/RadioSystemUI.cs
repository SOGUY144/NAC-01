using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SOGUY.CallSystem.Data;

namespace SOGUY.CallSystem.Core
{
    public class RadioSystemUI : MonoBehaviour
    {
        [Header("Radio Main Panel (หน้าต่างวิทยุ)")]
        public GameObject UIPanel;

        [Header("Radio Caller Info Display (ข้อมูลคนพูดวิทยุ)")]
        public TextMeshProUGUI CallerNameText;
        public TextMeshProUGUI CallerDialogueText;

        [Header("Choices Area (ปุ่มตอบวิทยุ)")]
        public RectTransform ChoicesContainer;
        public GameObject ChoiceButtonPrefab;

        [Header("QTE Timer (หลอดเวลาวิทยุ)")]
        public Slider TimerSlider;
        public GameObject TimerPanel;

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
            // รอจนกว่า CallManager จะพร้อมใช้งาน
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
            Debug.Log("[RadioSystemUI] เชื่อมต่อระบบวิทยุสำเร็จ!");
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
            
            // เช็คว่าเป็นสายวิทยุ (Radio) หรือไม่ ถ้าไม่ใช่ให้ปิด UI นี้ทิ้งไปเลย
            if (activeScenario == null || activeScenario.Type != CallType.Radio)
            {
                HideUI();
                return;
            }

            ShowUI();
            UpdateCallerInfo(scenario.CallerName);
            UpdateCallerText("");
            ClearChoices();
            HideTimer();
        }

        private void HandleNodeReached(DialogueNode node)
        {
            if (activeScenario == null || activeScenario.Type != CallType.Radio) return;

            ClearChoices();
            HideTimer();
            UpdateCallerText(node.CallerText);
            
            // อัพเดทชื่อคนพูดแบบ Real-time บนคอวิทยุ
            string speaker = string.IsNullOrEmpty(node.SpeakerName) ? (activeScenario != null ? activeScenario.CallerName : "UNKNOWN") : node.SpeakerName;
            UpdateCallerInfo(speaker);
        }

        private void HandleChoicesReady(DialogueNode node)
        {
            if (activeScenario == null || activeScenario.Type != CallType.Radio) return;
            DisplayChoices(node.Choices);
        }

        private void HandleCallEnded(CallOutcome outcome)
        {
            HideUI();
        }

        private void HandleTimerUpdate(float remaining, float max)
        {
            if (activeScenario == null || activeScenario.Type != CallType.Radio) return;

            if (TimerSlider != null)
            {
                if (!TimerSlider.gameObject.activeSelf) TimerSlider.gameObject.SetActive(true);
                TimerSlider.value = remaining / max;
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
            UpdateCallerInfo("UNKNOWN");
        }

        private void UpdateCallerInfo(string name)
        {
            if (CallerNameText != null) CallerNameText.text = name;
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
                int choiceIndex = i;
                Choice choice = choices[i];

                GameObject btnObj = Instantiate(ChoiceButtonPrefab, ChoicesContainer, false);
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
