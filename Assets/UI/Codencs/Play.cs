using UnityEngine;
using UnityEngine.Playables;

public class PlayButtonController : MonoBehaviour
{
    [Header("Camera")]
    public Transform cameraTransform;
    public Vector3 centerRotation;
    public float moveSpeed = 2f;

    [Header("Timeline")]
    public PlayableDirector timeline;

    [Header("UI")]
    public GameObject uiPanel;

    [Header("Light Flicker")]
    public FlickerLight flickerLight; // 👈 เพิ่มตรงนี้

    private bool isMoving = false;
    private bool hasPlayed = false;

    public void OnPressPlay()
    {
        Debug.Log("CLICKED");

        if (hasPlayed)
        {
            Debug.Log("Already played");
            return;
        }

        if (cameraTransform == null)
        {
            Debug.LogError("❌ Camera Transform ยังไม่ได้ใส่!");
            return;
        }

        // 🔴 ปิด UI
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }

        // 🔴 หยุดไฟกระพริบ
        if (flickerLight != null)
        {
            flickerLight.StopFlicker();
        }

        isMoving = true;
    }

    void Update()
    {
        if (!isMoving || hasPlayed) return;

        Quaternion targetRot = Quaternion.Euler(centerRotation);

        cameraTransform.localRotation = Quaternion.Slerp(
            cameraTransform.localRotation,
            targetRot,
            Time.deltaTime * moveSpeed
        );

        float angle = Quaternion.Angle(cameraTransform.localRotation, targetRot);

        if (angle < 2f)
        {
            isMoving = false;
            hasPlayed = true;

            Debug.Log("Reached center → Play Timeline");

            if (timeline != null)
            {
                timeline.Play();
            }
            else
            {
                Debug.LogError("❌ Timeline ยังไม่ได้ใส่!");
            }
        }
    }
}