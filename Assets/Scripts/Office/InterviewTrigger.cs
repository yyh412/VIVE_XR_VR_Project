using UnityEngine;

public class InterviewTrigger : MonoBehaviour
{
    [Header("玩家 Main Camera")]
    [Tooltip("拖入 XR Origin / Camera Offset / Main Camera")]
    public Transform playerHead;

    [Header("面试官欢迎")]
    public InterviewWelcome interviewWelcome;

    [Header("触发区域")]
    [Tooltip("拖入这个 InterviewTrigger 自己的 Box Collider")]
    public BoxCollider triggerArea;

    [Header("调试")]
    public bool showDebugLog = true;

    private bool hasTriggered = false;


    // =====================================================
    // Awake
    // =====================================================

    private void Awake()
    {
        // 如果没有手动拖入，就自动找自己身上的 BoxCollider
        if (triggerArea == null)
        {
            triggerArea = GetComponent<BoxCollider>();
        }
    }


    // =====================================================
    // Update
    // =====================================================

    private void Update()
    {
        if (hasTriggered)
            return;

        if (playerHead == null)
            return;

        if (triggerArea == null)
            return;


        // =================================================
        // 直接检测玩家头的位置是否进入 Box Collider
        //
        // 不依赖 Rigidbody
        // 不依赖 CharacterController
        // 不依赖 OnTriggerEnter
        // =================================================

        bool playerInside =
            triggerArea.bounds.Contains(playerHead.position);


        if (playerInside)
        {
            TriggerInterview();
        }
    }


    // =====================================================
    // 正式触发面试
    // =====================================================

    private void TriggerInterview()
    {
        if (hasTriggered)
            return;

        hasTriggered = true;


        if (showDebugLog)
        {
            Debug.Log(
                "[InterviewTrigger] 玩家 Main Camera 已进入面试区域。"
            );
        }


        // =================================================
        // 开始面试官欢迎流程
        // PlayWelcome 内部会：
        //
        // 1. 记录到达时间
        // 2. 立即停止倒计时
        // 3. 面试官说话
        // 4. 说完后等待
        // 5. 显示 Success / Failed
        // =================================================

        if (interviewWelcome != null)
        {
            interviewWelcome.PlayWelcome();
        }
        else
        {
            Debug.LogWarning(
                "[InterviewTrigger] InterviewWelcome 没有拖入。"
            );
        }
    }
}