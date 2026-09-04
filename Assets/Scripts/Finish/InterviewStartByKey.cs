using UnityEngine;

public class InterviewStartByKey : MonoBehaviour
{
    [Header("面试欢迎流程")]
    [Tooltip("拖入面试官身上的 InterviewWelcome")]
    public InterviewWelcome interviewWelcome;

    [Header("玩家")]
    [Tooltip("拖入 XR Origin / Camera Offset / Main Camera")]
    public Transform playerHead;

    [Header("面试官位置")]
    [Tooltip("拖入面试官 Transform")]
    public Transform interviewer;

    [Header("E键提示")]
    [Tooltip("例如 [E] Interview 这个提示物体")]
    public GameObject ePrompt;

    [Header("触发距离")]
    public float interactDistance = 5f;

    [Header("按键")]
    public KeyCode startKey = KeyCode.E;

    private bool hasStarted = false;


    private void Start()
    {
        // 开始时隐藏 E 提示
        if (ePrompt != null)
            ePrompt.SetActive(false);
    }


    private void Update()
    {
        if (hasStarted)
        {
            if (ePrompt != null)
                ePrompt.SetActive(false);

            return;
        }


        if (playerHead == null || interviewer == null)
            return;


        // 计算玩家和面试官距离
        float distance =
            Vector3.Distance(
                playerHead.position,
                interviewer.position
            );


        // =============================================
        // 5米内
        // =============================================

        if (distance <= interactDistance)
        {
            // 显示 E 提示
            if (ePrompt != null)
                ePrompt.SetActive(true);


            // 按 E
            if (Input.GetKeyDown(startKey))
            {
                StartInterview();
            }
        }

        // =============================================
        // 超过5米
        // =============================================

        else
        {
            if (ePrompt != null)
                ePrompt.SetActive(false);
        }
    }


    private void StartInterview()
    {
        if (hasStarted)
            return;

        hasStarted = true;


        // 开始面试后关闭提示
        if (ePrompt != null)
            ePrompt.SetActive(false);


        if (interviewWelcome != null)
        {
            Debug.Log(
                "[InterviewStartByKey] 玩家在5米内按E，开始面试。"
            );

            interviewWelcome.PlayWelcome();
        }
        else
        {
            Debug.LogWarning(
                "[InterviewStartByKey] InterviewWelcome 没有拖入。"
            );
        }
    }
}