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

    [Header("第二阶段：再继续推多久")]
    public float struggleDuration = 3.0f;

    [Header("持续震动设置")]
    [Range(0f, 1f)]
    public float hapticStrength = 0.35f;

    public float hapticPulseDuration = 0.08f;
    public float hapticInterval = 0.06f;

    [Header("汽车")]
    public Transform carTransform;

    [Header("车轮控制")]
    public CarWheelSpin carWheelSpin;

    [Header("汽车移动设置")]
    public float carMoveDistance = 2.5f;
    public float carMoveDuration = 2.0f;

    [Header("汽车移动方向")]
    public Vector3 carMoveDirection = Vector3.forward;

    private bool interactionEnabled = false;
    private bool handIsOnTarget = false;

    private bool encourageTriggered = false;
    private bool pushFinished = false;

    private float pushTimer = 0f;
    private float hapticTimer = 0f;

    private Renderer[] handPrintRenderers;

    private bool carIsMoving = false;
    private float carMoveTimer = 0f;

    private Vector3 carMoveStartPosition;
    private Vector3 carMoveTargetPosition;

    private void Start()
    {
        if (handPrint != null)
        {
            handPrintRenderers =
                handPrint.GetComponentsInChildren<Renderer>(true);

            handPrint.SetActive(false);
        }

        // 游戏开始保持陷泥潭状态
        if (carWheelSpin != null)
        {
            carWheelSpin.SetStuck();
        }

        ResetInternalState();
    }

    private void Update()
    {
        // =====================================
        // 汽车成功后正在向前移动
        // =====================================
        if (carIsMoving)
        {
            UpdateCarMovement();
            return;
        }

        if (!interactionEnabled)
            return;

        if (carHelpManager == null)
            return;

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

        // =========================
        // 手正在推车
        // =========================
        if (anyHandNear)
        {
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

                SetHandPrintMaterial(
                    redMaterial
                );

                Debug.Log(
                    "开始推车，手印变红"
                );
            }

            UpdateHaptics(
                leftNear,
                rightNear
            );

            pushTimer += Time.deltaTime;

            // =====================
            // 推满 2 秒
            // =====================
            if (pushTimer >= encourageDelay &&
                !encourageTriggered)
            {
                encourageTriggered = true;

                DriverEncourage();
            }

            // =====================
            // 再推 3 秒
            // =====================
            float totalRequiredTime =
                encourageDelay +
                struggleDuration;

            if (pushTimer >=
                    totalRequiredTime &&
                !pushFinished)
            {
                pushFinished = true;

                PushSucceeded();
            }
        }
        else
        {
            if (handIsOnTarget &&
                !pushFinished)
            {
                ResetPush();
            }
        }
    }

    private void DriverEncourage()
    {
        Debug.Log(
            "Driver: Keep pushing!"
        );

        if (speechBubble != null)
        {
            speechBubble
                .ShowEncourageMessage();
        }
    }

    private void UpdateHaptics(
        bool leftNear,
        bool rightNear)
    {
        hapticTimer -=
            Time.deltaTime;

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

    private void PushSucceeded()
    {
        Debug.Log(
            "持续推车成功：四个轮子开始转，汽车向前移动"
        );

        interactionEnabled = false;

        // 成功后隐藏手印
        if (handPrint != null)
        {
            handPrint.SetActive(false);
        }

        // =====================================
        // 四个车轮切换到 Driving
        // =====================================
        if (carWheelSpin != null)
        {
            carWheelSpin.SetDriving();
        }

        // =====================================
        // 开始移动汽车
        // =====================================
        if (carTransform != null)
        {
            carMoveStartPosition =
                carTransform.position;

            carMoveTargetPosition =
                carMoveStartPosition +
                carMoveDirection.normalized *
                carMoveDistance;

            carMoveTimer = 0f;
            carIsMoving = true;
        }
        else
        {
            Debug.LogWarning(
                "Car Transform 没有设置！"
            );
        }

        if (carHelpManager != null)
        {
            carHelpManager.SetStage(
                CarHelpManager.CarHelpStage.Finished
            );
        }
    }

    private void UpdateCarMovement()
    {
        if (carTransform == null)
        {
            carIsMoving = false;
            return;
        }

        carMoveTimer += Time.deltaTime;

        float t =
            Mathf.Clamp01(
                carMoveTimer /
                carMoveDuration
            );

        // 平滑启动、平滑停止
        float smoothT =
            t * t * (3f - 2f * t);

        carTransform.position =
            Vector3.Lerp(
                carMoveStartPosition,
                carMoveTargetPosition,
                smoothT
            );

        // =====================================
        // 移动完成
        // =====================================
        if (t >= 1f)
        {
            carTransform.position =
                carMoveTargetPosition;

            carIsMoving = false;

            Debug.Log(
                "汽车移动完成，四个车轮停止"
            );

            // 四个轮子停止
            if (carWheelSpin != null)
            {
                carWheelSpin.StopWheels();
            }

            // Driver 切到 Push Stop
            if (driverAnimator != null)
            {
                driverAnimator.SetTrigger(
                    "StopPush"
                );
            }
        }
    }

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

    private void ResetPush()
    {
        handIsOnTarget = false;

        pushTimer = 0f;
        hapticTimer = 0f;

        encourageTriggered = false;
        pushFinished = false;

        SetHandPrintMaterial(
            blueMaterial
        );

        // 如果 Keep pushing 已经出现，
        // 松手就立即隐藏
        if (speechBubble != null)
        {
            speechBubble.HideBubble();
        }

        // 中途松手时汽车仍然保持 Stuck 状态
        if (carWheelSpin != null)
        {
            carWheelSpin.SetStuck();
        }

        Debug.Log(
            "手移开 → 恢复蓝色 → 重新从2秒开始"
        );
    }

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

        carIsMoving = false;
    }

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