using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BriefcasePaperDropZone : MonoBehaviour
{
    [Header("两张需要收集的纸")]
    public GameObject playerPaper01;
    public GameObject playerPaper02;

    [Header("两张纸的提示灯")]
    public GameObject paperGlow01;
    public GameObject paperGlow02;

    [Header("公文包蓝色发光")]
    public GameObject briefcaseGlowBody;
    public GameObject briefcaseGlowLid;

    [Header("NPC完成帮助后的动画")]
    [Tooltip("拖入 mansuit@Walking 上的 Animator")]
    public Animator npcAnimator;

    [Tooltip("Holding Paper Idle → Standing 使用的 Trigger")]
    public string standUpTrigger = "StandUp";

    [Header("放入容错")]
    [Tooltip("纸刚离开 DropZone 后，在这段时间内松手仍然算放入")]
    public float dropGraceTime = 0.4f;

    [Header("调试")]
    public bool showDebugLog = true;


    private XRGrabInteractable grab01;
    private XRGrabInteractable grab02;

    private bool paper01InsideZone = false;
    private bool paper02InsideZone = false;

    private float paper01LastInsideTime = -999f;
    private float paper02LastInsideTime = -999f;

    private bool paper01Completed = false;
    private bool paper02Completed = false;

    private bool standUpTriggered = false;


    // =====================================================
    // Start
    // =====================================================

    private void Start()
    {
        // 第一张纸
        if (playerPaper01 != null)
        {
            grab01 =
                playerPaper01.GetComponent<XRGrabInteractable>();

            if (grab01 != null)
            {
                grab01.selectEntered.AddListener(
                    OnPaper01PickedUp
                );

                grab01.selectExited.AddListener(
                    OnPaper01Released
                );
            }
            else
            {
                Debug.LogWarning(
                    "PlayerPaper_01 没有 XRGrabInteractable"
                );
            }
        }


        // 第二张纸
        if (playerPaper02 != null)
        {
            grab02 =
                playerPaper02.GetComponent<XRGrabInteractable>();

            if (grab02 != null)
            {
                grab02.selectExited.AddListener(
                    OnPaper02Released
                );
            }
            else
            {
                Debug.LogWarning(
                    "PlayerPaper_02 没有 XRGrabInteractable"
                );
            }
        }


        // 第二张一开始不亮
        if (paperGlow02 != null)
        {
            paperGlow02.SetActive(false);
        }


        // 公文包一开始不亮
        SetBriefcaseGlow(false);
    }


    // =====================================================
    // OnDestroy
    // =====================================================

    private void OnDestroy()
    {
        if (grab01 != null)
        {
            grab01.selectEntered.RemoveListener(
                OnPaper01PickedUp
            );

            grab01.selectExited.RemoveListener(
                OnPaper01Released
            );
        }


        if (grab02 != null)
        {
            grab02.selectExited.RemoveListener(
                OnPaper02Released
            );
        }
    }


    // =====================================================
    // 第一张纸被拿起
    // =====================================================

    private void OnPaper01PickedUp(
        SelectEnterEventArgs args
    )
    {
        if (paper01Completed)
            return;


        SetBriefcaseGlow(true);


        if (showDebugLog)
        {
            Debug.Log(
                "拿起第一张纸，公文包开始发光"
            );
        }
    }


    // =====================================================
    // 纸进入 DropZone
    // =====================================================

    private void OnTriggerEnter(Collider other)
    {
        // 第一张
        if (
            playerPaper01 != null &&
            !paper01Completed &&
            IsPaperCollider(
                other,
                playerPaper01
            )
        )
        {
            paper01InsideZone = true;
            paper01LastInsideTime = Time.time;

            if (showDebugLog)
            {
                Debug.Log(
                    "PlayerPaper_01 进入 PaperDropZone"
                );
            }
        }


        // 第二张
        if (
            playerPaper02 != null &&
            !paper02Completed &&
            IsPaperCollider(
                other,
                playerPaper02
            )
        )
        {
            paper02InsideZone = true;
            paper02LastInsideTime = Time.time;

            if (showDebugLog)
            {
                Debug.Log(
                    "PlayerPaper_02 进入 PaperDropZone"
                );
            }
        }
    }


    // =====================================================
    // 纸持续停留在 DropZone
    // =====================================================

    private void OnTriggerStay(Collider other)
    {
        if (
            playerPaper01 != null &&
            !paper01Completed &&
            IsPaperCollider(
                other,
                playerPaper01
            )
        )
        {
            paper01InsideZone = true;
            paper01LastInsideTime = Time.time;
        }


        if (
            playerPaper02 != null &&
            !paper02Completed &&
            IsPaperCollider(
                other,
                playerPaper02
            )
        )
        {
            paper02InsideZone = true;
            paper02LastInsideTime = Time.time;
        }
    }


    // =====================================================
    // 纸离开 DropZone
    // =====================================================

    private void OnTriggerExit(Collider other)
    {
        if (
            playerPaper01 != null &&
            IsPaperCollider(
                other,
                playerPaper01
            )
        )
        {
            paper01InsideZone = false;

            if (showDebugLog)
            {
                Debug.Log(
                    "PlayerPaper_01 离开 PaperDropZone"
                );
            }
        }


        if (
            playerPaper02 != null &&
            IsPaperCollider(
                other,
                playerPaper02
            )
        )
        {
            paper02InsideZone = false;

            if (showDebugLog)
            {
                Debug.Log(
                    "PlayerPaper_02 离开 PaperDropZone"
                );
            }
        }
    }


    // =====================================================
    // 第一张纸松手
    // =====================================================

    private void OnPaper01Released(
        SelectExitEventArgs args
    )
    {
        if (paper01Completed)
            return;


        bool canDrop =
            paper01InsideZone ||
            Time.time - paper01LastInsideTime
            <= dropGraceTime;


        if (!canDrop)
        {
            if (showDebugLog)
            {
                Debug.Log(
                    "第一张松手，但不在 PaperDropZone"
                );
            }

            return;
        }


        CompletePaper01();
    }


    // =====================================================
    // 第二张纸松手
    // =====================================================

    private void OnPaper02Released(
        SelectExitEventArgs args
    )
    {
        if (paper02Completed)
            return;


        // 第一张必须先完成
        if (!paper01Completed)
        {
            if (showDebugLog)
            {
                Debug.Log(
                    "第一张还没完成，暂时不能放第二张"
                );
            }

            return;
        }


        bool canDrop =
            paper02InsideZone ||
            Time.time - paper02LastInsideTime
            <= dropGraceTime;


        if (!canDrop)
        {
            if (showDebugLog)
            {
                Debug.Log(
                    "第二张松手，但不在 PaperDropZone"
                );
            }

            return;
        }


        CompletePaper02();
    }


    // =====================================================
    // 第一张完成
    // =====================================================

    private void CompletePaper01()
    {
        if (paper01Completed)
            return;


        paper01Completed = true;


        if (showDebugLog)
        {
            Debug.Log(
                "第一张纸放入成功"
            );
        }


        // 第一张提示灯关闭
        if (paperGlow01 != null)
        {
            paperGlow01.SetActive(false);
        }


        // 第一张永久完成
        if (playerPaper01 != null)
        {
            PaperPickupGlowController controller =
                playerPaper01.GetComponent<
                    PaperPickupGlowController
                >();


            if (controller != null)
            {
                controller.MarkCompleted();
            }


            // 消失 = 已经放进公文包
            playerPaper01.SetActive(false);
        }


        // 公文包继续发光
        SetBriefcaseGlow(true);


        // 第二张提示灯开始亮
        if (paperGlow02 != null)
        {
            paperGlow02.SetActive(true);
        }


        paper01InsideZone = false;
    }


    // =====================================================
    // 第二张完成
    // =====================================================

    private void CompletePaper02()
    {
        if (paper02Completed)
            return;


        paper02Completed = true;


        if (showDebugLog)
        {
            Debug.Log(
                "第二张纸放入成功，帮助完成！"
            );
        }


        // 第二张提示灯关闭
        if (paperGlow02 != null)
        {
            paperGlow02.SetActive(false);
        }


        // 第二张永久完成
        if (playerPaper02 != null)
        {
            PaperPickupGlowController controller =
                playerPaper02.GetComponent<
                    PaperPickupGlowController
                >();


            if (controller != null)
            {
                controller.MarkCompleted();
            }


            // 消失 = 已经放进公文包
            playerPaper02.SetActive(false);
        }


        // 两张都完成
        // 公文包停止发光
        SetBriefcaseGlow(false);

        paper02InsideZone = false;


        // =====================================================
        // 新增：
        // 记录玩家已经帮助过这个掉文件的人
        // =====================================================

        HelpRecord.HelpedMansuit = true;

        Debug.Log(
            "[HelpRecord] HelpedMansuit = TRUE"
        );


        // =====================================================
        // 帮助完成
        // Holding Paper Idle
        // → StandUp
        // → Standing
        // → Talking
        // =====================================================

        TriggerNPCStandUp();
    }


    // =====================================================
    // Desktop模式快捷完成
    // 以后按E调用这个函数
    // =====================================================

    public void DesktopCompleteAllPapers()
    {
        if (
            paper01Completed &&
            paper02Completed
        )
        {
            return;
        }


        if (showDebugLog)
        {
            Debug.Log(
                "[Desktop] 按E：直接完成文件帮助任务"
            );
        }


        // 第一张直接完成
        if (!paper01Completed)
        {
            CompletePaper01();
        }


        // 第二张直接完成
        if (!paper02Completed)
        {
            CompletePaper02();
        }


        if (showDebugLog)
        {
            Debug.Log(
                "[Desktop] 两张纸已消失，NPC开始站起来"
            );
        }
    }


    // =====================================================
    // NPC站起来
    // =====================================================

    private void TriggerNPCStandUp()
    {
        if (standUpTriggered)
            return;


        standUpTriggered = true;


        if (npcAnimator == null)
        {
            Debug.LogWarning(
                "BriefcasePaperDropZone：NPC Animator 没有设置！"
            );

            return;
        }


        if (string.IsNullOrEmpty(standUpTrigger))
        {
            Debug.LogWarning(
                "BriefcasePaperDropZone：StandUp Trigger 名称为空！"
            );

            return;
        }


        npcAnimator.SetTrigger(
            standUpTrigger
        );


        if (showDebugLog)
        {
            Debug.Log(
                "帮助完成 → Animator Trigger："
                + standUpTrigger
            );
        }
    }


    // =====================================================
    // 控制公文包发光
    // =====================================================

    private void SetBriefcaseGlow(bool state)
    {
        if (briefcaseGlowBody != null)
        {
            briefcaseGlowBody.SetActive(state);
        }


        if (briefcaseGlowLid != null)
        {
            briefcaseGlowLid.SetActive(state);
        }
    }


    // =====================================================
    // 判断Collider是否属于对应纸张
    // =====================================================

    private bool IsPaperCollider(
        Collider other,
        GameObject targetPaper
    )
    {
        if (
            other == null ||
            targetPaper == null
        )
        {
            return false;
        }


        if (
            other.gameObject ==
            targetPaper
        )
        {
            return true;
        }


        Transform current =
            other.transform;


        while (current != null)
        {
            if (
                current.gameObject ==
                targetPaper
            )
            {
                return true;
            }


            current =
                current.parent;
        }


        return false;
    }
}