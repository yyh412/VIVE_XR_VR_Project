using UnityEngine;

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
    // 导航系统
    // ======================================================

    [Header("路线导航")]
    [Tooltip("拖入场景中的 VRNavigationPath")]
    public VRNavigationPath navigationPath;


    // ======================================================
    // Driver
    // ======================================================

    [Header("Driver")]
    public Animator driverAnimator;

    [Tooltip("Driver 驾驶动画状态名")]
    public string drivingStateName = "Driving";


    // ======================================================
    // 汽车自己的 Animator
    // ======================================================

    [Header("汽车 Animator")]
    [Tooltip("汽车自己的动画一直播放")]
    public Animator carAnimator;


    // ======================================================
    // Waypoint
    // ======================================================

    [Header("汽车路线")]
    [Tooltip("RoutePoint_01、02、03...依次拖进来")]
    public Transform[] routePoints;


    // ======================================================
    // 行驶参数
    // ======================================================

    [Header("汽车行驶")]

    public float driveSpeed = 2f;

    [Tooltip("水平转向速度")]
    public float turnSpeed = 3f;

    [Tooltip("距离路线点多近进入下一个点")]
    public float waypointReachDistance = 0.35f;


    // ======================================================
    // 模型朝向修正
    // ======================================================

    [Header("车头方向修正")]

    [Tooltip("你现在已经调正确的是90，就保持90")]
    public float carRotationOffset = 90f;


    // ======================================================
    // 四个车轮
    // ======================================================

    [Header("四轮贴地检测")]

    [Tooltip("左后轮")]
    public Transform wheelLB;

    [Tooltip("左前轮")]
    public Transform wheelLF;

    [Tooltip("右后轮")]
    public Transform wheelRB;

    [Tooltip("右前轮")]
    public Transform wheelRF;


    // ======================================================
    // 地面
    // ======================================================

    [Header("地面检测")]

    [Tooltip("只选择 Ground")]
    public LayerMask groundLayer;

    [Tooltip("从轮子上方多高开始射线")]
    public float wheelRayStartHeight = 1f;

    [Tooltip("向下检测距离")]
    public float wheelRayDistance = 3f;


    // ======================================================
    // 前后轮半径
    // ======================================================

    [Header("轮胎半径")]

    [Tooltip("前轮半径")]
    public float frontWheelRadius = 0.25f;

    [Tooltip("后轮半径")]
    public float rearWheelRadius = 0.35f;


    // ======================================================
    // 贴地速度
    // ======================================================

    [Header("车辆贴地")]

    [Tooltip("汽车高度跟随地面的速度")]
    public float groundPositionSpeed = 12f;

    [Tooltip("汽车坡度跟随速度")]
    public float groundRotationSpeed = 10f;


    // ======================================================
    // 终点
    // ======================================================

    [Header("终点减速")]

    public float slowDownDistance = 2f;

    public float minimumDriveSpeed = 0.3f;


    // ======================================================
    // 车轮转动
    // ======================================================

    [Header("车轮转动")]
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


        if (!drivingStarted)
        {
            TryStartDriving();
            return;
        }


        DriveAlongRoute();
    }


    // ======================================================
    // 是否可以开车
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


        // 玩家必须上车
        if (!boardingDetector.HasBoarded())
            return;


        // 必须已经进入 PassengerMode
        if (!passengerController.IsPassenger())
            return;


        // Driver 必须正在 Driving
        AnimatorStateInfo state =
            driverAnimator.GetCurrentAnimatorStateInfo(0);


        if (!state.IsName(drivingStateName))
            return;


        StartDriving();
    }


    // ======================================================
    // 开始开车
    // ======================================================

    private void StartDriving()
    {
        drivingStarted = true;
        arrived = false;

        currentPointIndex = 0;


        // 四轮开始转
        if (carWheelSpin != null)
        {
            carWheelSpin.SetDriving();
        }


        // 汽车自己的动画继续
        if (carAnimator != null)
        {
            carAnimator.enabled = true;
        }


        if (showDebugLog)
        {
            Debug.Log(
                "汽车启动 → 四轮Driving → Driver动画继续 → 车辆动画继续"
            );
        }
    }


    // ======================================================
    // 沿路线驾驶
    // ======================================================

    private void DriveAlongRoute()
    {
        if (routePoints == null ||
            routePoints.Length == 0)
        {
            return;
        }


        if (currentPointIndex >= routePoints.Length)
        {
            ArriveAtDestination();
            return;
        }


        Transform target =
            routePoints[currentPointIndex];


        if (target == null)
        {
            currentPointIndex++;
            return;
        }


        // ==================================================
        // 记录汽车这一帧开始的位置
        // ==================================================

        Vector3 carBefore =
            carTransform.position;


        // ==================================================
        // Waypoint方向
        // ==================================================

        Vector3 toTarget =
            target.position -
            carTransform.position;


        Vector3 flatDirection =
            new Vector3(
                toTarget.x,
                0f,
                toTarget.z
            );


        float distance =
            flatDirection.magnitude;


        // ==================================================
        // 到达当前点
        // ==================================================

        if (distance <= waypointReachDistance)
        {
            currentPointIndex++;


            if (showDebugLog)
            {
                Debug.Log(
                    "到达RoutePoint → " +
                    currentPointIndex
                );
            }


            if (currentPointIndex >= routePoints.Length)
            {
                ArriveAtDestination();
            }


            return;
        }


        if (flatDirection.sqrMagnitude < 0.001f)
            return;


        flatDirection.Normalize();


        // ==================================================
        // 速度
        // ==================================================

        float currentSpeed =
            driveSpeed;


        bool lastPoint =
            currentPointIndex ==
            routePoints.Length - 1;


        if (lastPoint &&
            distance < slowDownDistance)
        {
            float t =
                Mathf.Clamp01(
                    distance /
                    slowDownDistance
                );


            currentSpeed =
                Mathf.Lerp(
                    minimumDriveSpeed,
                    driveSpeed,
                    t
                );
        }


        // ==================================================
        // 先只进行水平路线移动
        // ==================================================

        Vector3 movement =
            flatDirection *
            currentSpeed *
            Time.deltaTime;


        if (movement.magnitude > distance)
        {
            movement =
                flatDirection *
                distance;
        }


        Vector3 nextPosition =
            carTransform.position +
            movement;


        nextPosition.y =
            carTransform.position.y;


        carTransform.position =
            nextPosition;


        // ==================================================
        // Waypoint负责水平车头方向
        // ==================================================

        Quaternion routeRotation =
            Quaternion.LookRotation(
                flatDirection,
                Vector3.up
            );


        Quaternion modelOffset =
            Quaternion.Euler(
                0f,
                carRotationOffset,
                0f
            );


        Quaternion horizontalRotation =
            routeRotation *
            modelOffset;


        carTransform.rotation =
            Quaternion.Slerp(
                carTransform.rotation,
                horizontalRotation,
                turnSpeed *
                Time.deltaTime
            );


        // ==================================================
        // 四个轮子独立检测地面
        // ==================================================

        UpdateWheelGroundContact();


        // ==================================================
        // 所有修正做完以后
        // 再计算汽车真正移动量
        // ==================================================

        Vector3 carDelta =
            carTransform.position -
            carBefore;


        // 玩家跟车
        if (xrOrigin != null)
        {
            xrOrigin.position +=
                carDelta;
        }
    }


    // ======================================================
    // 单轮检测
    // ======================================================

    private bool TryFindGround(
        Transform wheel,
        float radius,
        out Vector3 contactPoint,
        out Vector3 desiredWheelCenter)
    {
        contactPoint = Vector3.zero;
        desiredWheelCenter = Vector3.zero;


        if (wheel == null)
            return false;


        Vector3 rayStart =
            wheel.position +
            Vector3.up *
            wheelRayStartHeight;


        RaycastHit hit;


        if (!Physics.Raycast(
            rayStart,
            Vector3.down,
            out hit,
            wheelRayStartHeight +
            wheelRayDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore))
        {
            return false;
        }


        contactPoint =
            hit.point;


        desiredWheelCenter =
            hit.point +
            hit.normal.normalized *
            radius;


        return true;
    }


    // ======================================================
    // 前轴 / 后轴独立贴地
    // ======================================================

    private void UpdateWheelGroundContact()
    {
        if (wheelLB == null ||
            wheelLF == null ||
            wheelRB == null ||
            wheelRF == null)
        {
            return;
        }


        Vector3 contactLB;
        Vector3 contactLF;
        Vector3 contactRB;
        Vector3 contactRF;


        Vector3 targetLB;
        Vector3 targetLF;
        Vector3 targetRB;
        Vector3 targetRF;


        bool hitLB =
            TryFindGround(
                wheelLB,
                rearWheelRadius,
                out contactLB,
                out targetLB
            );


        bool hitLF =
            TryFindGround(
                wheelLF,
                frontWheelRadius,
                out contactLF,
                out targetLF
            );


        bool hitRB =
            TryFindGround(
                wheelRB,
                rearWheelRadius,
                out contactRB,
                out targetRB
            );


        bool hitRF =
            TryFindGround(
                wheelRF,
                frontWheelRadius,
                out contactRF,
                out targetRF
            );


        if (!hitLB ||
            !hitLF ||
            !hitRB ||
            !hitRF)
        {
            if (showDebugLog)
            {
                Debug.LogWarning(
                    "四轮贴地：至少一个轮子没有检测到Ground"
                );
            }

            return;
        }


        Vector3 currentFrontCenter =
            (
                wheelLF.position +
                wheelRF.position
            ) * 0.5f;


        Vector3 currentRearCenter =
            (
                wheelLB.position +
                wheelRB.position
            ) * 0.5f;


        Vector3 desiredFrontCenter =
            (
                targetLF +
                targetRF
            ) * 0.5f;


        Vector3 desiredRearCenter =
            (
                targetLB +
                targetRB
            ) * 0.5f;


        Vector3 desiredLeftCenter =
            (
                targetLF +
                targetLB
            ) * 0.5f;


        Vector3 desiredRightCenter =
            (
                targetRF +
                targetRB
            ) * 0.5f;


        Vector3 groundForward =
            desiredFrontCenter -
            desiredRearCenter;


        Vector3 groundRight =
            desiredRightCenter -
            desiredLeftCenter;


        if (groundForward.sqrMagnitude <
            0.001f)
        {
            return;
        }


        if (groundRight.sqrMagnitude <
            0.001f)
        {
            return;
        }


        groundForward.Normalize();
        groundRight.Normalize();


        Vector3 groundUp =
            Vector3.Cross(
                groundForward,
                groundRight
            ).normalized;


        if (groundUp.y < 0f)
        {
            groundUp =
                -groundUp;
        }


        Vector3 currentCarForward =
            carTransform.forward;


        Vector3 slopeForward =
            Vector3.ProjectOnPlane(
                currentCarForward,
                groundUp
            );


        if (slopeForward.sqrMagnitude >
            0.001f)
        {
            slopeForward.Normalize();


            Quaternion desiredRotation =
                Quaternion.LookRotation(
                    slopeForward,
                    groundUp
                );


            float rotT =
                Mathf.Clamp01(
                    groundRotationSpeed *
                    Time.deltaTime
                );


            carTransform.rotation =
                Quaternion.Slerp(
                    carTransform.rotation,
                    desiredRotation,
                    rotT
                );
        }


        Vector3 correctionLB =
            targetLB -
            wheelLB.position;


        Vector3 correctionLF =
            targetLF -
            wheelLF.position;


        Vector3 correctionRB =
            targetRB -
            wheelRB.position;


        Vector3 correctionRF =
            targetRF -
            wheelRF.position;


        Vector3 averageCorrection =
            (
                correctionLB +
                correctionLF +
                correctionRB +
                correctionRF
            ) / 4f;


        averageCorrection.x = 0f;
        averageCorrection.z = 0f;


        Vector3 desiredPosition =
            carTransform.position +
            averageCorrection;


        float posT =
            Mathf.Clamp01(
                groundPositionSpeed *
                Time.deltaTime
            );


        carTransform.position =
            Vector3.Lerp(
                carTransform.position,
                desiredPosition,
                posT
            );
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


        // ==================================================
        // 车轮停止
        // ==================================================

        if (carWheelSpin != null)
        {
            carWheelSpin.StopWheels();
        }


        // ==================================================
        // 汽车自己的动画继续
        // ==================================================

        if (carAnimator != null)
        {
            carAnimator.enabled = true;
        }


        // ==================================================
        // Driver Driving 继续
        // 不操作 driverAnimator
        // ==================================================


        // ==================================================
        // 玩家解除脚踏板范围限制
        // ==================================================

        if (passengerController != null)
        {
            passengerController.ExitPassengerMode();
        }


        // ==================================================
        // ★ 新增：
        // 玩家搭车路线结束
        // 恢复地面导航箭头
        // ==================================================

        if (navigationPath != null)
        {
            navigationPath.ShowNavigation();

            if (showDebugLog)
            {
                Debug.Log(
                    "玩家到达下车点 → 恢复黄色导航箭头"
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "CarDrivePassenger：Navigation Path 没有设置！"
            );
        }


        if (showDebugLog)
        {
            Debug.Log(
                "终点停车 → 四轮停止 → PassengerBounds解除 → 黄色导航恢复"
            );
        }
    }


    // ======================================================
    // 查询
    // ======================================================

    public bool IsDriving()
    {
        return drivingStarted;
    }


    public bool HasArrived()
    {
        return arrived;
    }


    // ======================================================
    // Scene中显示四条检测线
    // ======================================================

    private void OnDrawGizmosSelected()
    {
        DrawRay(wheelLB);
        DrawRay(wheelLF);
        DrawRay(wheelRB);
        DrawRay(wheelRF);
    }


    private void DrawRay(Transform wheel)
    {
        if (wheel == null)
            return;


        Vector3 start =
            wheel.position +
            Vector3.up *
            wheelRayStartHeight;


        Gizmos.DrawLine(
            start,
            start +
            Vector3.down *
            (
                wheelRayStartHeight +
                wheelRayDistance
            )
        );
    }
}