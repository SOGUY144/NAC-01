using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SlimeAI : MonoBehaviour
{
    public enum SlimeState
    {
        Idle,
        Wander,
        ChaseFood,
        Eat,
        LookPlayer
    }

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float walkRadius = 10f;
    public float waitTime = 2f;

    [Header("Player Detection")]
    public float detectionRadius = 5f;
    public float lookTime = 2f;

    [Header("Food")]
    public float foodDetectRadius = 8f;
    public float eatDistance = 1f;
    public LayerMask foodLayer;

    private NavMeshAgent agent;
    private Transform player;
    private Transform targetFood;

    private SlimeState currentState;

    private SlimeHunger slimeHunger;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = 0.5f;

        player = GameObject.FindGameObjectWithTag("Player").transform;

        slimeHunger = GetComponent<SlimeHunger>();

        ChangeState(SlimeState.Idle);
    }

    void Update()
    {
        CheckPlayer();
        CheckFood();
    }

    void ChangeState(SlimeState newState)
    {
        currentState = newState;

        StopAllCoroutines();

        switch (newState)
        {
            case SlimeState.Idle:
                StartCoroutine(IdleState());
                break;

            case SlimeState.Wander:
                StartCoroutine(WanderState());
                break;

            case SlimeState.ChaseFood:
                StartCoroutine(ChaseFoodState());
                break;

            case SlimeState.Eat:
                StartCoroutine(EatState());
                break;

            case SlimeState.LookPlayer:
                StartCoroutine(LookPlayerState());
                break;
        }
    }

    IEnumerator IdleState()
    {
        yield return new WaitForSeconds(waitTime);
        ChangeState(SlimeState.Wander);
    }

    IEnumerator WanderState()
    {
        if (!agent.isOnNavMesh)
            yield break;

        Vector3 randomPoint = Random.insideUnitSphere * walkRadius;
        randomPoint += transform.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomPoint, out hit, walkRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        while (agent.pathPending || agent.remainingDistance > 0.5f)
        {
            yield return null;
        }

        ChangeState(SlimeState.Idle);
    }

    IEnumerator ChaseFoodState()
    {
        while (targetFood != null)
        {
            agent.SetDestination(targetFood.position);

            float distance = Vector3.Distance(transform.position, targetFood.position);

            if (distance < eatDistance)
            {
                ChangeState(SlimeState.Eat);
                yield break;
            }

            yield return null;
        }

        ChangeState(SlimeState.Idle);
    }

    IEnumerator EatState()
    {
        agent.ResetPath();

        Food food = targetFood.GetComponent<Food>();

        if (food != null)
        {
            yield return new WaitForSeconds(1f);

            // เพิ่มค่า Hunger
            if (slimeHunger != null)
            {
                slimeHunger.Eat(food.foodValue);
            }

            food.GetEaten();
        }

        targetFood = null;

        ChangeState(SlimeState.Idle);
    }

    IEnumerator LookPlayerState()
    {
        float timer = lookTime;

        while (timer > 0)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0;

            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 4f);

            timer -= Time.deltaTime;

            yield return null;
        }

        ChangeState(SlimeState.Idle);
    }

    void CheckPlayer()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < detectionRadius && currentState != SlimeState.LookPlayer)
        {
            ChangeState(SlimeState.LookPlayer);
        }
    }

    void CheckFood()
    {

        SlimeHunger hunger = GetComponent<SlimeHunger>();

        if (hunger != null)
        {
            // ถ้ายังอิ่มอยู่ไม่ต้องหาอาหาร
            if (hunger.currentHunger > 0f)
                return;
        }

        if (currentState == SlimeState.ChaseFood || currentState == SlimeState.Eat)
            return;

        Collider[] foods = Physics.OverlapSphere(transform.position, foodDetectRadius, foodLayer);

        if (foods.Length > 0)
        {
            targetFood = foods[0].transform;
            ChangeState(SlimeState.ChaseFood);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, foodDetectRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, walkRadius);
    }
}