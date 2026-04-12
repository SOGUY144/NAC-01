using UnityEngine;
using SOGUY.CallSystem.Data;
using SOGUY.CallSystem.Core;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class RadioInteract : MonoBehaviour
{
    [Header("แฟ้มคดีวิทยุที่จะเปิดเมื่อกด E")]
    [Tooltip("ลากแฟ้ม Call Scenario (ประเภท Radio) มาใส่ตรงนี้")]
    public CallScenario RadioScenario;

    [Header("UI ปุ่มกด E (ถ้ามี)")]
    public GameObject InteractPromptUI; 

    [Header("เสียงนำทาง (วิทยุส่งเสียงซ่าๆ เรียกผู้เล่น)")]
    [Tooltip("ลากไฟล์เสียงซ่าๆ/บี๊บๆ ของวิทยุมาใส่")]
    public AudioClip RadioStaticClip;

    [Tooltip("ความดังของเสียงซ่า (0-1)")]
    [Range(0f, 1f)]
    public float StaticVolume = 0.7f;

    [Header("เอฟเฟกต์แสง (ถ้าต้องการ)")]
    public GameObject GlowEffect;
    public bool PulseGlow = false;
    public float PulseSpeed = 2f;
    [Range(0f, 1f)]
    public float PulseMinIntensity = 0.3f;

    [Header("สถานะ (อ่านอย่างเดียว)")]
    [SerializeField] private bool _isRadioReady = false;

    public bool IsRadioReady => _isRadioReady;

    private AudioSource _audioSource;
    private Light _glowLight;
    private float _glowBaseIntensity;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        // ตั้งค่า 3D Audio ให้เสียงดังจากตำแหน่งวิทยุจริงๆ
        _audioSource.spatialBlend = 1f;      // 3D เต็มร้อย
        _audioSource.rolloffMode = AudioRolloffMode.Linear;
        _audioSource.minDistance = 1f;       // ใกล้กว่านี้ดังเต็ม
        _audioSource.maxDistance = 15f;      // ไกลกว่านี้จะไม่ได้ยิน
        _audioSource.playOnAwake = false;
        _audioSource.loop = true;
    }

    private void Start()
    {
        HidePrompt();
        HideGlow();

        if (GlowEffect != null)
        {
            _glowLight = GlowEffect.GetComponent<Light>();
            if (_glowLight != null) _glowBaseIntensity = _glowLight.intensity;
        }
    }

    private void Update()
    {
        if (_isRadioReady && PulseGlow && _glowLight != null)
        {
            float pulse = Mathf.Lerp(PulseMinIntensity, 1f, (Mathf.Sin(Time.time * PulseSpeed) + 1f) / 2f);
            _glowLight.intensity = _glowBaseIntensity * pulse;
        }
    }

    /// <summary>
    /// สั่งเปิดวิทยุให้พร้อมรับสาย (เสียงซ่าจะดังขึ้น!)
    /// </summary>
    public void EnableRadio()
    {
        _isRadioReady = true;
        StartRadioStatic();
        ShowGlow();
        Debug.Log("[RadioInteract] วิทยุพร้อมรับสายแล้ว! เสียงซ่าดังอยู่...");
    }

    /// <summary>
    /// สั่งเปิดวิทยุ พร้อมเปลี่ยนแฟ้มคดีด้วย
    /// </summary>
    public void EnableRadio(CallScenario scenario)
    {
        RadioScenario = scenario;
        EnableRadio();
    }

    /// <summary>
    /// สั่งปิดวิทยุ ไม่ให้กดได้ (เสียงซ่าหยุด)
    /// </summary>
    public void DisableRadio()
    {
        _isRadioReady = false;
        StopRadioStatic();
        HidePrompt();
        HideGlow();
    }

    // ---------- เสียง ----------
    private void StartRadioStatic()
    {
        if (RadioStaticClip != null && _audioSource != null)
        {
            _audioSource.clip = RadioStaticClip;
            _audioSource.volume = StaticVolume;
            _audioSource.Play();
        }
    }

    private void StopRadioStatic()
    {
        if (_audioSource != null) _audioSource.Stop();
    }

    // ---------- UI Prompt ----------
    public void ShowPrompt()
    {
        if (InteractPromptUI != null && _isRadioReady) InteractPromptUI.SetActive(true);
    }

    public void HidePrompt()
    {
        if (InteractPromptUI != null) InteractPromptUI.SetActive(false);
    }

    // ---------- Glow ----------
    private void ShowGlow()
    {
        if (GlowEffect != null) GlowEffect.SetActive(true);
    }

    private void HideGlow()
    {
        if (GlowEffect != null) GlowEffect.SetActive(false);
    }

    // ---------- Interaction ----------
    public void TriggerInteraction()
    {
        if (!_isRadioReady) return;

        if (RadioScenario == null)
        {
            Debug.LogWarning("[RadioInteract] ยังไม่ได้ใส่แฟ้มคดีวิทยุ!");
            return;
        }

        // เสียงซ่าหยุด -> เริ่มบทสนทนา
        StopRadioStatic();
        
        CallManager.Instance.ReceiveIncomingCall(RadioScenario);
        CallManager.Instance.AnswerCall();
        
        _isRadioReady = false;
        HidePrompt();
        HideGlow();
    }
}
