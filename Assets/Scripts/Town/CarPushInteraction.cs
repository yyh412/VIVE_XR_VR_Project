using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CarPushInteraction : MonoBehaviour
{
    [Header("流程控制")]
    public CarHelpManager carHelpManager;

    [Header("左右手的位置")]
    public Transform leftHand;
    public Transform rightHand;

    [Header("左右控制器")]
    public XRBaseController leftController;
    public XRBaseController rightController;

    [Header("手印物体")]
    public GameObject handPrint;

    [Header("手印材质")]
    public Material blueMaterial;
    public Material redMaterial;

    [Header("检测设置")]
    public float handDetectDistance = 0.30f;

    [Header("第一阶段保持时间")]
    public float holdDuration = 2.0f;

    [Header("持续震动设置")]
    [Range(0f, 1f)]
    public float hapticStrength = 0.35f;

    public float hapticPulseDuration = 0.08f;
    public float hapticInterval = 0.06f;

    // 是否已经允许玩家开始手部推车
    private bool interactionEnabled = false;

    // 当前是否有手在目标范围内
    private bool handIsOnTarget = false;

    // 是否已经达到连续推 2 秒
    private bool twoSecondsReached = false;

    // 当前连续保持时间
    private float holdTimer = 0f;

    // 控制持续震动频率
    private float hapticTimer = 0f;

    private Renderer[] handPrintRenderers;

    private void Start()
    {
        // 获取 HandPrint 的 Renderer
        if (handPrint != null)
        {
            handPrintRenderers =
                handPrint.GetComponentsInChildren<Renderer>(true);

            // 游戏开始时手印隐藏
            handPrint.SetActive(false);
        }
        else
        {
            Debug.LogWarning("CarPushInteraction：HandPrint 没有设置！");
        }

        interactionEnabled = false;
        handIsOnTarget = false;
        twoSecondsReached = false;

        holdTimer = 0f;
        hapticTimer = 0f;
    }

    private void Update()
    {
        // ==================================================
        // 1. 玩家还没到 DriverPushPoint
        // ==================================================
        if (!interactionEnabled)
            return;

        // ==================================================
        // 2. 流程状态不正确
        // ==================================================
        if (carHelpManager == null)
            return;

        if (!carHelpManager.IsStage(
                CarHelpManager.CarHelpStage.ReadyToPush) &&
            !carHelpManager.IsStage(
                CarHelpManager.CarHelpStage.Pushing))
        {
            return;
        }

        // ==================================================
        // 3. 检测左右手距离
        // ==================================================
        bool leftNear = IsHandNear(leftHand);
        bool rightNear = IsHandNear(rightHand);

        bool anyHandNear = leftNear || rightNear;

        // ==================================================
        // 4. 有手正在推
        // ==================================================
        if (anyHandNear)
        {
            // 第一次把手放上去
            if (!handIsOnTarget)
            {
                handIsOnTarget = true;

                holdTimer = 0f;
                hapticTimer = 0f;
                twoSecondsReached = false;

                // 进入 Pushing 阶段
                carHelpManager.SetStage(
                    CarHelpManager.CarHelpStage.Pushing
                );

                // 手印：蓝色 → 红色
                SetHandPrintMaterial(redMaterial);

                Debug.Log(
                    "开始推车：HandPrint 变红"
                );
            }

            // ==================================================
            // 5. 控制器持续震动
            // ==================================================
            hapticTimer -= Time.deltaTime;

            if (hapticTimer <= 0f)
            {
                // 左手正在目标附近
                if (leftNear && leftController != null)
                {
                    leftController.SendHapticImpulse(
                        hapticStrength,
                        hapticPulseDuration
                    );
                }

                // 右手正在目标附近
                if (rightNear && rightController != null)
                {
                    rightController.SendHapticImpulse(
                        hapticStrength,
                        hapticPulseDuration
                    );
                }

                hapticTimer = hapticInterval;
            }

            // ==================================================
            // 6. 连续保持时间累计
            // ==================================================
            holdTimer += Time.deltaTime;

            // ==================================================
            // 7. 连续推满 2 秒
            // ==================================================
            if (holdTimer >= holdDuration &&
                !twoSecondsReached)
            {
                twoSecondsReached = true;

                Debug.Log(
                    "连续推车达到 2 秒！"
                );

                Debug.Log(
                    "下一步：Driver 说『加把劲！』"
                );

                // 下一步我们就在这里继续接：
                // Driver 说“加把劲”
                // 然后再坚持 3 秒
                // 最后车挣扎并向前移动
            }
        }

        // ==================================================
        // 8. 手离开 PushHandTarget
        // ==================================================
        else
        {
            if (handIsOnTarget)
            {
                ResetPush();
            }
        }
    }

    // ======================================================
    // 检测某只手是否靠近 PushHandTarget
    // ======================================================
    private bool IsHandNear(Transform hand)
    {
        if (hand == null)
            return false;

        float distance = Vector3.Distance(
            hand.position,
            transform.position
        );

        return distance <= handDetectDistance;
    }

    // ======================================================
    // 手离开：全部重新开始
    // ======================================================
    private void ResetPush()
    {
        handIsOnTarget = false;

        holdTimer = 0f;
        hapticTimer = 0f;

        twoSecondsReached = false;

        // 手印恢复蓝色
        SetHandPrintMaterial(blueMaterial);

        Debug.Log(
            "手离开 PushHandTarget → 恢复蓝色 → 2秒重新计时"
        );

        // 注意：
        // Stage 保持 Pushing
        // 因为玩家已经到达车后面了。
        //
        // 下一次把手放回来，
        // 会从 0 秒重新开始。
    }

    // ======================================================
    // HelpTrigger：
    // Driver 说完话以后调用
    // ======================================================
    public void ShowHandPrint()
    {
        if (handPrint == null)
        {
            Debug.LogWarning(
                "CarPushInteraction：HandPrint 没有设置！"
            );

            return;
        }

        handPrint.SetActive(true);

        // 默认显示蓝色发光材质
        SetHandPrintMaterial(blueMaterial);

        Debug.Log(
            "Driver Talking 结束 → 蓝色 HandPrint 出现"
        );
    }

    // ======================================================
    // DriverPushPoint：
    // 玩家到车后以后调用
    // ======================================================
    public void EnablePushInteraction()
    {
        interactionEnabled = true;

        handIsOnTarget = false;
        twoSecondsReached = false;

        holdTimer = 0f;
        hapticTimer = 0f;

        // 确保开始时是蓝色
        SetHandPrintMaterial(blueMaterial);

        Debug.Log(
            "玩家到达 DriverPushPoint → 开启手部推车检测"
        );
    }

    // ======================================================
    // 切换 HandPrint 材质
    // ======================================================
    private void SetHandPrintMaterial(Material material)
    {
        if (material == null)
            return;

        if (handPrintRenderers == null)
            return;

        foreach (Renderer renderer in handPrintRenderers)
        {
            renderer.material = material;
        }
    }
}