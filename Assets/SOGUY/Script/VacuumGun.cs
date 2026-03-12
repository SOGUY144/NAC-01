using UnityEngine;

public class VacuumGun : MonoBehaviour
{
    public float captureDistance = 8f;
    public float pullForce = 20f;
    public float captureRange = 2f;

    public LayerMask slimeLayer;
    public LayerMask foodLayer; // เพิ่ม Layer สำหรับอาหาร

    PlayerInventory inventory;

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            TryCapture();
        }
    }

    void TryCapture()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * captureDistance, Color.red);

        // รวม Layer ทั้งสองเพื่อยิง Ray ครั้งเดียว
        LayerMask combinedLayer = slimeLayer | foodLayer;

        if (Physics.Raycast(ray, out hit, captureDistance, combinedLayer))
        {
            // --- ดูดสไลม์ ---
            SlimeCapture slime = hit.collider.GetComponent<SlimeCapture>();
            if (slime != null)
            {
                Rigidbody rb = slime.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 dir = (Camera.main.transform.position - slime.transform.position).normalized;
                    rb.AddForce(dir * pullForce, ForceMode.Acceleration);
                }

                float dist = Vector3.Distance(transform.position, slime.transform.position);
                if (dist < captureRange)
                {
                    slime.Capture(inventory);
                }
            }

            // --- ดูดอาหาร ---
            Food food = hit.collider.GetComponent<Food>();
            if (food != null)
            {
                Rigidbody rb = food.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 dir = (Camera.main.transform.position - food.transform.position).normalized;
                    rb.AddForce(dir * pullForce, ForceMode.Acceleration);
                }

                float dist = Vector3.Distance(transform.position, food.transform.position);
                if (dist < captureRange)
                {
                    inventory.AddBanana();
                    food.GetEaten(); // ซ่อนอาหาร + เริ่ม respawn
                }
            }
        }
    }
}