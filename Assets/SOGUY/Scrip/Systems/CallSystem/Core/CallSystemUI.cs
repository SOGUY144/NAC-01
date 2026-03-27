using UnityEngine;
using TMPro;
using UnityEngine.UI;
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

        private List<GameObject> activeChoiceButtons = new List<GameObject>();

        private void Start()
        {
            if (CallManager.Instance != null)
            {
                CallManager.Instance.OnCallAnswered += HandleCallAnswered;
                CallManager.Instance.OnNodeReached += HandleNodeReached;
                CallManager.Instance.OnChoicesReady += HandleChoicesReady;
                CallManager.Instance.OnCallEnded += HandleCallEnded;
            }
            else
            {
                Debug.LogError("[CallSystemUI] ไม่พบ CallManager ในฉาก! สคริปต์ UI จะไม่ทำงาน");
            }

            HideUI();
        }

        private void OnDestroy()
        {
            if (CallManager.Instance != null)
            {
                CallManager.Instance.OnCallAnswered -= HandleCallAnswered;
                CallManager.Instance.OnNodeReached -= HandleNodeReached;
                CallManager.Instance.OnChoicesReady -= HandleChoicesReady;
                CallManager.Instance.OnCallEnded -= HandleCallEnded;
            }
        }

        private void HandleCallAnswered(CallScenario scenario)
        {
            ShowUI();
            UpdateCallerInfo(scenario.CallerName, scenario.LocationEstimate);
            UpdateCallerText("");
            ClearChoices();
        }

        private void HandleNodeReached(DialogueNode node)
        {
            ClearChoices();
            UpdateCallerText(node.CallerText);
        }

        private void HandleChoicesReady(DialogueNode node)
        {
            DisplayChoices(node.Choices);
        }

        private void HandleCallEnded(CallOutcome outcome)
        {
            HideUI();
        }

        private void ShowUI()
        {
            if (UIPanel != null) UIPanel.SetActive(true);
        }

        private void HideUI()
        {
            if (UIPanel != null) UIPanel.SetActive(false);
            ClearChoices();
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
