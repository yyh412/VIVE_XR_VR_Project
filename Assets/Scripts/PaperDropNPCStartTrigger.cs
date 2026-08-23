using UnityEngine;
using UnityEngine.AI;

public class PaperDropNPCStartTrigger : MonoBehaviour
{
    [Header("玩家 XR Rig")]
    public Transform playerRoot;

    [Header("文件散落事件 NPC")]
    public GameObject npc;

    private NavMeshAgent npcAgent;
    private InterviewerPath interviewerPath;

    private bool triggered = false;

    void Awake()
    {
        if (npc == null)
        {
            Debug.LogWarning("PaperDropNPCStartTrigger：没有设置 NPC");
            return;
        }

        npcAgent = npc.GetComponent<NavMeshAgent>();
        interviewerPath = npc.GetComponent<InterviewerPath>();

        // 游戏开始时不让NPC走
        if (interviewerPath != null)
        {
            interviewerPath.enabled = false;
        }

        if (npcAgent != null)
        {
            npcAgent.isStopped = true;
            npcAgent.velocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("PaperDropStartZone碰到了：" + other.name);

        if (triggered)
            return;

        if (playerRoot == null)
        {
            Debug.LogWarning("没有设置 Player Root");
            return;
        }

        // 检查进入Trigger的Collider是不是玩家本体或玩家的子物体
        if (other.transform == playerRoot ||
            other.transform.IsChildOf(playerRoot))
        {
            StartPaperDropNPC();
        }
    }

    private void StartPaperDropNPC()
    {
        triggered = true;

        Debug.Log("成功触发！文件散落NPC开始移动");

        if (npcAgent != null)
        {
            npcAgent.isStopped = false;
        }

        if (interviewerPath != null)
        {
            interviewerPath.enabled = true;
        }
    }
}