using UnityEngine;
using System;
using SOGUY.CallSystem.Data;
using System.Collections;

namespace SOGUY.CallSystem.Core
{
    [RequireComponent(typeof(AudioSource))]
    public class CallManager : MonoBehaviour
    {
        public static CallManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private AudioSource _audioSource;

        [Header("Phone Settings")]
        [Tooltip("เสียงเสียงโทรศัพท์เรียกเข้า")]
        public AudioClip _ringtoneClip;

        [Header("Current State (Read-Only)")]
        [SerializeField] private CallScenario _activeScenario;
        [SerializeField] private DialogueNode _currentNode;
        [SerializeField] private bool _isCallActive;
        [SerializeField] private bool _isRinging; // สถานะสายรอรับ
        
        public bool IsRinging => _isRinging;
        public bool IsCallActive => _isCallActive;

        // --- Core Events ---
        public event Action<CallScenario> OnCallRinging;     // โทรศัพท์เริ่มดัง
        public event Action<CallScenario> OnCallAnswered;    // ผู้เล่นกดรับสายแล้ว
        public event Action<DialogueNode> OnNodeReached;     // คนโทรเริ่มพูด
        public event Action<DialogueNode> OnChoicesReady;    // คนโทรพูดจบ โชว์ปุ่ม
        public event Action<CallOutcome> OnCallEnded;        // สายตัด

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// กระตุ้นให้โทรศัพท์เริ่มดัง แต่ยังไม่เริ่มบทสนทนา
        /// </summary>
        public void ReceiveIncomingCall(CallScenario scenario)
        {
            if (_isCallActive || _isRinging) return;
            if (scenario == null || scenario.StartingNode == null) return;

            _activeScenario = scenario;
            _isRinging = true;

            // เล่นเสียง Ringtone แบบวนลูป
            if (_ringtoneClip != null)
            {
                _audioSource.clip = _ringtoneClip;
                _audioSource.loop = true;
                _audioSource.Play();
            }

            OnCallRinging?.Invoke(_activeScenario);
        }

        /// <summary>
        /// ให้ผู้เล่น (หรือ UI/ปุ่ม) เรียกคำสั่งนี้เพื่อรับสาย
        /// </summary>
        public void AnswerCall()
        {
            if (!_isRinging) return;

            _isRinging = false;
            _isCallActive = true;

            // หยุดเสียง Ringtone
            _audioSource.Stop();
            _audioSource.loop = false;

            OnCallAnswered?.Invoke(_activeScenario);

            // กระโดดเข้าสู่บทสนทนาแรก
            GoToNode(_activeScenario.StartingNode);
        }

        private void GoToNode(DialogueNode node)
        {
            if (node == null)
            {
                EndCall(CallOutcome.Failure);
                return;
            }

            _currentNode = node;
            OnNodeReached?.Invoke(_currentNode);

            StopAllCoroutines();
            StartCoroutine(PlayNodeSequence());
        }

        private IEnumerator PlayNodeSequence()
        {
            if (_currentNode.CallerAudio != null)
            {
                _audioSource.clip = _currentNode.CallerAudio;
                _audioSource.Play();
                yield return new WaitForSeconds(_audioSource.clip.length);
            }

            yield return new WaitForSeconds(_currentNode.DelayAfterAudio);

            if (_currentNode.IsTerminalNode)
            {
                EndCall(_currentNode.Outcome);
            }
            else
            {
                OnChoicesReady?.Invoke(_currentNode);
            }
        }

        public void MakeChoice(int choiceIndex)
        {
            if (!_isCallActive || _currentNode == null || _currentNode.IsTerminalNode) return;

            if (choiceIndex >= 0 && choiceIndex < _currentNode.Choices.Count)
            {
                Choice selectedChoice = _currentNode.Choices[choiceIndex];

                _audioSource.Stop(); 

                if (!string.IsNullOrEmpty(selectedChoice.EventTriggerID))
                {
                    Debug.Log($"[CallManager] Triggering Event: {selectedChoice.EventTriggerID}");
                }

                GoToNode(selectedChoice.NextNode);
            }
        }

        public void EndCall(CallOutcome outcome)
        {
            if (!_isCallActive && !_isRinging) return;

            _isCallActive = false;
            _isRinging = false;
            _audioSource.Stop();
            _audioSource.loop = false;
            
            OnCallEnded?.Invoke(outcome);

            _activeScenario = null;
            _currentNode = null;
        }
    }
}
