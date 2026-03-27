using System;
using UnityEngine;

namespace SOGUY.CallSystem.Data
{
    [Serializable]
    public class Choice
    {
        [Tooltip("The text the operator (player) says or selects.")]
        [TextArea(2, 3)]
        public string ChoiceText;

        [Tooltip("The node to jump to if this choice is selected.")]
        public DialogueNode NextNode;

        [Tooltip("Optional event ID to trigger external logic (e.g., 'DeployPolice', 'IncreaseStress').")]
        public string EventTriggerID;
    }
}
