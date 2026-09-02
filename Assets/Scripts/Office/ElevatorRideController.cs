using System.Collections;
using UnityEngine;

public class ElevatorRideController : MonoBehaviour
{
    // =====================================================
    // 玩家
    // =====================================================

    [Header("玩家 XR Origin")]
    [Tooltip("拖入 XR Origin (XR Rig)")]
    public Transform xrOrigin;


    // =====================================================
    // 电梯轿厢
    // =====================================================

    [Header("电梯轿厢零件")]
    [Tooltip("所有需要跟着电梯一起上升的零件")]
    public Transform[] cabinParts;


    // =====================================================
    // 电梯门
    // =====================================================

    [Header("电梯门")]
    [Tooltip("拖入 ElevatorDoorSystem")]
    public ElevatorDoorController elevatorDoor;


    // =====================================================
    // 3楼目标
    // =====================================================

    [Header("3楼目标")]
    [Tooltip("拖入 ElevatorFloor3Target")]
    public Transform floor3Target;


    // =====================================================
    // 电梯移动参数
    // =====================================================

    [Header("电梯移动设置")]

    [Tooltip("电梯每秒移动多少米")]
    public float moveSpeed = 2f;

    [Tooltip("按3以后，关门后等待多久开始上升")]
    public float delayBeforeMove = 1.0f;

    [Tooltip("到达3楼后等待多久开门")]
    public float delayBeforeOpenDoor = 0.5f;

    [Tooltip("到达目标高度允许误差")]
    public float arriveThreshold = 0.01f;


    // =====================================================
    // 键盘测试
    // =====================================================

    [Header("键盘测试")]

    [Tooltip("勾选后 Play 模式按数字3可以测试")]
    public bool enableKeyboardTest = true;

    [Tooltip("默认键盘数字3")]
    public KeyCode floor3TestKey = KeyCode.Alpha3;


    // =====================================================
    // 状态
    // =====================================================

    [Header("状态")]

    [Tooltip("电梯是否正在移动")]
    public bool isMoving = false;

    [Tooltip("当前楼层")]
    public int currentFloor = 1;


    // =====================================================
    // Update
    // =====================================================

    private void Update()
    {
        // 键盘测试
        if (enableKeyboardTest &&
            Input.GetKeyDown(floor3TestKey))
        {
            GoToFloor3();
        }
    }


    // =====================================================
    // 去3楼
    //
    // ★ 这是给 VR 按钮调用的公开函数
    // =====================================================

    public void GoToFloor3()
    {
        // 已经在运行
        if (isMoving)
        {
            Debug.Log(
                "[ElevatorRide] 电梯正在运行。"
            );

            return;
        }


        // 已经在3楼
        if (currentFloor == 3)
        {
            Debug.Log(
                "[ElevatorRide] 已经在3楼。"
            );

            return;
        }


        // 没有设置3楼目标
        if (floor3Target == null)
        {
            Debug.LogWarning(
                "[ElevatorRide] Floor3 Target 没有拖入！"
            );

            return;
        }


        StartCoroutine(
            GoToFloor3Routine()
        );
    }


    // =====================================================
    // 3楼完整流程
    // =====================================================

    private IEnumerator GoToFloor3Routine()
    {
        isMoving = true;


        Debug.Log(
            "[ElevatorRide] 收到去3楼指令。"
        );


        // =================================================
        // 1. 关闭电梯门
        // =================================================

        if (elevatorDoor != null)
        {
            elevatorDoor.CloseDoor();

            Debug.Log(
                "[ElevatorRide] 开始关门。"
            );
        }
        else
        {
            Debug.LogWarning(
                "[ElevatorRide] Elevator Door 没有拖入！"
            );
        }


        // =================================================
        // 2. 等待关门
        // =================================================

        yield return new WaitForSeconds(
            delayBeforeMove
        );


        // =================================================
        // 3. 检查 Cabin Parts
        // =================================================

        if (cabinParts == null ||
            cabinParts.Length == 0)
        {
            Debug.LogWarning(
                "[ElevatorRide] Cabin Parts 是空的！"
            );

            isMoving = false;

            yield break;
        }


        // =================================================
        // 4. 找到一个轿厢参考零件
        // =================================================

        Transform referencePart = null;


        for (int i = 0;
             i < cabinParts.Length;
             i++)
        {
            if (cabinParts[i] != null)
            {
                referencePart =
                    cabinParts[i];

                break;
            }
        }


        if (referencePart == null)
        {
            Debug.LogWarning(
                "[ElevatorRide] Cabin Parts 里面没有有效零件！"
            );

            isMoving = false;

            yield break;
        }


        // =================================================
        // 5. 计算上升距离
        //
        // 只看 Y
        // X 和 Z 完全不改变
        // =================================================

        float startReferenceY =
            referencePart.position.y;


        float targetY =
            floor3Target.position.y;


        float moveDistanceY =
            targetY -
            startReferenceY;


        Debug.Log(
            "[ElevatorRide] 当前参考高度 = " +
            startReferenceY.ToString("F3") +
            " | 3楼目标高度 = " +
            targetY.ToString("F3") +
            " | 电梯移动距离 = " +
            moveDistanceY.ToString("F3")
        );


        // =================================================
        // 6. 保存所有 Cabin Parts 初始世界坐标
        // =================================================

        Vector3[] cabinStartPositions =
            new Vector3[cabinParts.Length];


        for (int i = 0;
             i < cabinParts.Length;
             i++)
        {
            if (cabinParts[i] != null)
            {
                cabinStartPositions[i] =
                    cabinParts[i].position;
            }
        }


        // =================================================
        // 7. 保存 XR Origin 初始位置
        // =================================================

        Vector3 xrStartPosition =
            Vector3.zero;


        if (xrOrigin != null)
        {
            xrStartPosition =
                xrOrigin.position;
        }
        else
        {
            Debug.LogWarning(
                "[ElevatorRide] XR Origin 没有拖入！" +
                " 电梯会移动，但是玩家不会跟着移动。"
            );
        }


        // =================================================
        // 8. 开始垂直上升
        // =================================================

        float movedY = 0f;


        Debug.Log(
            "[ElevatorRide] 电梯开始上升。"
        );


        while (
            Mathf.Abs(
                moveDistanceY -
                movedY
            ) > arriveThreshold
        )
        {
            // ---------------------------------------------
            // 每帧逐渐靠近最终移动距离
            // ---------------------------------------------

            movedY =
                Mathf.MoveTowards(
                    movedY,
                    moveDistanceY,
                    moveSpeed *
                    Time.deltaTime
                );


            Vector3 offset =
                new Vector3(
                    0f,
                    movedY,
                    0f
                );


            // ---------------------------------------------
            // 移动整个电梯轿厢
            // ---------------------------------------------

            for (int i = 0;
                 i < cabinParts.Length;
                 i++)
            {
                if (cabinParts[i] == null)
                    continue;


                cabinParts[i].position =
                    cabinStartPositions[i] +
                    offset;
            }


            // ---------------------------------------------
            // XR Origin 同步移动
            // ---------------------------------------------

            if (xrOrigin != null)
            {
                xrOrigin.position =
                    xrStartPosition +
                    offset;
            }


            yield return null;
        }


        // =================================================
        // 9. 最终精确定位
        // =================================================

        Vector3 finalOffset =
            new Vector3(
                0f,
                moveDistanceY,
                0f
            );


        for (int i = 0;
             i < cabinParts.Length;
             i++)
        {
            if (cabinParts[i] == null)
                continue;


            cabinParts[i].position =
                cabinStartPositions[i] +
                finalOffset;
        }


        if (xrOrigin != null)
        {
            xrOrigin.position =
                xrStartPosition +
                finalOffset;
        }


        // =================================================
        // 10. 到达3楼
        // =================================================

        currentFloor = 3;


        Debug.Log(
            "[ElevatorRide] 已到达3楼。"
        );


        // =================================================
        // 11. 等一下
        // =================================================

        yield return new WaitForSeconds(
            delayBeforeOpenDoor
        );


        // =================================================
        // 12. 打开电梯门
        // =================================================

        if (elevatorDoor != null)
        {
            elevatorDoor.OpenDoor();

            Debug.Log(
                "[ElevatorRide] 3楼电梯门打开。"
            );
        }


        // =================================================
        // 完成
        // =================================================

        isMoving = false;
    }
}