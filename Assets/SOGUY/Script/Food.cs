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
        
        // ตั้ง tag เป็น "Food" ถ้ายังไม่มี
        if (!gameObject.CompareTag("Food"))
        {
            gameObject.tag = "Food";
        }
        
        // เพิ่ม Layer สำหรับอาหาร
        gameObject.layer = LayerMask.NameToLayer("Food");
        if (gameObject.layer == -1)
        {
            // ถ้าไม่มี Layer "Food" ให้ใช้ Default layer
            gameObject.layer = 0;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // ตรวจจับเมื่อ slime เข้ามาใกล้
        if (other.CompareTag("Slime"))
        {
            // slime จะจัดการกินอาหารใน SlimeAI script
        }
    }
    
    public void GetEaten()
    {
        if (!isEaten)
        {
            isEaten = true;
            
            // ซ่อนอาหาร
            gameObject.SetActive(false);
            
            // เรียก respawn หลังจากเวลาที่กำหนด
            Invoke("Respawn", respawnTime);
        }
    }
    
    void Respawn()
    {
        isEaten = false;
        transform.position = originalPosition;
        gameObject.SetActive(true);
    }
    
    void OnDrawGizmos()
    {
        // แสดงขอบเขตอาหาร
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}