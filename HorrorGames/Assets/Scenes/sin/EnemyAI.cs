using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform target;

    [Header("Ž‹ŠE")]
    public float viewAngle = 60f;
    public float viewDistance = 10f;

    [Header("œpœj")]
    public float wanderRadius = 10f;
    public float wanderInterval = 3f;

    private NavMeshAgent agent;
    private float wanderTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        wanderTimer = wanderInterval;
    }

    void Update()
    {
        if (target == null) return;

        if (CanSeePlayer())
        {
            // ?? ƒvƒŒƒCƒ„[‚ðŒ©‚½ ¨ ’Ç‚¢‚©‚¯‚é
            agent.SetDestination(target.position);
        }
        else
        {
            // ? Œ©‚¦‚Ä‚È‚¢ ¨ œpœj
            Wander();
        }
    }

    void Wander()
    {
        wanderTimer += Time.deltaTime;

        if (wanderTimer >= wanderInterval)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius);
            agent.SetDestination(newPos);
            wanderTimer = 0;
        }
    }

    Vector3 RandomNavSphere(Vector3 origin, float dist)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMeshHit navHit;

        if (NavMesh.SamplePosition(randDirection, out navHit, dist, NavMesh.AllAreas))
        {
            return navHit.position;
        }

        return origin;
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = target.position - transform.position;
        float distance = directionToPlayer.magnitude;

        if (distance > viewDistance)
            return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle < viewAngle / 2f)
        {
            Ray ray = new Ray(transform.position, directionToPlayer.normalized);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, viewDistance))
            {
                if (hit.transform == target)
                {
                    return true;
                }
            }
        }

        return false;
    }
}