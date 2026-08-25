using UnityEngine;
using UnityEngine.AI;

public class PushStopCarSync : MonoBehaviour
{
    [Header("Driver")]
    public Animator driverAnimator;
    public NavMeshAgent driverAgent;

    [Header("汽车根物体")]
    public Transform carTransform;

    [Header("车轮控制")]
    public CarWheelSpin carWheelSpin;

    [Header("Push Stop 状态名字")]
    public string pushStopStateName = "Push Stop";

    [Header("NavMesh 搜索距离")]
    public float navMeshSearchDistance = 1.0f;

    [Header("Driver脚底高度微调")]
    public float driverHeightOffset = 0.2f;

    [Header("汽车高度微调")]
    public float carHeightOffset = 0.02f;

    private bool wasInPushStop = false;
    private bool movementStarted = false;

    private void OnAnimatorMove()
    {
        if (driverAnimator == null)
            return;

        AnimatorStateInfo stateInfo =
            driverAnimator.GetCurrentAnimatorStateInfo(0);

        bool isPushStop =
            stateInfo.IsName(pushStopStateName);

        // =========================================
        // 正在播放 Push Stop
        // =========================================
        if (isPushStop)
        {
            if (!wasInPushStop)
            {
                StartPushStopMovement();
            }

            // 动画这一帧的 Root Motion
            Vector3 rootDelta =
                driverAnimator.deltaPosition;

            Quaternion deltaRotation =
                driverAnimator.deltaRotation;

            // =========================================
            // 1. Driver 先按 Root Motion 算 X/Z
            // =========================================
            Vector3 driverDesired =
                transform.position +
                rootDelta;

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

            // 记录 Driver 这一帧 X/Z 实际移动量
            Vector3 driverBefore =
                transform.position;

            transform.position =
                driverDesired;

            transform.rotation =
                deltaRotation *
                transform.rotation;

            Vector3 driverMoveDelta =
                transform.position -
                driverBefore;

            // =========================================
            // 2. 汽车只跟 Driver 的 X/Z 位移
            //    Y 自己重新贴地
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

            wasInPushStop = true;
        }
        else
        {
            if (wasInPushStop)
            {
                FinishPushStopMovement();
            }

            wasInPushStop = false;
        }
    }

    private void StartPushStopMovement()
    {
        if (movementStarted)
            return;

        movementStarted = true;

        // 防止 NavMeshAgent 和 Root Motion 抢位置
        if (driverAgent != null)
        {
            driverAgent.isStopped = true;
            driverAgent.updatePosition = false;
            driverAgent.updateRotation = false;
        }

        // 四轮开始转
        if (carWheelSpin != null)
        {
            carWheelSpin.SetDriving();
        }

        Debug.Log(
            "Push Stop开始 → Driver和车分别贴坡前进 → 四轮开始转"
        );
    }

    private void FinishPushStopMovement()
    {
        // 四轮停止
        if (carWheelSpin != null)
        {
            carWheelSpin.StopWheels();
        }

        // 同步 NavMeshAgent 到 Driver 最终位置
        if (driverAgent != null)
        {
            driverAgent.nextPosition =
                transform.position;

            driverAgent.updatePosition = true;
            driverAgent.updateRotation = true;

            driverAgent.isStopped = true;
        }

        movementStarted = false;

        Debug.Log(
            "Push Stop结束 → Driver和车停止 → 四轮停止"
        );
    }
}