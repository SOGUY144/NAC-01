using UnityEngine;
using System.Collections;

public class SlimeAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;
    public float walkRadius = 10f;
    public float waitTime = 2f;

    [Header("Player Detection")]
    public float detectionRadius = 5f;
    public float lookAtDuration = 3f;

    [Header("Food Settings")]
    public float foodDetectionRadius = 8f;
    public float eatRadius = 1f;
    public LayerMask foodLayer;

    private Vector3 randomDirection;
    private bool isWalking = false;
    private bool isLookingAtPlayer = false;
    private bool isGoingToFood = false;
    private Transform player;
    private Transform targetFood;
    private float waitTimer;
    private float lookAtTimer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        waitTimer = waitTime;
        StartCoroutine(RandomMovement());
    }

    void Update()
    {
        CheckForPlayer();
        CheckForFood();

        if (!isLookingAtPlayer && !isGoingToFood && !isWalking)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                StartCoroutine(RandomMovement());
                waitTimer = waitTime;
            }
        }
    }

    IEnumerator RandomMovement()
    {
        isWalking = true;

        // สุ่มตำแหน่งใหม่
        Vector3 randomPos = Random.insideUnitSphere * walkRadius;
        randomPos += transform.position;
        randomPos.y = transform.position.y;

        Vector3 direction = (randomPos - transform.position).normalized;

        // เดินไปยังตำแหน่งที่สุ่ม
        float walkTime = Random.Range(3f, 6f);
        float timer = 0;

        while (timer < walkTime && !isLookingAtPlayer && !isGoingToFood)
        {
            // หันไปทางที่จะเดิน
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // เดินไปข้างหน้า
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null;
        }

        isWalking = false;
    }

    void CheckForPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius && !isLookingAtPlayer)
        {
            StartCoroutine(LookAtPlayer());
        }
    }

    IEnumerator LookAtPlayer()
    {
        isLookingAtPlayer = true;
        lookAtTimer = lookAtDuration;

        while (lookAtTimer > 0)
        {
            // หันไปหาผู้เล่น
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            lookAtTimer -= Time.deltaTime;
            yield return null;
        }

        isLookingAtPlayer = false;
    }

    void CheckForFood()
    {
        if (isGoingToFood) return;

        Collider[] foodColliders = Physics.OverlapSphere(transform.position, foodDetectionRadius, foodLayer);

        if (foodColliders.Length > 0)
        {
            // หาอาหารที่ใกล้ที่สุด
            float closestDistance = Mathf.Infinity;
            Transform closestFood = null;

            foreach (Collider food in foodColliders)
            {
                float distance = Vector3.Distance(transform.position, food.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestFood = food.transform;
                }
            }

            if (closestFood != null)
            {
                targetFood = closestFood;
                StartCoroutine(GoToFood());
            }
        }
    }

    IEnumerator GoToFood()
    {
        isGoingToFood = true;

        while (targetFood != null && Vector3.Distance(transform.position, targetFood.position) > eatRadius)
        {
            // เดินไปหาอาหาร
            Vector3 directionToFood = (targetFood.position - transform.position).normalized;
            directionToFood.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(directionToFood);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            transform.Translate(Vector3.forward * moveSpeed * 1.5f * Time.deltaTime);

            yield return null;
        }

        // กินอาหาร
        if (targetFood != null)
        {
            EatFood();
        }

        isGoingToFood = false;
        targetFood = null;
    }

    void EatFood()
    {
        if (targetFood != null)
        {
            // ทำลายอาหาร
            Destroy(targetFood.gameObject);

            // เพิ่มเอฟเฟกต์หรือเสียงตอนกินได้ที่นี่
            Debug.Log("Slime กินอาหารแล้ว!");
        }
    }

    void OnDrawGizmosSelected()
    {
        // แสดงรัศมีตรวจจับผู้เล่น
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // แสดงรัศมีตรวจจับอาหาร
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, foodDetectionRadius);

        // แสดงรัศมีกินอาหาร
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, eatRadius);

        // แสดงรัศมีเดินสุ่ม
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, walkRadius);
    }
}