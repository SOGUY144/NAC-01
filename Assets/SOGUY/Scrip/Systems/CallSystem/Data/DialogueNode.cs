using UnityEngine;
using System.Collections.Generic;

namespace SOGUY.CallSystem.Data
{
    public enum CallOutcome
    {
        Neutral,
        Success, // Caller is safe
        Failure  // Caller died / Bad ending
    }

    [CreateAssetMenu(fileName = "New Dialogue Node", menuName = "SOGUY/Call System/Dialogue Node", order = 1)]
    public class DialogueNode : ScriptableObject
    {
        [Header("Caller Information")]
        [TextArea(3, 5)]
        [Tooltip("What the caller says over the phone.")]
        public string CallerText;

        [Tooltip("The audio asset for the caller's voice.")]
        public AudioClip CallerAudio;

        [Tooltip("Delay in seconds before choices appear after the audio finishes playing. If no audio, it acts as a pure pause.")]
        public float DelayAfterAudio = 0.5f;

        [Header("Node Branching")]
        [Tooltip("Does the call disconnect/end on this node?")]
        public bool IsTerminalNode = false;

        [Tooltip("If the call ends here, what is the outcome?")]
        public CallOutcome Outcome = CallOutcome.Neutral;

        [Tooltip("The choices presented to the player after the caller finishes speaking.")]
        public List<Choice> Choices = new List<Choice>();
    }
}
