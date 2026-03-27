using UnityEngine;

namespace SOGUY.CallSystem.Data
{
    [CreateAssetMenu(fileName = "New Call Scenario", menuName = "SOGUY/Call System/Call Scenario", order = 0)]
    public class CallScenario : ScriptableObject
    {
        [Tooltip("The ID or Name displayed on the terminal (e.g., 'UNKNOWN', '911-555-0199').")]
        public string CallerName = "UNKNOWN";

        [Tooltip("Location data for the dispatch terminal.")]
        public string LocationEstimate = "Acquiring...";

        [Tooltip("The opening line of the call.")]
        public DialogueNode StartingNode;
    }
}
