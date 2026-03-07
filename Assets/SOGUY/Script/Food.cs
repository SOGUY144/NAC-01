using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Food Settings")]
    public int foodValue = 10;
    public float respawnTime = 30f;

    private Vector3 originalPosition;
    private bool isEaten = false;

    void Start()
    {
        originalPosition = transform.position;

        // ตั้ง Tag เป็น Food ถ้ายังไม่ได้ตั้ง
        if (!CompareTag("Food"))
        {
            gameObject.tag = "Food";
        }

        // ตั้ง Layer เป็น Food ถ้ามี
        int foodLayer = LayerMask.NameToLayer("Food");

        if (foodLayer != -1)
        {
            gameObject.layer = foodLayer;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Slime"))
        {
            // Slime จะจัดการกินใน SlimeAI
        }
    }

    public void GetEaten()
    {
        if (isEaten) return;

        isEaten = true;

        Debug.Log("Food eaten");

        // ซ่อนอาหาร
        gameObject.SetActive(false);

        // Respawn
        Invoke(nameof(Respawn), respawnTime);
    }

    void Respawn()
    {
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