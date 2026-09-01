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
            agent.SetDestination(
                waypoints[currentPoint].position
            );
        }
    }

    void Update()
    {
        if (waypoints.Length == 0)
            return;

        if (
            !agent.pathPending &&
            agent.remainingDistance <=
            agent.stoppingDistance
        )
        {
            if (
                currentPoint <
                waypoints.Length - 1
            )
            {
                currentPoint++;

                agent.SetDestination(
                    waypoints[currentPoint].position
                );
            }
        }
    }
}