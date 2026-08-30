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

    private void Start()
    {
        // =========================
        // 第一张纸
        // =========================

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

        // =========================
        // 第二张纸
        // =========================

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

        // 拿起第一张以后
        // 公文包开始发光
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

        // 第一张必须已经完成
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
        paper01Completed = true;

        if (showDebugLog)
        {
            Debug.Log(
                "第一张纸放入成功"
            );
        }

        // 第一张灯关闭
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

            // 消失，模拟放入公文包
            playerPaper01.SetActive(false);
        }

        // 公文包继续亮
        SetBriefcaseGlow(true);

        // 第二张开始亮
        if (paperGlow02 != null)
        {
            paperGlow02.SetActive(true);
        }

        // 清理状态
        paper01InsideZone = false;
    }

    // =====================================================
    // 第二张完成
    // =====================================================

    private void CompletePaper02()
    {
        paper02Completed = true;

        if (showDebugLog)
        {
            Debug.Log(
                "第二张纸放入成功，帮助完成！"
            );
        }

        // 第二张灯关闭
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

            // 消失
            playerPaper02.SetActive(false);
        }

        // 两张都完成
        // 公文包全部停止发光
        SetBriefcaseGlow(false);

        paper02InsideZone = false;

        // ==========================================
        // 下一步可在这里接：
        // NPC 谢谢
        // NPC 拿包
        // NPC 站起来
        // NPC 离开
        // ==========================================
    }

    // =====================================================
    // 控制包身 + 盖子
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
    // 判断 Collider 是否属于指定纸张
    // 兼容 Collider 在纸的子物体上
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