using UnityEngine;

public class CameraTimeControl : MonoBehaviour
{
    [System.Serializable]
    public class CameraPoint
    {
        public Vector3 rotation;   // มุมที่จะไป
        public float moveTime;     // ใช้เวลากี่วิในการหมุนไป
        public float waitTime;     // หยุดกี่วิ
    }

    public CameraPoint[] points;

    private int index = 0;
    private float timer = 0f;
    private bool isWaiting = false;

    private Quaternion startRot;
    private Quaternion targetRot;

    void Start()
    {
        startRot = transform.localRotation;
        targetRot = Quaternion.Euler(points[0].rotation);
    }

    void Update()
    {
        if (isWaiting)
        {
            timer += Time.deltaTime;

            if (timer >= points[index].waitTime)
            {
                timer = 0f;
                isWaiting = false;

                index = (index + 1) % points.Length;

                startRot = transform.localRotation;
                targetRot = Quaternion.Euler(points[index].rotation);
            }

            return;
        }

        timer += Time.deltaTime;

        float t = timer / points[index].moveTime;

        transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);

        if (t >= 1f)
        {
            timer = 0f;
            isWaiting = true;
        }
    }
}