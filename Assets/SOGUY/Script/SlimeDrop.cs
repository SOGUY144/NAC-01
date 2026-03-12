using UnityEngine;
using UnityEngine.AI;

public class SlimeDrop : MonoBehaviour
{
    public PlayerInventory inventory;
    public Transform dropPoint;

    public GameObject waterSlimePrefab;
    public GameObject fireSlimePrefab;
    public GameObject greenSlimePrefab;
    public GameObject bananaPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) DropSlime();
        if (Input.GetKeyDown(KeyCode.E)) DropFood();
    }

    void DropSlime()
    {
        GameObject prefabToSpawn = null;

        if (inventory.waterSlime > 0)
        {
            inventory.waterSlime--;
            prefabToSpawn = waterSlimePrefab;
        }
        else if (inventory.fireSlime > 0)
        {
            inventory.fireSlime--;
            prefabToSpawn = fireSlimePrefab;
        }
        else if (inventory.greenSlime > 0)
        {
            inventory.greenSlime--;
            prefabToSpawn = greenSlimePrefab;
        }

        if (prefabToSpawn == null)
        {
            Debug.Log("ไม่มีสไลม์ใน Inventory!");
            return;
        }

        SpawnObject(prefabToSpawn, false);
    }

    void DropFood()
    {
        if (inventory.banana <= 0)
        {
            Debug.Log("ไม่มีกล้วยใน Inventory!");
            return;
        }

        inventory.banana--;
        SpawnObject(bananaPrefab, true);
    }

    void SpawnObject(GameObject prefab, bool isFood)
    {
        if (prefab == null) return;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(dropPoint.position, out hit, 5f, NavMesh.AllAreas))
        {
            GameObject obj = Instantiate(prefab, hit.position + Vector3.up * 0.3f, Quaternion.identity);

            if (isFood)
            {
                Food food = obj.GetComponent<Food>();
                if (food != null) food.DropFromInventory(); // set isDropped = true ด้วย
            }

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce((transform.forward + Vector3.up) * 6f, ForceMode.Impulse);
            }
        }
        else
        {
            Debug.LogWarning("No NavMesh near drop point!");
        }
    }
}