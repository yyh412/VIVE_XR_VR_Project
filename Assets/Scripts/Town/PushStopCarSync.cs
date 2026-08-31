using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PushStopCarSync : MonoBehaviour
{
    // ======================================================
    // Driver
    // ======================================================

    [Header("Driver")]
    public Animator driverAnimator;
    public NavMeshAgent driverAgent;

    [Header("Driver 推车手 IK")]
    public DriverPushHandIK driverPushHandIK;


    // ======================================================
    // 玩家
    // ======================================================

    [Header("玩家")]
    public Transform playerTransform;


    // ======================================================
    // 汽车
    // ======================================================

    [Header("汽车根物体")]
    public Transform carTransform;

    [Header("车轮控制")]
    public CarWheelSpin carWheelSpin;

    [Header("帮助完成后环境恢复彩色")]
    [Tooltip("拖入 DriverColorZone")]
    public EnvironmentColorZone environmentColorZone;


    // ======================================================
    // 字幕 / 语音
    // ======================================================

    [Header("字幕气泡")]
    public DriverSpeechBubble speechBubble;

    [Header("感谢语音")]
    public AudioSource driverAudioSource;
    public AudioClip thankVoiceClip;


    // ======================================================
    // Driver 去驾驶室路线
    // ======================================================

    [Header("Driver 绕车路线")]
    public Transform driverWalkPoint1;
    public Transform driverWalkPoint2;
    public Transform driverDoorPoint;

    [Header("到达路线点判定距离")]
    public float waypointReachDistance = 0.25f;


    // ======================================================
    // 玩家动态避让
    // ======================================================

    [Header("玩家动态避让")]
    public float playerAvoidDistance = 1.0f;

    [Header("侧向绕开距离")]
    public float sideAvoidDistance = 1.0f;

    [Header("向前绕开距离")]
    public float forwardAvoidDistance = 0.5f;

    [Header("多久检查一次玩家")]
    public float avoidCheckInterval = 0.2f;

    [Header("寻找临时避让点的 NavMesh 范围")]
    public float avoidNavMeshSearchDistance = 1.0f;


    // ======================================================
    // 上台阶
    // ======================================================

    [Header("上台阶状态名字")]
    public string ascendingStairsStateName = "Ascending Stairs";

    [Header("上台阶 Trigger")]
    public string stepUpTrigger = "StepUp";

    [Header("上台阶前原地转向角度")]
    public float stepTurnAngle = 90f;

    [Header("上台阶前转向时间")]
    public float stepTurnDuration = 0.5f;


    // ======================================================
    // 进入车
    // ======================================================

    [Header("进入车状态名字")]
    public string enteringCarStateName = "Entering Car";

    [Header("进入车 Trigger")]
    public string enterCarTrigger = "EnterCar";

    [Header("上台阶播放到多少时进入车")]
    [Range(0.5f, 1f)]
    public float enterCarAtNormalizedTime = 0.98f;


    // ======================================================
    // Driving
    // ======================================================

    [Header("驾驶位最终位置")]
    public Transform driverSeatPoint;

    [Header("Driving 状态名字")]
    public string drivingStateName = "Driving";

    [Header("进入 Driving Trigger")]
    public string driveTrigger = "Drive";

    [Header("Entering Car 播放到多少时切 Driving")]
    [Range(0.5f, 1f)]
    public float driveAtNormalizedTime = 0.95f;


    // ======================================================
    // Entering Car 座位位置修正
    // ======================================================

    [Header("开始修正坐姿位置的动画进度")]
    [Range(0f, 0.8f)]
    public float seatBlendStart = 0.20f;

    [Header("完成座位位置修正的动画进度")]
    [Range(0.2f, 1f)]
    public float seatBlendEnd = 0.85f;


    // ======================================================
    // Animator
    // ======================================================

    [Header("Push Stop 状态名字")]
    public string pushStopStateName = "Push Stop";

    [Header("停车后站立 Trigger")]
    public string pauseTrigger = "Pause";

    [Header("感谢 Talking Trigger")]
    public string thankTrigger = "Thank";

    [Header("Talking → Walking Trigger")]
    public string goDriveTrigger = "GoDrive";


    // ======================================================
    // 时间
    // ======================================================

    [Header("Push Stop 后停顿时间")]
    public float pauseBeforeTalking = 2.0f;

    [Header("语音结束后继续 Talking 的时间")]
    public float pauseAfterVoice = 1.5f;

    [Header("转向玩家时间")]
    public float turnDuration = 0.6f;


    // ======================================================
    // NavMesh / 高度
    // ======================================================

    [Header("NavMesh 搜索距离")]
    public float navMeshSearchDistance = 1.0f;

    [Header("Driver脚底高度微调")]
    public float driverHeightOffset = 0.2f;

    [Header("汽车高度微调")]
    public float carHeightOffset = -0.06f;


    // ======================================================
    // 内部状态
    // ======================================================

    private bool movementStarted = false;
    private bool pushStopFinished = false;
    private bool thankSequenceStarted = false;

    private bool enterCarTriggered = false;
    private bool driveTriggered = false;

    private bool enteringCarPositionInitialized = false;

    private Vector3 enteringCarStartPosition;
    private Quaternion enteringCarStartRotation;


    // ======================================================
    // Animator Root Motion
    // ======================================================

    private void OnAnimatorMove()
    {
        if (driverAnimator == null)
            return;


        AnimatorStateInfo stateInfo =
            driverAnimator.GetCurrentAnimatorStateInfo(0);


        bool isPushStop =
            stateInfo.IsName(
                pushStopStateName
            );


        bool isAscendingStairs =
            stateInfo.IsName(
                ascendingStairsStateName
            );


        bool isEnteringCar =
            stateInfo.IsName(
                enteringCarStateName
            );


        bool isDriving =
            stateInfo.IsName(
                drivingStateName
            );


        // ==================================================
        // Ascending Stairs
        //
        // 使用动画自己的 Root Motion
        // ==================================================

        if (isAscendingStairs)
        {
            Vector3 rootDelta =
                driverAnimator.deltaPosition;


            Quaternion deltaRotation =
                driverAnimator.deltaRotation;


            transform.position +=
                rootDelta;


            transform.rotation =
                deltaRotation *
                transform.rotation;


            // =========================================
            // 上台阶结束 → Entering Car
            // =========================================

            if (stateInfo.normalizedTime >=
                    enterCarAtNormalizedTime &&
                !enterCarTriggered)
            {
                enterCarTriggered = true;

                driveTriggered = false;

                enteringCarPositionInitialized =
                    false;


                driverAnimator.SetTrigger(
                    enterCarTrigger
                );


                Debug.Log(
                    "Ascending Stairs结束 → Entering Car"
                );
            }


            return;
        }


        // ==================================================
        // Entering Car
        //
        // 前段保持起点
        // 中段逐渐向 SeatPoint 修正
        // 后段固定座位
        // ==================================================

        if (isEnteringCar)
        {
            // =========================================
            // 第一次进入 Entering Car
            // 记录当前起始位置
            // =========================================

            if (!enteringCarPositionInitialized)
            {
                enteringCarPositionInitialized =
                    true;


                enteringCarStartPosition =
                    transform.position;


                enteringCarStartRotation =
                    transform.rotation;


                Debug.Log(
                    "Entering Car开始 → 记录进入车起点"
                );
            }


            float animationProgress =
                Mathf.Clamp01(
                    stateInfo.normalizedTime
                );


            if (driverSeatPoint != null)
            {
                float seatBlend = 0f;


                // =====================================
                // 前段：暂时不修正位置
                // =====================================

                if (animationProgress <=
                    seatBlendStart)
                {
                    seatBlend = 0f;
                }


                // =====================================
                // 中段：逐渐移动到座位
                // =====================================

                else if (animationProgress <
                         seatBlendEnd)
                {
                    seatBlend =
                        Mathf.InverseLerp(
                            seatBlendStart,
                            seatBlendEnd,
                            animationProgress
                        );


                    seatBlend =
                        Mathf.SmoothStep(
                            0f,
                            1f,
                            seatBlend
                        );
                }


                // =====================================
                // 后段：已经完全到座位
                // =====================================

                else
                {
                    seatBlend = 1f;
                }


                transform.position =
                    Vector3.Lerp(
                        enteringCarStartPosition,
                        driverSeatPoint.position,
                        seatBlend
                    );


                transform.rotation =
                    Quaternion.Slerp(
                        enteringCarStartRotation,
                        driverSeatPoint.rotation,
                        seatBlend
                    );
            }


            // =========================================
            // Entering Car → Driving
            // =========================================

            if (stateInfo.normalizedTime >=
                    driveAtNormalizedTime &&
                !driveTriggered)
            {
                driveTriggered = true;


                if (driverSeatPoint != null)
                {
                    transform.position =
                        driverSeatPoint.position;


                    transform.rotation =
                        driverSeatPoint.rotation;
                }


                driverAnimator.SetTrigger(
                    driveTrigger
                );


                Debug.Log(
                    "Entering Car结束 → Driving"
                );
            }


            return;
        }


        // ==================================================
        // Driving
        //
        // 根节点固定在驾驶位
        // 手脚由 DriverDrivingIK 修正
        // ==================================================

        if (isDriving)
        {
            if (driverSeatPoint != null)
            {
                transform.position =
                    driverSeatPoint.position;


                transform.rotation =
                    driverSeatPoint.rotation;
            }


            return;
        }


        // ==================================================
        // 不是 Push Stop
        // ==================================================

        if (!isPushStop)
            return;


        // ==================================================
        // Push Stop
        // ==================================================

        if (!movementStarted)
        {
            StartPushStopMovement();
        }


        if (!pushStopFinished)
        {
            Vector3 rootDelta =
                driverAnimator.deltaPosition;


            Quaternion deltaRotation =
                driverAnimator.deltaRotation;


            Vector3 driverBefore =
                transform.position;


            Vector3 driverDesired =
                transform.position +
                rootDelta;


            // =========================================
            // Driver 跟随坡面
            // =========================================

            NavMeshHit driverHit;


            if (NavMesh.SamplePosition(
                driverDesired,
                out driverHit,
                navMeshSearchDistance,
                NavMesh.AllAreas))
            {
                driverDesired.y =
                    driverHit.position.y +
                    driverHeightOffset;
            }


            transform.position =
                driverDesired;


            transform.rotation =
                deltaRotation *
                transform.rotation;


            // =========================================
            // Driver 实际移动量
            // =========================================

            Vector3 driverMoveDelta =
                transform.position -
                driverBefore;


            // =========================================
            // 汽车同步 Driver X/Z
            // Y 独立贴坡
            // =========================================

            if (carTransform != null)
            {
                Vector3 carDesired =
                    carTransform.position;


                carDesired.x +=
                    driverMoveDelta.x;


                carDesired.z +=
                    driverMoveDelta.z;


                NavMeshHit carHit;


                if (NavMesh.SamplePosition(
                    carDesired,
                    out carHit,
                    navMeshSearchDistance,
                    NavMesh.AllAreas))
                {
                    carDesired.y =
                        carHit.position.y +
                        carHeightOffset;
                }


                carTransform.position =
                    carDesired;
            }


            // =========================================
            // Push Stop 播放完成
            // =========================================

            if (stateInfo.normalizedTime >= 1.0f)
            {
                pushStopFinished = true;


                StopPushMovement();


                if (!thankSequenceStarted)
                {
                    thankSequenceStarted = true;


                    StartCoroutine(
                        ThankAndGoToCar()
                    );
                }
            }
        }
    }


    // ======================================================
    // Push Stop 开始
    // ======================================================

    private void StartPushStopMovement()
    {
        movementStarted = true;

        pushStopFinished = false;

        thankSequenceStarted = false;


        // =========================================
        // 停止 NavMeshAgent 抢位置
        // =========================================

        if (driverAgent != null)
        {
            driverAgent.isStopped = true;


            driverAgent.updatePosition =
                false;


            driverAgent.updateRotation =
                false;
        }


        // =========================================
        // Push Stop 开始
        //
        // 再保险：
        // 强制保持推车双手 IK
        //
        // Pushing → Push Stop
        // 手始终保持同一组 Target
        // =========================================

        if (driverPushHandIK != null)
        {
            driverPushHandIK.EnablePushHandIK();


            Debug.Log(
                "Push Stop开始 → 双手继续固定在推车点"
            );
        }


        // =========================================
        // 车轮开始转
        // =========================================

        if (carWheelSpin != null)
        {
            carWheelSpin.SetDriving();
        }


        Debug.Log(
            "Push Stop开始 → Driver和汽车开始移动"
        );
    }


    // ======================================================
    // Push Stop 结束
    // ======================================================

    private void StopPushMovement()
    {
        // =========================================
        // 1. 车轮停止
        // =========================================

        if (carWheelSpin != null)
        {
            carWheelSpin.StopWheels();
        }


        // =========================================
        // 2. Push Stop 真正完成以后
        // 才释放双手
        // =========================================

        if (driverPushHandIK != null)
        {
            driverPushHandIK.DisablePushHandIK();


            Debug.Log(
                "Push Stop结束 → 推车双手 IK 释放"
            );
        }


        // =========================================
        // 3. NavMeshAgent 同步最终位置
        // =========================================

        if (driverAgent != null)
        {
            driverAgent.nextPosition =
                transform.position;


            driverAgent.updatePosition =
                true;


            driverAgent.updateRotation =
                true;


            driverAgent.isStopped =
                true;
        }


        // =========================================
        // 4. Push Stop → Pause Idle
        // =========================================

        if (driverAnimator != null)
        {
            driverAnimator.SetTrigger(
                pauseTrigger
            );


            Debug.Log(
                "Push Stop结束 → Pause Idle"
            );
        }


        // =========================================
        // 5. 帮助完成 → 周围环境恢复彩色
        // =========================================

        if (environmentColorZone != null)
        {
            environmentColorZone.RestoreColorInZone();


            Debug.Log(
                "Push Stop结束 → Driver周围环境恢复彩色"
            );
        }
        else
        {
            Debug.LogWarning(
                "PushStopCarSync 没有设置 EnvironmentColorZone"
            );
        }
    }


    // ======================================================
    // 感谢
    // ======================================================

    private IEnumerator ThankAndGoToCar()
    {
        // =========================================
        // 推完先停2秒
        // =========================================

        yield return new WaitForSeconds(
            pauseBeforeTalking
        );


        // =========================================
        // 转向玩家
        // =========================================

        if (playerTransform != null)
        {
            Vector3 direction =
                playerTransform.position -
                transform.position;


            direction.y = 0f;


            if (direction.sqrMagnitude >
                0.001f)
            {
                Quaternion startRotation =
                    transform.rotation;


                Quaternion targetRotation =
                    Quaternion.LookRotation(
                        direction
                    );


                float timer = 0f;


                while (timer <
                       turnDuration)
                {
                    timer +=
                        Time.deltaTime;


                    float t =
                        Mathf.Clamp01(
                            timer /
                            turnDuration
                        );


                    float smoothT =
                        t * t *
                        (3f - 2f * t);


                    transform.rotation =
                        Quaternion.Slerp(
                            startRotation,
                            targetRotation,
                            smoothT
                        );


                    yield return null;
                }


                transform.rotation =
                    targetRotation;
            }
        }


        // =========================================
        // Talking
        // =========================================

        if (driverAnimator != null)
        {
            driverAnimator.SetTrigger(
                thankTrigger
            );
        }


        yield return null;


        // =========================================
        // Thank you 字幕
        // =========================================

        if (speechBubble != null)
        {
            speechBubble.ShowThankMessage();
        }


        // =========================================
        // Thank you 语音
        // =========================================

        if (driverAudioSource != null &&
            thankVoiceClip != null)
        {
            driverAudioSource.pitch =
                1.0f;


            driverAudioSource.clip =
                thankVoiceClip;


            driverAudioSource.Play();


            while (
                driverAudioSource.isPlaying)
            {
                yield return null;
            }
        }


        // =========================================
        // 语音结束
        // 字幕继续保留1.5秒
        // =========================================

        yield return new WaitForSeconds(
            pauseAfterVoice
        );


        if (speechBubble != null)
        {
            speechBubble.HideBubble();
        }


        // =========================================
        // Talking → Walking
        // =========================================

        if (driverAnimator != null)
        {
            driverAnimator.SetTrigger(
                goDriveTrigger
            );
        }


        // =========================================
        // 开始走向驾驶室
        // =========================================

        yield return StartCoroutine(
            WalkToDriverDoor()
        );
    }


    // ======================================================
    // Driver 走到驾驶室
    // ======================================================

    private IEnumerator WalkToDriverDoor()
    {
        if (driverAgent == null)
            yield break;


        driverAgent.updatePosition =
            true;


        driverAgent.updateRotation =
            true;


        driverAgent.isStopped =
            false;


        // =========================================
        // WalkPoint1
        // =========================================

        if (driverWalkPoint1 != null)
        {
            yield return StartCoroutine(
                MoveToPointWithPlayerAvoidance(
                    driverWalkPoint1
                )
            );
        }


        // =========================================
        // WalkPoint2
        // =========================================

        if (driverWalkPoint2 != null)
        {
            yield return StartCoroutine(
                MoveToPointWithPlayerAvoidance(
                    driverWalkPoint2
                )
            );
        }


        // =========================================
        // DriverDoorPoint
        // =========================================

        if (driverDoorPoint != null)
        {
            yield return StartCoroutine(
                MoveToPointWithPlayerAvoidance(
                    driverDoorPoint
                )
            );
        }


        driverAgent.isStopped =
            true;


        Debug.Log(
            "Driver到达驾驶室入口"
        );


        // =========================================
        // 开始上台阶
        // =========================================

        yield return StartCoroutine(
            StartStepUpSequence()
        );
    }


    // ======================================================
    // 上台阶
    // ======================================================

    private IEnumerator StartStepUpSequence()
    {
        // =========================================
        // 停止 NavMesh
        // =========================================

        if (driverAgent != null)
        {
            driverAgent.isStopped =
                true;


            driverAgent.updatePosition =
                false;


            driverAgent.updateRotation =
                false;
        }


        // =========================================
        // 原地转90度
        // =========================================

        Quaternion startRotation =
            transform.rotation;


        Quaternion targetRotation =
            startRotation *
            Quaternion.Euler(
                0f,
                stepTurnAngle,
                0f
            );


        float timer = 0f;


        while (timer <
               stepTurnDuration)
        {
            timer +=
                Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    timer /
                    stepTurnDuration
                );


            float smoothT =
                t * t *
                (3f - 2f * t);


            transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    smoothT
                );


            yield return null;
        }


        transform.rotation =
            targetRotation;


        // =========================================
        // 重置上车流程状态
        // =========================================

        enterCarTriggered =
            false;


        driveTriggered =
            false;


        enteringCarPositionInitialized =
            false;


        // =========================================
        // Ascending Stairs
        // =========================================

        if (driverAnimator != null)
        {
            driverAnimator.SetTrigger(
                stepUpTrigger
            );


            Debug.Log(
                "Driver转向汽车 → Ascending Stairs"
            );
        }
    }


    // ======================================================
    // 路线点 + 玩家避让
    // ======================================================

    private IEnumerator MoveToPointWithPlayerAvoidance(
        Transform targetPoint)
    {
        if (targetPoint == null ||
            driverAgent == null)
        {
            yield break;
        }


        Vector3 originalTarget =
            targetPoint.position;


        driverAgent.SetDestination(
            originalTarget
        );


        float checkTimer = 0f;


        while (true)
        {
            checkTimer -=
                Time.deltaTime;


            // =========================================
            // 玩家避让检查
            // =========================================

            if (checkTimer <= 0f)
            {
                checkTimer =
                    avoidCheckInterval;


                Vector3 destination =
                    GetAvoidedDestination(
                        originalTarget
                    );


                driverAgent.SetDestination(
                    destination
                );
            }


            // =========================================
            // 是否到达
            // =========================================

            if (!driverAgent.pathPending &&
                driverAgent.remainingDistance <=
                waypointReachDistance)
            {
                Vector3 flatDriver =
                    transform.position;


                flatDriver.y = 0f;


                Vector3 flatTarget =
                    originalTarget;


                flatTarget.y = 0f;


                float realDistance =
                    Vector3.Distance(
                        flatDriver,
                        flatTarget
                    );


                if (realDistance <=
                    waypointReachDistance)
                {
                    break;
                }


                driverAgent.SetDestination(
                    originalTarget
                );
            }


            yield return null;
        }
    }


    // ======================================================
    // 玩家动态避让
    // ======================================================

    private Vector3 GetAvoidedDestination(
        Vector3 originalTarget)
    {
        if (playerTransform == null)
            return originalTarget;


        Vector3 driverPos =
            transform.position;


        Vector3 playerPos =
            playerTransform.position;


        driverPos.y = 0f;

        playerPos.y = 0f;


        Vector3 toPlayer =
            playerPos -
            driverPos;


        // =========================================
        // 玩家距离够远
        // 正常走
        // =========================================

        if (toPlayer.magnitude >
            playerAvoidDistance)
        {
            return originalTarget;
        }


        Vector3 toTarget =
            originalTarget -
            transform.position;


        toTarget.y = 0f;


        if (toTarget.sqrMagnitude <
            0.001f)
        {
            return originalTarget;
        }


        Vector3 forward =
            toTarget.normalized;


        Vector3 right =
            Vector3.Cross(
                Vector3.up,
                forward
            ).normalized;


        // =========================================
        // 玩家在哪一边
        // =========================================

        float playerSide =
            Vector3.Dot(
                toPlayer.normalized,
                right
            );


        Vector3 preferredSide =
            playerSide >= 0f
            ? -right
            : right;


        // =========================================
        // 尝试第一侧
        // =========================================

        Vector3 candidate =
            transform.position +
            preferredSide *
            sideAvoidDistance +
            forward *
            forwardAvoidDistance;


        NavMeshHit avoidHit;


        if (NavMesh.SamplePosition(
            candidate,
            out avoidHit,
            avoidNavMeshSearchDistance,
            NavMesh.AllAreas))
        {
            return avoidHit.position;
        }


        // =========================================
        // 尝试另外一侧
        // =========================================

        candidate =
            transform.position -
            preferredSide *
            sideAvoidDistance +
            forward *
            forwardAvoidDistance;


        if (NavMesh.SamplePosition(
            candidate,
            out avoidHit,
            avoidNavMeshSearchDistance,
            NavMesh.AllAreas))
        {
            return avoidHit.position;
        }


        return originalTarget;
    }
}