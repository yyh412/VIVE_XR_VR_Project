using System.Collections;
using UnityEngine;

public class CarFarewellAndLeave : MonoBehaviour
{
    [Header("汽车")]
    public Transform carTransform;

    [Header("原来的载人开车系统")]
    public CarDrivePassenger carDrivePassenger;

    [Header("玩家")]
    public Transform headset;

    [Header("下车区域")]
    public BoxCollider passengerBounds;

    [Header("Driver")]
    public Animator driverAnimator;
    public DriverFarewellLookAt farewellLookAt;

    [Header("字幕")]
    public DriverSpeechBubble speechBubble;

    [Header("语音")]
    public AudioSource driverAudioSource;
    public AudioClip goodLuckClip;

    [Header("告别")]
    public float farewellDelay = 0.5f;
    public float lookBeforeSpeakingDelay = 0.35f;
    public float leaveDelay = 1f;

    [Header("汽车离场路线")]
    public Transform[] exitRoutePoints;

    [Header("离场速度")]
    public float leaveSpeed = 2f;
    public float turnSpeed = 3f;
    public float waypointReachDistance = 0.4f;

    [Header("车头方向修正")]
    public float carRotationOffset = 90f;

    [Header("四轮贴地检测")]
    public Transform wheelLB;
    public Transform wheelLF;
    public Transform wheelRB;
    public Transform wheelRF;

    [Header("地面检测")]
    public LayerMask groundLayer;
    public float wheelRayStartHeight = 1f;
    public float wheelRayDistance = 3f;

    [Header("轮胎半径")]
    public float frontWheelRadius = 0.25f;
    public float rearWheelRadius = 0.35f;

    [Header("车辆贴地")]
    public float groundPositionSpeed = 12f;
    public float groundRotationSpeed = 10f;

    [Header("车轮")]
    public CarWheelSpin carWheelSpin;

    [Header("汽车 Animator")]
    public Animator carAnimator;

    [Header("调试")]
    public bool showDebugLog = true;

    private bool farewellStarted = false;
    private bool carLeaving = false;
    private bool leaveFinished = false;

    private int currentExitPoint = 0;


    private void Update()
    {
        if (!farewellStarted)
        {
            CheckPlayerHasLeftCar();
        }

        if (carLeaving && !leaveFinished)
        {
            DriveAway();
        }
    }


    // ======================================================
    // 玩家真正离开脚踏板后触发告别
    // ======================================================

    private void CheckPlayerHasLeftCar()
    {
        if (carDrivePassenger == null)
            return;

        if (!carDrivePassenger.HasArrived())
            return;

        if (headset == null || passengerBounds == null)
            return;


        Bounds bounds =
            passengerBounds.bounds;

        Vector3 headPosition =
            headset.position;


        bool insideX =
            headPosition.x >= bounds.min.x &&
            headPosition.x <= bounds.max.x;

        bool insideZ =
            headPosition.z >= bounds.min.z &&
            headPosition.z <= bounds.max.z;


        bool stillOnBoard =
            insideX && insideZ;


        if (!stillOnBoard)
        {
            farewellStarted = true;

            StartCoroutine(
                FarewellSequence()
            );
        }
    }


    // ======================================================
    // 告别流程
    // ======================================================

    private IEnumerator FarewellSequence()
    {
        // 玩家下车后稍等
        yield return new WaitForSeconds(
            farewellDelay
        );


        // 1. Driver开始扭头看玩家
        if (farewellLookAt != null)
        {
            farewellLookAt
                .StartLookingAtPlayer();
        }


        // 给头一点时间转过去
        yield return new WaitForSeconds(
            lookBeforeSpeakingDelay
        );


        // 2. 显示 Good luck
        if (speechBubble != null)
        {
            speechBubble
                .ShowGoodLuckMessage();
        }


        // 3. 播放 Bye 音频
        if (driverAudioSource != null &&
            goodLuckClip != null)
        {
            driverAudioSource.Stop();

            driverAudioSource.clip =
                goodLuckClip;

            driverAudioSource.Play();


            while (driverAudioSource.isPlaying)
            {
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(
                1.5f
            );
        }


        // 4. 隐藏字幕
        if (speechBubble != null)
        {
            speechBubble.HideBubble();
        }


        // 5. Driver停止看玩家
        if (farewellLookAt != null)
        {
            farewellLookAt
                .StopLookingAtPlayer();
        }


        // 6. 稍等，再开走
        yield return new WaitForSeconds(
            leaveDelay
        );


        currentExitPoint = 0;
        carLeaving = true;


        // 轮子重新开始转
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
                "Good luck结束 → 汽车开始离场"
            );
        }
    }


    // ======================================================
    // 沿 ExitRoute 离开
    // ======================================================

    private void DriveAway()
    {
        if (carTransform == null)
            return;

        if (exitRoutePoints == null ||
            exitRoutePoints.Length == 0)
        {
            return;
        }


        if (currentExitPoint >=
            exitRoutePoints.Length)
        {
            FinishLeaving();
            return;
        }


        Transform target =
            exitRoutePoints[currentExitPoint];


        if (target == null)
        {
            currentExitPoint++;
            return;
        }


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


        if (distance <=
            waypointReachDistance)
        {
            currentExitPoint++;

            if (currentExitPoint >=
                exitRoutePoints.Length)
            {
                FinishLeaving();
            }

            return;
        }


        if (flatDirection.sqrMagnitude <
            0.001f)
        {
            return;
        }


        flatDirection.Normalize();


        // 水平移动
        Vector3 movement =
            flatDirection *
            leaveSpeed *
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


        // Y由四轮贴地处理
        nextPosition.y =
            carTransform.position.y;


        carTransform.position =
            nextPosition;


        // 基础路线转向
        Quaternion routeRotation =
            Quaternion.LookRotation(
                flatDirection,
                Vector3.up
            );


        Quaternion offsetRotation =
            Quaternion.Euler(
                0f,
                carRotationOffset,
                0f
            );


        Quaternion targetRotation =
            routeRotation *
            offsetRotation;


        carTransform.rotation =
            Quaternion.Slerp(
                carTransform.rotation,
                targetRotation,
                turnSpeed *
                Time.deltaTime
            );


        // 四轮继续贴地
        UpdateWheelGroundContact();
    }


    // ======================================================
    // 单个轮子检测地面
    // ======================================================

    private bool TryFindGround(
        Transform wheel,
        float radius,
        out Vector3 desiredWheelCenter)
    {
        desiredWheelCenter =
            Vector3.zero;


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


        desiredWheelCenter =
            hit.point +
            hit.normal.normalized *
            radius;


        return true;
    }


    // ======================================================
    // 四轮贴地
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


        Vector3 targetLB;
        Vector3 targetLF;
        Vector3 targetRB;
        Vector3 targetRF;


        bool hitLB =
            TryFindGround(
                wheelLB,
                rearWheelRadius,
                out targetLB
            );


        bool hitLF =
            TryFindGround(
                wheelLF,
                frontWheelRadius,
                out targetLF
            );


        bool hitRB =
            TryFindGround(
                wheelRB,
                rearWheelRadius,
                out targetRB
            );


        bool hitRF =
            TryFindGround(
                wheelRF,
                frontWheelRadius,
                out targetRF
            );


        if (!hitLB ||
            !hitLF ||
            !hitRB ||
            !hitRF)
        {
            return;
        }


        // 前后轴中心
        Vector3 desiredFront =
            (targetLF + targetRF) * 0.5f;

        Vector3 desiredRear =
            (targetLB + targetRB) * 0.5f;


        // 左右中心
        Vector3 desiredLeft =
            (targetLF + targetLB) * 0.5f;

        Vector3 desiredRight =
            (targetRF + targetRB) * 0.5f;


        Vector3 groundForward =
            desiredFront -
            desiredRear;


        Vector3 groundRight =
            desiredRight -
            desiredLeft;


        if (groundForward.sqrMagnitude <
            0.001f ||
            groundRight.sqrMagnitude <
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
            groundUp = -groundUp;
        }


        // 坡度
        Vector3 slopeForward =
            Vector3.ProjectOnPlane(
                carTransform.forward,
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


            carTransform.rotation =
                Quaternion.Slerp(
                    carTransform.rotation,
                    desiredRotation,
                    Mathf.Clamp01(
                        groundRotationSpeed *
                        Time.deltaTime
                    )
                );
        }


        // 高度
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


        Vector3 targetPosition =
            carTransform.position +
            averageCorrection;


        carTransform.position =
            Vector3.Lerp(
                carTransform.position,
                targetPosition,
                Mathf.Clamp01(
                    groundPositionSpeed *
                    Time.deltaTime
                )
            );
    }


    // ======================================================
    // 离场结束
    // ======================================================

    private void FinishLeaving()
    {
        if (leaveFinished)
            return;


        leaveFinished = true;
        carLeaving = false;


        if (carWheelSpin != null)
        {
            carWheelSpin.StopWheels();
        }


        if (showDebugLog)
        {
            Debug.Log(
                "汽车完成离场路线"
            );
        }
    }
}