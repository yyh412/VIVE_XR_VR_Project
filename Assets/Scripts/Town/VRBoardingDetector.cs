using UnityEngine;

public class VRBoardingDetector : MonoBehaviour
{
    [Header("VR头显")]
    [Tooltip("拖入 XR Origin 下面的 Main Camera")]
    public Transform headset;


    [Header("上车检测区域")]
    [Tooltip("拖入 BoardingZone 的 Box Collider")]
    public BoxCollider boardingZone;


    [Header("脚印")]
    public FootprintGuide footprintGuide;


    [Header("脚踏板蓝色呼吸灯")]
    public StepBoardGlowPulse stepBoardGlow;


    [Header("乘客控制")]
    [Tooltip("拖入 XR Origin 上的 CarPassengerController")]
    public CarPassengerController carPassengerController;


    [Header("调试")]
    public bool showDebugLog = true;


    private bool hasBoarded = false;


    private void Update()
    {
        // 已经上车后不再重复检测
        if (hasBoarded)
            return;


        if (headset == null ||
            boardingZone == null)
        {
            return;
        }


        // ==================================================
        // 获取真实头显追踪后的世界坐标
        // ==================================================

        Vector3 headPosition =
            headset.position;


        // BoardingZone 世界范围
        Bounds zoneBounds =
            boardingZone.bounds;


        // ==================================================
        // 判断头显是否真正进入 BoardingZone
        // ==================================================

        bool insideX =
            headPosition.x >=
            zoneBounds.min.x &&
            headPosition.x <=
            zoneBounds.max.x;


        bool insideY =
            headPosition.y >=
            zoneBounds.min.y &&
            headPosition.y <=
            zoneBounds.max.y;


        bool insideZ =
            headPosition.z >=
            zoneBounds.min.z &&
            headPosition.z <=
            zoneBounds.max.z;


        // ==================================================
        // 玩家真实走进脚踏板区域
        // ==================================================

        if (insideX &&
            insideY &&
            insideZ)
        {
            PlayerBoarded();
        }
    }


    // ======================================================
    // 上车成功
    // ======================================================

    private void PlayerBoarded()
    {
        if (hasBoarded)
            return;


        hasBoarded = true;


        if (showDebugLog)
        {
            Debug.Log(
                "玩家真实走到脚踏板 → Boarding Success!"
            );
        }


        // ==================================================
        // 1. 删除所有引导脚印
        // ==================================================

        if (footprintGuide != null)
        {
            footprintGuide.PlayerBoarded();
        }


        // ==================================================
        // 2. 关闭脚踏板蓝色呼吸灯
        // ==================================================

        if (stepBoardGlow != null)
        {
            stepBoardGlow.StopGlow();
        }


        // ==================================================
        // 3. VR玩家抬高到脚踏板
        //    然后进入 Passenger Mode
        // ==================================================

        if (carPassengerController != null)
        {
            carPassengerController.BeginBoarding();
        }
        else
        {
            Debug.LogWarning(
                "VRBoardingDetector：Car Passenger Controller 没有设置！"
            );
        }
    }


    // ======================================================
    // 是否已经上车
    // 后面汽车启动时可以查询
    // ======================================================

    public bool HasBoarded()
    {
        return hasBoarded;
    }


    // ======================================================
    // 重置
    // ======================================================

    public void ResetBoarding()
    {
        hasBoarded = false;
    }
}