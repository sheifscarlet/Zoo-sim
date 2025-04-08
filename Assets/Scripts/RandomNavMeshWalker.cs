using UnityEngine;
using UnityEngine.AI;

public class RandomNavMeshWalker : MonoBehaviour
{
    public float walkRadius = 10f;
    public float waitTime = 2f;
    public float rotationSpeed = 5f;
    public bool invertRotation = false; 

    private NavMeshAgent agent;
    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = waitTime;
        MoveToRandomPoint();
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                MoveToRandomPoint();
                timer = waitTime;
            }
        }

        RotateTowardsMovementDirection();
    }

    void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, walkRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void RotateTowardsMovementDirection()
    {
        Vector3 velocity = agent.velocity;
        if (velocity.sqrMagnitude > 0.1f)
        {
            Vector3 direction = velocity.normalized;

            if (invertRotation)
                direction = -direction;

            Quaternion desiredRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSpeed);
        }
    }
}