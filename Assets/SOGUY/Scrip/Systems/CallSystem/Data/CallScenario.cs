using UnityEngine;

namespace SOGUY.CallSystem.Data
{
    public enum CallType
    {
        Telephone,
        Radio
    }

    [CreateAssetMenu(fileName = "New Call Scenario", menuName = "SOGUY/Call System/Call Scenario", order = 0)]
    public class CallScenario : ScriptableObject
    {
        [Header("Scenario Settings")]
        [Tooltip("เลือกประเภทสาย: โทรศัพท์โต๊ะ หรือ วิทยุวอร์พกพา")]
        public CallType Type = CallType.Telephone;

        [Header("Caller Details")]
        [Tooltip("The ID or Name displayed on the terminal (e.g., 'UNKNOWN', '911-555-0199').")]
        public string CallerName = "UNKNOWN";

        [Tooltip("Location data for the dispatch terminal.")]
        public string LocationEstimate = "Acquiring...";

        [Tooltip("The opening line of the call.")]
        public DialogueNode StartingNode;
    }
}
