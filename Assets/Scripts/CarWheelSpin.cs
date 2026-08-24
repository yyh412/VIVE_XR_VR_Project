using UnityEngine;

public class CarWheelSpin : MonoBehaviour
{
    [Header("四个车轮骨骼")]
    public Transform wheelLB;
    public Transform wheelLF;
    public Transform wheelRB;
    public Transform wheelRF;

    [Header("车身骨骼")]
    public Transform bodyBone;

    [Header("陷泥潭空转设置")]
    public float stuckSpinSpeed = 450f;
    public float spinDuration = 1.0f;
    public float stopDuration = 0.5f;

    [Header("车身挣扎抖动")]
    public float bodyShakeAngle = 1.5f;
    public float bodyShakeSpeed = 18f;

    [Header("正常行驶")]
    public float drivingSpinSpeed = 300f;

    public enum WheelState
    {
        Stuck,
        Driving,
        Stop
    }

    [Header("当前状态")]
    public WheelState currentState = WheelState.Stuck;

    private float timer = 0f;
    private bool stuckWheelSpinning = true;

    // 记录车身原始旋转
    private Quaternion bodyOriginalRotation;

    void Start()
    {
        if (bodyBone != null)
        {
            bodyOriginalRotation = bodyBone.localRotation;
        }
    }

    void LateUpdate()
    {
        switch (currentState)
        {
            case WheelState.Stuck:
                UpdateStuckState();
                break;

            case WheelState.Driving:
                RotateAllWheels();
                ResetBody();
                break;

            case WheelState.Stop:
                ResetBody();
                break;
        }
    }

    void UpdateStuckState()
    {
        timer += Time.deltaTime;

        if (stuckWheelSpinning)
        {
            // 左后轮陷在泥潭里
            RotateWheel(wheelLB, stuckSpinSpeed);

            // 同时让车身轻微挣扎
            ShakeBody();

            if (timer >= spinDuration)
            {
                timer = 0f;
                stuckWheelSpinning = false;
            }
        }
        else
        {
            // 轮子停止时，车身也恢复
            ResetBody();

            if (timer >= stopDuration)
            {
                timer = 0f;
                stuckWheelSpinning = true;
            }
        }
    }

    void ShakeBody()
    {
        if (bodyBone == null)
            return;

        float shake =
            Mathf.Sin(Time.time * bodyShakeSpeed) * bodyShakeAngle;

        // 在原始旋转基础上轻微左右倾斜
        bodyBone.localRotation =
            bodyOriginalRotation *
            Quaternion.Euler(shake, 0f, 0f);
    }

    void ResetBody()
    {
        if (bodyBone == null)
            return;

        // 平滑回到原来的姿态
        bodyBone.localRotation =
            Quaternion.Slerp(
                bodyBone.localRotation,
                bodyOriginalRotation,
                Time.deltaTime * 10f
            );
    }

    void RotateAllWheels()
    {
        RotateWheel(wheelLB, drivingSpinSpeed);
        RotateWheel(wheelLF, drivingSpinSpeed);
        RotateWheel(wheelRB, drivingSpinSpeed);
        RotateWheel(wheelRF, drivingSpinSpeed);
    }

    void RotateWheel(Transform wheel, float speed)
    {
        if (wheel == null)
            return;

        // 你已经确认轮子正确轴是本地 Y
        wheel.Rotate(
            0f,
            speed * Time.deltaTime,
            0f,
            Space.Self
        );
    }

    public void SetStuck()
    {
        currentState = WheelState.Stuck;
        timer = 0f;
        stuckWheelSpinning = true;
    }

    public void SetDriving()
    {
        currentState = WheelState.Driving;
        timer = 0f;
    }

    public void StopWheels()
    {
        currentState = WheelState.Stop;
        timer = 0f;
        stuckWheelSpinning = false;
    }
}