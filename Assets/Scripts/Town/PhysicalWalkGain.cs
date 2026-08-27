using UnityEngine;

public class PhysicalWalkGain : MonoBehaviour
{
    [Header("XR Origin")]
    public Transform xrOrigin;

    [Header("头显")]
    public Transform headset;

    [Header("Camera Offset")]
    public Transform trackingSpace;

    [Header("真实走路放大")]
    [Tooltip("1 = 正常，2 = 现实走0.5米，虚拟大约走1米")]
    public float walkGain = 2f;

    [Header("是否启用")]
    public bool gainEnabled = false;

    private Vector3 lastLocalHeadPosition;
    private bool initialized = false;

    void LateUpdate()
    {
        if (!gainEnabled)
        {
            initialized = false;
            return;
        }

        if (xrOrigin == null ||
            headset == null ||
            trackingSpace == null)
        {
            return;
        }

        // 获取头显相对于 Camera Offset 的真实追踪位置
        Vector3 currentLocalHeadPosition =
            trackingSpace.InverseTransformPoint(
                headset.position
            );

        if (!initialized)
        {
            lastLocalHeadPosition =
                currentLocalHeadPosition;

            initialized = true;
            return;
        }

        // 计算这一帧玩家现实中真正走了多少
        Vector3 physicalDelta =
            currentLocalHeadPosition -
            lastLocalHeadPosition;

        // 只放大水平移动，不改变身高
        physicalDelta.y = 0f;

        // 额外增加的虚拟移动量
        Vector3 extraLocalMovement =
            physicalDelta *
            (walkGain - 1f);

        // 转换成世界方向
        Vector3 extraWorldMovement =
            trackingSpace.TransformDirection(
                extraLocalMovement
            );

        extraWorldMovement.y = 0f;

        // 移动整个 XR Origin
        xrOrigin.position +=
            extraWorldMovement;

        lastLocalHeadPosition =
            currentLocalHeadPosition;
    }


    // 开始放大真实走路
    public void StartWalkGain()
    {
        gainEnabled = true;
        initialized = false;

        Debug.Log("上车真实步行放大开启");
    }


    // 停止放大
    public void StopWalkGain()
    {
        gainEnabled = false;
        initialized = false;

        Debug.Log("上车真实步行放大关闭");
    }
}