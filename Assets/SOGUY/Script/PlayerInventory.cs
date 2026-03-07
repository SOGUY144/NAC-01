using UnityEngine;
using UnityEngine.AI;

public class PlayerInventory : MonoBehaviour
{
    public int slimeCount = 0;
    public int maxSlime = 10;

    public GameObject slimePrefab;
    public Transform dropPoint;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropSlime();
        }
    }

    public void AddSlime(GameObject slime)
    {
        if (slimeCount >= maxSlime) return;

        slimeCount++;

        Debug.Log("Captured Slime! Total: " + slimeCount);
    }

    void DropSlime()
    {
        if (slimeCount <= 0) return;

        slimeCount--;

        NavMeshHit hit;

        // หาตำแหน่ง NavMesh ใกล้ dropPoint
        if (NavMesh.SamplePosition(dropPoint.position, out hit, 5f, NavMesh.AllAreas))
        {
            Vector3 spawnPos = hit.position + Vector3.up * 0.3f;

            GameObject newSlime = Instantiate(slimePrefab, spawnPos, Quaternion.identity);

            Rigidbody rb = newSlime.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // ยิง slime ออกไปข้างหน้า + ขึ้นนิดหน่อย
                rb.AddForce((transform.forward + Vector3.up) * 6f, ForceMode.Impulse);
            }
        }
        else
        {
            Debug.LogWarning("No NavMesh near drop point!");
        }
    }
}