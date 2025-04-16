using UnityEngine;
using UnityEngine.AI;

public class Wander : MonoBehaviour
{
    private NavMeshAgent agent;
    public float wanderRadius = 20f; // Max distance to pick a random point
    public float wanderTimer = 5f; // Time before picking a new destination

    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
        SetNewDestination();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Pick a new destination after the timer expires or if the agent reached the current destination
        if (timer >= wanderTimer || !agent.hasPath)
        {
            SetNewDestination();
            timer = 0f;
        }
    }

    void SetNewDestination()
    {
        Vector3 randomPoint = GetRandomPointOnNavMesh(transform.position, wanderRadius);
        if (randomPoint != Vector3.zero)
        {
            agent.SetDestination(randomPoint);
        }
    }

    Vector3 GetRandomPointOnNavMesh(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius + center;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return Vector3.zero; // Fallback (rare case)
    }
}