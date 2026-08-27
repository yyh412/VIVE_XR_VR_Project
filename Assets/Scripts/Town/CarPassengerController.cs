using System.Collections;
using UnityEngine;

public class CarPassengerController : MonoBehaviour
{
    [Header("XR 玩家")]
    [Tooltip("拖入 XR Origin (XR Rig)")]
    public Transform xrOrigin;

    [Tooltip("拖入 XR Origin 下面的 Main Camera")]
    public Transform headset;


    [Header("汽车")]
    [Tooltip("拖入汽车最外层根物体")]
    public Transform carTransform;


    [Header("乘客默认站立点")]
    [Tooltip("拖入汽车下面的 PassengerAnchor")]
    public Transform passengerAnchor;


    [Header("乘客允许活动范围")]
    [Tooltip("拖入 PassengerBounds 的 Box Collider")]
    public BoxCollider passengerBounds;


    [Header("上车抬高")]
    [Tooltip("现实中走平地，进入脚踏板时，VR玩家整体向上抬高多少米")]
    public float boardHeightOffset = 0.25f;

    [Tooltip("抬高过程持续多久")]
    public float boardLiftDuration = 0.35f;


    [Header("脚踏板范围限制")]
    [Tooltip("距离 PassengerBounds 边缘预留的安全距离")]
    public float edgePadding = 0.05f;


    [Header("状态")]
    [Tooltip("正常情况下游戏开始不要勾选")]
    public bool passengerMode = false;


    private bool isBoarding = false;


    private void LateUpdate()
    {
        // 只有玩家真正上车以后才限制范围
        if (!passengerMode)
            return;

        if (xrOrigin == null ||
            headset == null ||
            passengerBounds == null)
        {
            return;
        }


        // ==================================================
        // 读取 PassengerBounds 的世界范围
        // ==================================================

        Bounds bounds =
            passengerBounds.bounds;


        // 当前头显世界坐标
        Vector3 headPosition =
            headset.position;


        // ==================================================
        // 计算允许的 X / Z 范围
        // ==================================================

        float minX =
            bounds.min.x + edgePadding;

        float maxX =
            bounds.max.x - edgePadding;

        float minZ =
            bounds.min.z + edgePadding;

        float maxZ =
            bounds.max.z - edgePadding;


        // ==================================================
        // 把头显限制在脚踏板范围内
        // ==================================================

        float clampedX =
            Mathf.Clamp(
                headPosition.x,
                minX,
                maxX
            );

        float clampedZ =
            Mathf.Clamp(
                headPosition.z,
                minZ,
                maxZ
            );


        // ==================================================
        // 计算需要补偿 XR Origin 多少
        //
        // 无论是：
        // 1. 玩家现实走路
        // 2. 玩家手柄摇杆移动
        //
        // 最终只要 Main Camera 想跑出脚踏板，
        // 就把 XR Origin 拉回来。
        // ==================================================

        Vector3 correction =
            Vector3.zero;

        correction.x =
            clampedX - headPosition.x;

        correction.z =
            clampedZ - headPosition.z;


        // 只修正水平位置
        // 不限制玩家蹲下、站起来、转头
        xrOrigin.position +=
            correction;
    }


    // ======================================================
    // 玩家上车
    // ======================================================

    public void BeginBoarding()
    {
        if (passengerMode)
            return;

        if (isBoarding)
            return;


        StartCoroutine(
            BoardingRoutine()
        );
    }


    // ======================================================
    // 上车过程
    //
    // 现实世界：
    // 玩家还是走平地
    //
    // VR：
    // XR Origin 平滑抬高
    // 看起来像真正站上脚踏板
    // ======================================================

    private IEnumerator BoardingRoutine()
    {
        isBoarding = true;


        if (xrOrigin == null)
        {
            Debug.LogWarning(
                "CarPassengerController：XR Origin 没有设置！"
            );

            isBoarding = false;

            yield break;
        }


        Vector3 startPosition =
            xrOrigin.position;


        Vector3 targetPosition =
            startPosition +
            Vector3.up *
            boardHeightOffset;


        float timer = 0f;


        // ==================================================
        // 平滑抬高
        // ==================================================

        while (timer <
               boardLiftDuration)
        {
            timer +=
                Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    timer /
                    boardLiftDuration
                );


            // SmoothStep
            float smoothT =
                t * t *
                (3f - 2f * t);


            Vector3 newPosition =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    smoothT
                );


            xrOrigin.position =
                newPosition;


            yield return null;
        }


        // 确保最后准确到目标高度
        xrOrigin.position =
            targetPosition;


        // ==================================================
        // 正式进入乘客模式
        // ==================================================

        passengerMode = true;

        isBoarding = false;


        Debug.Log(
            "玩家上车完成 → VR视角已抬高 → Passenger Mode 开启"
        );
    }


    // ======================================================
    // 退出乘客模式
    // 后面玩家到达面试地点下车时使用
    // ======================================================

    public void ExitPassengerMode()
    {
        passengerMode = false;

        isBoarding = false;


        Debug.Log(
            "Passenger Mode 关闭"
        );
    }


    // ======================================================
    // 外部查询
    // ======================================================

    public bool IsPassenger()
    {
        return passengerMode;
    }
}