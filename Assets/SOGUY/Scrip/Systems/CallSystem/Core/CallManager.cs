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
        public event Action<float, float> OnTimerUpdate;     // อัพเดทเวลา (เหลือ, เต็ม)
        public event Action OnTimerEnd;                      // เวลาหมด

        private Coroutine _choiceTimerCoroutine;

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
            else if (_currentNode.IsAutoProceed)
            {
                if (_currentNode.AutoNextNode != null)
                {
                    GoToNode(_currentNode.AutoNextNode);
                }
                else
                {
                    Debug.LogWarning("[CallManager] IsAutoProceed ถูกเปิดไว้ แต่ไม่ได้ใส่ AutoNextNode! สายจะถูกตัดเพื่อป้องกันบัค");
                    EndCall(CallOutcome.Failure);
                }
            }
            else
            {
                OnChoicesReady?.Invoke(_currentNode);
                
                // ถ้าระบุเวลาไว้ ให้เริ่มวิ่งตัวหน่วงเวลา (QTE)
                if (_currentNode.TimerDuration > 0f)
                {
                    if (_currentNode.TimeoutNode != null)
                    {
                        _choiceTimerCoroutine = StartCoroutine(ChoiceTimerRoutine(_currentNode.TimerDuration, _currentNode.TimeoutNode));
                    }
                    else
                    {
                        Debug.LogWarning("[CallManager] มีกการตั้งเวลา Timer แต่ไม่ได้ใส่ TimeoutNode ไว้! ถือว่าให้เวลาผู้เล่นเลือกชิลๆ");
                    }
                }
            }
        }

        private IEnumerator ChoiceTimerRoutine(float duration, DialogueNode timeoutNode)
        {
            float remaining = duration;
            while (remaining > 0)
            {
                remaining -= Time.deltaTime;
                OnTimerUpdate?.Invoke(remaining, duration);
                yield return null; // รอเฟรมถัดไป
            }

            // เวลาหมด!
            remaining = 0;
            OnTimerUpdate?.Invoke(remaining, duration);
            OnTimerEnd?.Invoke();
            
            GoToNode(timeoutNode); // บังคับเตะเข้าสู่ฉาก Bad Outcome
        }

        public void MakeChoice(int choiceIndex)
        {
            if (!_isCallActive || _currentNode == null || _currentNode.IsTerminalNode) return;

            // ผู้เล่นกดเลือกทันเวลา! ยกเลิกการนับเวลาถอยหลังทันที
            if (_choiceTimerCoroutine != null)
            {
                StopCoroutine(_choiceTimerCoroutine);
                _choiceTimerCoroutine = null;
            }

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
