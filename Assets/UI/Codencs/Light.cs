using UnityEngine;
using System.Collections;

public class FlickerLight : MonoBehaviour
{
    public GameObject lightObject; // 🔥 ตัวไฟ (GameObject ที่มี Light)

    public float flickerSpeed = 0.05f;
    public float startDelay = 1.5f;

    private Coroutine flickerCoroutine;
    private bool isFlickering = false;

    void Start()
    {
        StartFlicker();
    }

    public void StartFlicker()
    {
        if (isFlickering) return;

        flickerCoroutine = StartCoroutine(FlickerRoutine());
        isFlickering = true;
    }

    public void StopFlicker()
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }

        isFlickering = false;

        // 🔴 ให้ไฟกลับมาติดแน่นอน
        if (lightObject != null)
        {
            lightObject.SetActive(true);
        }
    }

    IEnumerator FlickerRoutine()
    {
        // ⏱ รอก่อนเริ่ม
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            yield return FlickerBlock(0.5f);
            yield return new WaitForSeconds(5f);

            yield return FlickerBlock(1f);
            yield return new WaitForSeconds(5f);
        }
    }

    IEnumerator FlickerBlock(float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            bool state = Random.value > 0.5f;

            // 🔥 เปิด/ปิดทั้ง GameObject
            lightObject.SetActive(state);

            yield return new WaitForSeconds(flickerSpeed);
            time += flickerSpeed;
        }

        // 👉 จบให้ไฟติด
        lightObject.SetActive(true);
    }
}