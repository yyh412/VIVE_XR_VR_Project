using UnityEngine;
using UnityEngine.AI;

public class NPCWalkToTarget : MonoBehaviour
{
    [Header("按顺序走的路径点")]
    public Transform[] waypoints;

    [Header("NavMesh Agent")]
    public NavMeshAgent agent;

    [Header("到达每个点的距离")]
    public float arriveDistance = 0.2f;

    private int currentIndex = 0;
    private bool finished = false;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("NPC：没有 NavMeshAgent！");
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("NPC：没有设置 Waypoints！");
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("NPC：没有站在 NavMesh 上！");
            return;
        }

        currentIndex = 0;
        MoveToCurrentPoint();
    }

    void Update()
    {
        if (finished || agent == null)
            return;

        if (!agent.isOnNavMesh || agent.pathPending)
            return;

        if (agent.remainingDistance <= arriveDistance)
        {
            currentIndex++;

            if (currentIndex >= waypoints.Length)
            {
                finished = true;
                agent.isStopped = true;
                return;
            }

            MoveToCurrentPoint();
        }
    }

    void MoveToCurrentPoint()
    {
        if (waypoints[currentIndex] == null)
            return;

        agent.isStopped = false;
        agent.SetDestination(waypoints[currentIndex].position);
    }
}