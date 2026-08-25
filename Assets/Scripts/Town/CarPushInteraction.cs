using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CarPushInteraction : MonoBehaviour
{
    [Header("流程控制")]
    public CarHelpManager carHelpManager;

    [Header("Driver")]
    public Animator driverAnimator;

    [Header("字幕气泡")]
    public DriverSpeechBubble speechBubble;

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

    [Header("第一阶段：多久后说 Keep pushing")]
    public float encourageDelay = 2.0f;

    [Header("第二阶段：Keep pushing 后继续推多久")]
    public float struggleDuration = 3.0f;

    [Header("持续震动设置")]
    [Range(0f, 1f)]
    public float hapticStrength = 0.35f;

    public float hapticPulseDuration = 0.08f;
    public float hapticInterval = 0.06f;

    private bool interactionEnabled = false;
    private bool handIsOnTarget = false;

    private bool encourageTriggered = false;
    private bool pushFinished = false;

    private float pushTimer = 0f;
    private float hapticTimer = 0f;

    private Renderer[] handPrintRenderers;

    private void Start()
    {
        if (handPrint != null)
        {
            handPrintRenderers =
                handPrint.GetComponentsInChildren<Renderer>(true);

            // 游戏开始隐藏手印
            handPrint.SetActive(false);
        }

        ResetInternalState();
    }

    private void Update()
    {
        // 还没到 DriverPushPoint
        if (!interactionEnabled)
            return;

        if (carHelpManager == null)
            return;

        // 只有 ReadyToPush / Pushing 阶段才能推
        if (!carHelpManager.IsStage(
                CarHelpManager.CarHelpStage.ReadyToPush) &&
            !carHelpManager.IsStage(
                CarHelpManager.CarHelpStage.Pushing))
        {
            return;
        }

        bool leftNear = IsHandNear(leftHand);
        bool rightNear = IsHandNear(rightHand);

        bool anyHandNear =
            leftNear || rightNear;

        // ==================================================
        // 玩家正在推
        // ==================================================
        if (anyHandNear)
        {
            // 第一次把手放上去
            if (!handIsOnTarget)
            {
                handIsOnTarget = true;

                pushTimer = 0f;
                hapticTimer = 0f;

                encourageTriggered = false;
                pushFinished = false;

                carHelpManager.SetStage(
                    CarHelpManager.CarHelpStage.Pushing
                );

                // 蓝色手印 → 红色
                SetHandPrintMaterial(redMaterial);

                Debug.Log("开始推车，手印变红");
            }

            // 持续震动
            UpdateHaptics(
                leftNear,
                rightNear
            );

            // 推车计时
            pushTimer += Time.deltaTime;

            // ==================================================
            // 第一阶段：持续推2秒
            // ==================================================
            if (pushTimer >= encourageDelay &&
                !encourageTriggered)
            {
                encourageTriggered = true;

                DriverEncourage();
            }

            // ==================================================
            // 第二阶段：再持续推3秒
            // ==================================================
            float totalRequiredTime =
                encourageDelay +
                struggleDuration;

            if (pushTimer >= totalRequiredTime &&
                !pushFinished)
            {
                pushFinished = true;

                PushSucceeded();
            }
        }

        // ==================================================
        // 手移开
        // ==================================================
        else
        {
            if (handIsOnTarget &&
                !pushFinished)
            {
                ResetPush();
            }
        }
    }

    // ======================================================
    // 2秒后：Keep pushing!
    // ======================================================
    private void DriverEncourage()
    {
        Debug.Log("Driver: Keep pushing!");

        if (speechBubble != null)
        {
            speechBubble.ShowEncourageMessage();
        }
    }

    // ======================================================
    // 2秒 + 3秒完成
    // ======================================================
    private void PushSucceeded()
    {
        Debug.Log(
            "持续推车成功 → Driver 切换到 Push Stop"
        );

        // 不再检测玩家手
        interactionEnabled = false;

        // 隐藏手印
        if (handPrint != null)
        {
            handPrint.SetActive(false);
        }

        // 如果 Keep pushing 还在显示，可以关掉
        if (speechBubble != null)
        {
            speechBubble.HideBubble();
        }

        // ==========================================
        // 最关键：
        // 从 Push_InPlace 切到 Push Stop
        // ==========================================
        if (driverAnimator != null)
        {
            driverAnimator.SetTrigger("StopPush");

            Debug.Log(
                "StopPush Trigger 已发送"
            );
        }
        else
        {
            Debug.LogWarning(
                "Driver Animator 没有设置！"
            );
        }

        // 这里不要控制汽车
        // 也不要在这里 SetDriving()
        //
        // PushStopCarSync 会在检测到
        // Push Stop 真正开始播放时：
        //
        // 1. 四轮 SetDriving()
        // 2. Driver 按 Root Motion 前进
        // 3. car 同步同样位移
        // 4. Push Stop 结束后 StopWheels()

        if (carHelpManager != null)
        {
            carHelpManager.SetStage(
                CarHelpManager.CarHelpStage.Finished
            );
        }
    }

    // ======================================================
    // 持续震动
    // ======================================================
    private void UpdateHaptics(
        bool leftNear,
        bool rightNear)
    {
        hapticTimer -= Time.deltaTime;

        if (hapticTimer > 0f)
            return;

        if (leftNear &&
            leftController != null)
        {
            leftController.SendHapticImpulse(
                hapticStrength,
                hapticPulseDuration
            );
        }

        if (rightNear &&
            rightController != null)
        {
            rightController.SendHapticImpulse(
                hapticStrength,
                hapticPulseDuration
            );
        }

        hapticTimer =
            hapticInterval;
    }

    // ======================================================
    // 检测控制器位置
    // ======================================================
    private bool IsHandNear(
        Transform hand)
    {
        if (hand == null)
            return false;

        float distance =
            Vector3.Distance(
                hand.position,
                transform.position
            );

        return distance <=
               handDetectDistance;
    }

    // ======================================================
    // 中途松手
    // ======================================================
    private void ResetPush()
    {
        handIsOnTarget = false;

        pushTimer = 0f;
        hapticTimer = 0f;

        encourageTriggered = false;
        pushFinished = false;

        // 红色恢复蓝色
        SetHandPrintMaterial(
            blueMaterial
        );

        // Keep pushing 如果已经出现，
        // 松手立即隐藏
        if (speechBubble != null)
        {
            speechBubble.HideBubble();
        }

        Debug.Log(
            "手移开 → 恢复蓝色 → 重新从前2秒开始"
        );
    }

    // ======================================================
    // Driver Talking 结束后调用
    // ======================================================
    public void ShowHandPrint()
    {
        if (handPrint == null)
            return;

        handPrint.SetActive(true);

        SetHandPrintMaterial(
            blueMaterial
        );

        Debug.Log(
            "Talking结束 → 蓝色HandPrint出现"
        );
    }

    // ======================================================
    // 玩家到 DriverPushPoint 后调用
    // ======================================================
    public void EnablePushInteraction()
    {
        interactionEnabled = true;

        ResetInternalState();

        SetHandPrintMaterial(
            blueMaterial
        );

        Debug.Log(
            "到达DriverPushPoint → 开始检测手"
        );
    }

    private void ResetInternalState()
    {
        handIsOnTarget = false;

        encourageTriggered = false;
        pushFinished = false;

        pushTimer = 0f;
        hapticTimer = 0f;
    }

    // ======================================================
    // 蓝 / 红手印材质切换
    // ======================================================
    private void SetHandPrintMaterial(
        Material material)
    {
        if (material == null ||
            handPrintRenderers == null)
        {
            return;
        }

        foreach (
            Renderer renderer
            in handPrintRenderers)
        {
            renderer.material =
                material;
        }
    }
}