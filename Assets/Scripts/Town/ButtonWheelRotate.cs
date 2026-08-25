using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ButtonWheelRotate : MonoBehaviour
{
    [Header("真正旋转的中心")]
    public Transform buttonPivot;

    [Header("抓取组件")]
    public XRGrabInteractable grabInteractable;

    [Header("ButtonPivot 的本地旋转轴")]
    public Vector3 localRotationAxis = Vector3.forward;

    [Header("转满多少度完成")]
    public float requiredRotation = 360f;

    [Header("旋转灵敏度")]
    public float rotationSensitivity = 1f;

    [Header("输入方向反转")]
    [Tooltip("如果你往想要的方向拧手腕时按钮不转，就切换这个")]
    public bool reverseInputDirection = false;

    [Header("按钮视觉方向反转")]
    [Tooltip("如果按钮转动方向和手腕方向相反，就切换这个")]
    public bool reverseButtonDirection = false;

    [Header("最小有效旋转")]
    public float minimumAngle = 0.05f;

    [Header("完成后的效果")]
    public BottomButtonGlowPulse glowPulse;
    public DoorLiftController doorController;

    private Transform controllerTransform;
    private Quaternion lastControllerRotation;

    private bool isGrabbed = false;
    private bool completed = false;

    private float totalRotation = 0f;

    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (completed)
            return;

        controllerTransform = args.interactorObject.transform;

        if (controllerTransform == null)
            return;

        lastControllerRotation = controllerTransform.rotation;

        isGrabbed = true;

        Debug.Log("抓住旋钮");
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        controllerTransform = null;

        Debug.Log("松开旋钮");
    }

    private void Update()
    {
        if (!isGrabbed ||
            completed ||
            controllerTransform == null ||
            buttonPivot == null)
        {
            return;
        }

        Quaternion currentRotation = controllerTransform.rotation;

        // 当前这一帧相对于上一帧，手柄旋转了多少
        Quaternion deltaRotation =
            currentRotation * Quaternion.Inverse(lastControllerRotation);

        deltaRotation.ToAngleAxis(
            out float angle,
            out Vector3 axis
        );

        // 转换到 -180 ~ 180
        if (angle > 180f)
        {
            angle -= 360f;
        }

        // ButtonPivot 实际的世界旋转轴
        Vector3 worldRotationAxis =
            buttonPivot.TransformDirection(localRotationAxis).normalized;

        // 提取手柄沿旋钮轴方向的旋转量
        float inputAngle =
            angle * Vector3.Dot(
                axis.normalized,
                worldRotationAxis
            );

        inputAngle *= rotationSensitivity;

        // 输入方向反转
        if (reverseInputDirection)
        {
            inputAngle = -inputAngle;
        }

        /*
         * 只允许一个方向旋转。
         *
         * 正方向：
         * 按钮旋转并累计进度。
         *
         * 反方向：
         * 手腕可以回位，
         * 但按钮不倒退，也不减少进度。
         */
        if (inputAngle > minimumAngle)
        {
            float remainingRotation =
                requiredRotation - totalRotation;

            float acceptedAngle =
                Mathf.Min(
                    inputAngle,
                    remainingRotation
                );

            float buttonAngle = acceptedAngle;

            // 如果按钮视觉方向需要反转
            if (reverseButtonDirection)
            {
                buttonAngle = -buttonAngle;
            }

            // 旋转真正的 ButtonPivot
            buttonPivot.Rotate(
                localRotationAxis,
                buttonAngle,
                Space.Self
            );

            // 累计旋转进度
            totalRotation += acceptedAngle;

            Debug.Log(
                "旋转进度：" +
                totalRotation.ToString("F1") +
                " / " +
                requiredRotation
            );

            if (totalRotation >= requiredRotation - 0.01f)
            {
                CompleteRotation();
            }
        }

        // 无论正转还是手腕回位，都更新当前手柄角度
        lastControllerRotation = currentRotation;
    }

    private void CompleteRotation()
    {
        completed = true;
        isGrabbed = false;
        controllerTransform = null;

        totalRotation = requiredRotation;

        Debug.Log("旋钮已经单方向转满360度！");

        // 永久关闭按钮呼吸灯
        if (glowPulse != null)
        {
            glowPulse.StopGlowPermanently();
        }

        // 打开门：向上升起
        if (doorController != null)
        {
            doorController.OpenDoor();
        }
    }
}