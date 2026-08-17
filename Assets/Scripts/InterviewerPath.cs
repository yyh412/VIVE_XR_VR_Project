using UnityEngine;
using UnityEngine.AI;

public class InterviewerPath : MonoBehaviour
{
    public Transform[] waypoints;

    private NavMeshAgent agent;
    private int currentPoint = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentPoint].position);
        }
    }

    void Update()
    {
        if (waypoints.Length == 0)
            return;

        // 到达当前路点
        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            // 如果后面还有路点
            if (currentPoint < waypoints.Length - 1)
            {
                currentPoint++;
                agent.SetDestination(waypoints[currentPoint].position);
            }
        }
    }
}