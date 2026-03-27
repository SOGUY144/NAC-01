using UnityEngine;
using System.Collections;

public class PhoneSystem : MonoBehaviour
{
    public AudioSource ringtone;
    public AudioSource pickup;
    public AudioSource voice;

    private Coroutine ringingCoroutine;
    private bool canAnswer = false;

    void Start()
    {
        // เริ่มโทรหลังเกมเริ่ม 2 วินาที
        StartCoroutine(StartRingingAfterDelay(2f));
    }

    IEnumerator StartRingingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartRinging();
        canAnswer = true; // ผู้เล่นสามารถกด E ได้
        // TODO: แสดง UI "กด E เพื่อรับสาย" ถ้าต้องการ
    }

    public void StartRinging()
    {
        if (ringingCoroutine != null)
            StopCoroutine(ringingCoroutine);

        ringingCoroutine = StartCoroutine(RingLoop());
    }

    IEnumerator RingLoop()
    {
        while (true)
        {
            ringtone.Play();
            yield return new WaitForSeconds(ringtone.clip.length);
        }
    }

    public void AnswerCall()
    {
        if (!canAnswer) return;

        canAnswer = false;

        if (ringingCoroutine != null)
            StopCoroutine(ringingCoroutine);

        ringtone.Stop();
        pickup.Play();

        StartCoroutine(PlayVoiceAfterPickup());
    }

    IEnumerator PlayVoiceAfterPickup()
    {
        yield return new WaitForSeconds(pickup.clip.length);
        voice.Play();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canAnswer)
        {
            AnswerCall();
        }
    }

    // ฟังก์ชันหยุด/เริ่มริงโทนตอน UI Panel
    public void PauseRingtone()
    {
        if (ringingCoroutine != null)
            StopCoroutine(ringingCoroutine);
        ringtone.Stop();
        // ไม่แตะ canAnswer เพื่อให้ UI ยังทำงาน
    }

    public void ResumeRingtone()
    {
        StartRinging();
    }
}