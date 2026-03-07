using UnityEngine;

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

        GameObject newSlime = Instantiate(slimePrefab, dropPoint.position, Quaternion.identity);

        Rigidbody rb = newSlime.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(transform.forward * 6f, ForceMode.Impulse);
        }
    }
}