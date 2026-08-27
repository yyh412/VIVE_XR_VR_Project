using UnityEngine;
using UnityEngine.AI;

public class CarDrivePassenger : MonoBehaviour
{
    // ======================================================
    // 汽车
    // ======================================================

    [Header("汽车")]
    [Tooltip("拖入汽车最外层根物体")]
    public Transform carTransform;


    // ======================================================
    // 玩家
    // ======================================================

    [Header("玩家 XR Origin")]
    [Tooltip("拖入 XR Origin (XR Rig)")]
    public Transform xrOrigin;


    // ======================================================
    // 上车系统
    // ======================================================

    [Header("上车检测")]
    public VRBoardingDetector boardingDetector;

    [Header("乘客控制")]
    public CarPassengerController passengerController;


    // ======================================================
    // Driver
    // ======================================================

    [Header("Driver")]
    public Animator driverAnimator;

    [Tooltip("Driver 的驾驶动画状态名字")]
    public string drivingStateName = "Driving";


    // ======================================================
    // Waypoint 路线
    // ======================================================

    [Header("汽车路线")]
    [Tooltip("按顺序拖 RoutePoint_01、02、03...")]
    public Transform[] routePoints;


    // ======================================================
    // 行驶参数
    // ======================================================

    [Header("汽车速度")]
    public float driveSpeed = 2.0f;

    [Tooltip("汽车转向速度")]
    public float turnSpeed = 3.0f;

    [Tooltip("距离路线点多近时进入下一个点")]
    public float waypointReachDistance = 0.4f;


    // ======================================================
    // 车头方向修正
    // ======================================================

    [Header("车头方向修正")]
    [Tooltip("如果汽车模型车头不是 Unity +Z，就在这里修正。常用：0 / 90 / -90 / 180")]
    public float carRotationOffset = 0f;


    // ======================================================
    // NavMesh 贴地
    // ======================================================

    [Header("汽车贴地")]

    [Tooltip("从当前位置附近多大范围寻找 NavMesh")]
    public float navMeshSearchDistance = 2.0f;

    [Tooltip("车体相对于 NavMesh 地面的高度微调")]
    public float carHeightOffset = -0.06f;

    [Tooltip("是否使用 NavMesh 自动修正汽车高度")]
    public bool useNavMeshHeight = true;


    // ======================================================
    // 最后减速
    // ======================================================

    [Header("终点减速")]

    [Tooltip("距离最后一个路线点多远开始减速")]
    public float slowDownDistance = 2.0f;

    [Tooltip("减速时最低速度")]
    public float minimumDriveSpeed = 0.3f;


    // ======================================================
    // 车轮
    // ======================================================

    [Header("车轮")]
    public CarWheelSpin carWheelSpin;


    // ======================================================
    // 调试
    // ======================================================

    [Header("调试")]
    public bool showDebugLog = true;


    // ======================================================
    // 内部状态
    // ======================================================

    private bool drivingStarted = false;
    private bool arrived = false;

    private int currentPointIndex = 0;


    // ======================================================
    // Update
    // ======================================================

    private void Update()
    {
        if (arrived)
            return;


        // 还没有正式启动
        if (!drivingStarted)
        {
            TryStartDriving();
            return;
        }


        // 已经启动
        DriveAlongRoute();
    }


    // ======================================================
    // 是否满足开车条件
    // ======================================================

    private void TryStartDriving()
    {
        if (carTransform == null)
            return;

        if (xrOrigin == null)
            return;

        if (boardingDetector == null)
            return;

        if (passengerController == null)
            return;

        if (driverAnimator == null)
            return;

        if (routePoints == null ||
            routePoints.Length == 0)
        {
            return;
        }


        // 玩家必须已经进入 BoardingZone
        if (!boardingDetector.HasBoarded())
            return;


        // 玩家必须已经抬高完成
        // 正式进入 Passenger Mode
        if (!passengerController.IsPassenger())
            return;


        // Driver 必须正在 Driving
        AnimatorStateInfo stateInfo =
            driverAnimator.GetCurrentAnimatorStateInfo(0);


        if (!stateInfo.IsName(drivingStateName))
            return;


        StartDriving();
    }


    // ======================================================
    // 开始驾驶
    // ======================================================

    private void StartDriving()
    {
        drivingStarted = true;
        arrived = false;

        currentPointIndex = 0;


        if (carWheelSpin != null)
        {
            carWheelSpin.SetDriving();
        }


        if (showDebugLog)
        {
            Debug.Log(
                "玩家已上车 + Driver 已进入 Driving → 汽车开始沿 Waypoint 行驶"
            );
        }
    }


    // ======================================================
    // 沿 Waypoint 路线驾驶
    // ======================================================

    private void DriveAlongRoute()
    {
        if (carTransform == null)
            return;


        if (routePoints == null ||
            routePoints.Length == 0)
        {
            return;
        }


        if (currentPointIndex >=
            routePoints.Length)
        {
            ArriveAtDestination();
            return;
        }


        Transform targetPoint =
            routePoints[currentPointIndex];


        // 某个 Element 没拖
        // 自动跳过
        if (targetPoint == null)
        {
            currentPointIndex++;
            return;
        }


        // ==================================================
        // 保存汽车移动前的位置
        // ==================================================

        Vector3 carBefore =
            carTransform.position;


        // ==================================================
        // 计算当前目标方向
        // ==================================================

        Vector3 toTarget =
            targetPoint.position -
            carTransform.position;


        // 行驶方向只看 X/Z
        Vector3 flatDirection =
            new Vector3(
                toTarget.x,
                0f,
                toTarget.z
            );


        float distance =
            flatDirection.magnitude;


        // ==================================================
        // 到达当前 Waypoint
        // ==================================================

        if (distance <=
            waypointReachDistance)
        {
            currentPointIndex++;


            if (showDebugLog)
            {
                Debug.Log(
                    "到达 RoutePoint → 当前路线索引：" +
                    currentPointIndex
                );
            }


            if (currentPointIndex >=
                routePoints.Length)
            {
                ArriveAtDestination();
            }


            return;
        }


        if (flatDirection.sqrMagnitude <
            0.001f)
        {
            return;
        }


        flatDirection.Normalize();


        // ==================================================
        // 计算正确的汽车朝向
        // ==================================================

        Quaternion routeRotation =
            Quaternion.LookRotation(
                flatDirection,
                Vector3.up
            );


        // 模型自身方向修正
        Quaternion offsetRotation =
            Quaternion.Euler(
                0f,
                carRotationOffset,
                0f
            );


        Quaternion targetRotation =
            routeRotation *
            offsetRotation;


        // ==================================================
        // 平滑转向
        // ==================================================

        carTransform.rotation =
            Quaternion.Slerp(
                carTransform.rotation,
                targetRotation,
                turnSpeed *
                Time.deltaTime
            );


        // ==================================================
        // 当前速度
        // ==================================================

        float currentSpeed =
            driveSpeed;


        bool isLastPoint =
            currentPointIndex ==
            routePoints.Length - 1;


        // 最后一段减速
        if (isLastPoint &&
            distance <
            slowDownDistance)
        {
            float speedT =
                Mathf.Clamp01(
                    distance /
                    slowDownDistance
                );


            currentSpeed =
                Mathf.Lerp(
                    minimumDriveSpeed,
                    driveSpeed,
                    speedT
                );
        }


        // ==================================================
        // 非常重要：
        //
        // 移动方向直接使用路线方向，
        // 不使用 carTransform.forward。
        //
        // 因为你的汽车模型 forward 轴目前并不确定。
        //
        // Rotation Offset 只负责视觉车头方向。
        // ==================================================

        Vector3 movement =
            flatDirection *
            currentSpeed *
            Time.deltaTime;


        // 防止超过当前点
        if (movement.magnitude >
            distance)
        {
            movement =
                flatDirection *
                distance;
        }


        Vector3 desiredPosition =
            carTransform.position +
            movement;


        // ==================================================
        // NavMesh 自动贴地
        // ==================================================

        if (useNavMeshHeight)
        {
            NavMeshHit hit;


            if (NavMesh.SamplePosition(
                desiredPosition,
                out hit,
                navMeshSearchDistance,
                NavMesh.AllAreas))
            {
                desiredPosition.y =
                    hit.position.y +
                    carHeightOffset;
            }
            else
            {
                // 找不到 NavMesh 时，
                // 绝对不要突然改变高度
                desiredPosition.y =
                    carTransform.position.y;


                if (showDebugLog)
                {
                    Debug.LogWarning(
                        "汽车当前位置附近没有找到 NavMesh → 暂时保持原高度"
                    );
                }
            }
        }
        else
        {
            // 不使用 NavMesh 时
            // 永远保持当前 Y
            desiredPosition.y =
                carTransform.position.y;
        }


        // ==================================================
        // 真正移动汽车
        // ==================================================

        carTransform.position =
            desiredPosition;


        // ==================================================
        // 计算汽车这一帧真正移动量
        // ==================================================

        Vector3 carDelta =
            carTransform.position -
            carBefore;


        // ==================================================
        // 玩家同步汽车位移
        //
        // 汽车走多少，
        // XR Origin 同样走多少。
        //
        // 因此玩家不会被汽车留在原地。
        // ==================================================

        if (xrOrigin != null)
        {
            xrOrigin.position +=
                carDelta;
        }
    }


    // ======================================================
    // 到达终点
    // ======================================================

    private void ArriveAtDestination()
    {
        if (arrived)
            return;


        arrived = true;
        drivingStarted = false;


        if (carWheelSpin != null)
        {
            carWheelSpin.StopWheels();
        }


        if (showDebugLog)
        {
            Debug.Log(
                "汽车到达最后一个 RoutePoint → 停车"
            );
        }


        // 暂时保持 Passenger Mode
        //
        // 后面我们再接：
        // Driver说再见
        // → 玩家下车
        // → Passenger Mode关闭
    }


    // ======================================================
    // 查询状态
    // ======================================================

    public bool IsDriving()
    {
        return drivingStarted;
    }


    public bool HasArrived()
    {
        return arrived;
    }
}