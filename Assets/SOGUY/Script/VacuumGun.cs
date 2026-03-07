using UnityEngine;

public class VacuumGun : MonoBehaviour
{
    public float captureDistance = 8f;
    public float pullForce = 20f;
    public float captureRange = 2f;

    public LayerMask slimeLayer;

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
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, captureDistance, slimeLayer))
        {
            SlimeCapture slime = hit.collider.GetComponent<SlimeCapture>();

            if (slime != null)
            {
                Rigidbody rb = slime.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    Vector3 dir = (transform.position - slime.transform.position).normalized;

                    rb.linearVelocity = dir * pullForce;
                }

                float dist = Vector3.Distance(transform.position, slime.transform.position);

                if (dist < captureRange)
                {
                    slime.Capture(inventory);
                }
            }
        }
    }
}