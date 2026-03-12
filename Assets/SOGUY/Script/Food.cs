using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Food Settings")]
    public int foodValue = 10;
    public float respawnTime = 30f;

    public bool isDropped = false; // เพิ่มตัวนี้

    private Vector3 originalPosition;
    private bool isEaten = false;

    void Start()
    {
        originalPosition = transform.position;

        if (!CompareTag("Food")) gameObject.tag = "Food";

        int foodLayer = LayerMask.NameToLayer("Food");
        if (foodLayer != -1) gameObject.layer = foodLayer;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Slime"))
        {
            // การกินจะถูกจัดการใน SlimeAI
        }
    }

    public void GetEaten()
    {
        if (isEaten) return;
        isEaten = true;
        Debug.Log("Food eaten");
        gameObject.SetActive(false);
        Invoke(nameof(Respawn), respawnTime);
    }

    public void DropFromInventory()
    {
        isDropped = true; // บอกว่านี่คือของที่ Drop ออกมา
        isEaten = false;
        CancelInvoke(nameof(Respawn));
        gameObject.SetActive(true);
    }

    void Respawn()
    {
        if (isDropped)
        {
            Destroy(gameObject); // ของที่ Drop มาพอถูกกินแล้วหายไปเลย
            return;
        }

        isEaten = false;
        transform.position = originalPosition;
        gameObject.SetActive(true);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}