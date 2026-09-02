using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MovableBoxTask : MonoBehaviour
{
    [Header("箱子抓取组件")]
    public XRGrabInteractable grabInteractable;

    [Header("箱子呼吸灯")]
    public BoxGlowPulse boxGlow;

    [Header("这个箱子对应的正确 Socket")]
    public XRSocketInteractor socket;

    [Header("目标位置提示灯")]
    public GameObject targetGlow;

    [Header("总任务管理")]
    public BoxTaskManager taskManager;

    [Header("状态")]
    public bool taskStarted = false;
    public bool placed = false;

    private Rigidbody rb;


    // =====================================================
    // Awake
    // =====================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (targetGlow != null)
            targetGlow.SetActive(false);

        if (boxGlow != null)
            boxGlow.StopGlow();

        taskStarted = false;
        placed = false;
    }


    // =====================================================
    // 注册事件
    // =====================================================

    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }

        if (socket != null)
        {
            socket.selectEntered.AddListener(OnCorrectSocketEntered);
        }
    }


    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }

        if (socket != null)
        {
            socket.selectEntered.RemoveListener(OnCorrectSocketEntered);
        }
    }


    // =====================================================
    // 开始箱子任务
    // =====================================================

    public void BeginTask()
    {
        if (placed)
            return;

        taskStarted = true;

        if (boxGlow != null)
            boxGlow.StartGlow();

        if (targetGlow != null)
            targetGlow.SetActive(false);

        Debug.Log(
            "[MovableBoxTask] " +
            gameObject.name +
            " 开始任务。"
        );
    }


    // =====================================================
    // 玩家拿起箱子
    // =====================================================

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (!taskStarted)
            return;

        if (placed)
            return;

        if (boxGlow != null)
            boxGlow.StopGlow();

        if (targetGlow != null)
            targetGlow.SetActive(true);

        Debug.Log(
            "[MovableBoxTask] " +
            gameObject.name +
            " 被拿起。"
        );
    }


    // =====================================================
    // 玩家放下箱子
    // =====================================================

    private void OnReleased(SelectExitEventArgs args)
    {
        if (!taskStarted)
            return;

        if (placed)
            return;

        // 如果正确 Socket 正在抓这个箱子，
        // 就不要重新打开呼吸灯
        if (socket != null &&
            socket.hasSelection)
        {
            return;
        }

        if (boxGlow != null)
            boxGlow.StartGlow();

        if (targetGlow != null)
            targetGlow.SetActive(false);

        Debug.Log(
            "[MovableBoxTask] " +
            gameObject.name +
            " 被放下，但没有进入正确位置。"
        );
    }


    // =====================================================
    // 进入正确 Socket
    // =====================================================

    private void OnCorrectSocketEntered(
        SelectEnterEventArgs args
    )
    {
        if (!taskStarted)
            return;

        if (placed)
            return;

        if (grabInteractable == null)
            return;


        IXRSelectInteractable enteredInteractable =
            args.interactableObject;


        if (enteredInteractable == null)
            return;


        // =================================================
        // 非常重要：
        // 必须确认进入这个 Socket 的
        // 就是“我自己这个箱子”
        // =================================================

        if (enteredInteractable !=
            (IXRSelectInteractable)grabInteractable)
        {
            Debug.Log(
                "[MovableBoxTask] " +
                socket.gameObject.name +
                " 进入了错误箱子，忽略。"
            );

            return;
        }


        // =================================================
        // 正确箱子进入正确 Socket
        // =================================================

        placed = true;


        if (boxGlow != null)
            boxGlow.StopGlow();

        if (targetGlow != null)
            targetGlow.SetActive(false);


        // 停止箱子物理运动
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }


        Debug.Log(
            "[MovableBoxTask] " +
            gameObject.name +
            " 已进入正确的 " +
            socket.gameObject.name +
            "。"
        );


        // 通知总任务
        if (taskManager != null)
        {
            taskManager.BoxCompleted(this);
        }
        else
        {
            Debug.LogWarning(
                "[MovableBoxTask] Task Manager 没有拖入。"
            );
        }
    }
}