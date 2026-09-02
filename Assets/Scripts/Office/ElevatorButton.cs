using System.Collections;
using UnityEngine;

public class ElevatorButton : MonoBehaviour
{
    // =====================================================
    // 按钮零件
    // =====================================================

    [Header("按钮零件")]
    [Tooltip("拖入真正需要往下移动的按钮零件")]
    public Transform buttonPart;


    // =====================================================
    // 电梯门
    // =====================================================

    [Header("电梯门")]
    [Tooltip("拖入 ElevatorDoorSystem")]
    public ElevatorDoorController elevatorDoor;


    // =====================================================
    // 按钮移动设置
    // =====================================================

    [Header("按钮按下设置")]

    [Tooltip("按钮往下移动多少米")]
    public float pressDistance = 0.02f;

    [Tooltip("按钮按下和弹回的速度")]
    public float pressSpeed = 0.15f;

    [Tooltip("按钮按到底后停留多久")]
    public float holdTime = 0.15f;


    // =====================================================
    // 键盘测试
    // =====================================================

    [Header("键盘测试")]

    [Tooltip("勾选后，可以在 Play 模式下用键盘测试")]
    public bool enableKeyboardTest = true;

    [Tooltip("默认按 E 测试")]
    public KeyCode testKey = KeyCode.E;


    // =====================================================
    // 状态
    // =====================================================

    [Header("状态")]

    public bool isPressing = false;

    private Vector3 originalLocalPosition;


    // =====================================================
    // Start
    // =====================================================

    private void Start()
    {
        if (buttonPart == null)
        {
            buttonPart = transform;
        }

        originalLocalPosition =
            buttonPart.localPosition;
    }


    // =====================================================
    // Update
    // =====================================================

    private void Update()
    {
        // 键盘测试
        if (enableKeyboardTest &&
            Input.GetKeyDown(testKey))
        {
            PressButton();
        }
    }


    // =====================================================
    // 按按钮
    // XR 和键盘都调用这个
    // =====================================================

    public void PressButton()
    {
        if (isPressing)
            return;

        StartCoroutine(
            PressRoutine()
        );
    }


    // =====================================================
    // 按钮动画
    // =====================================================

    private IEnumerator PressRoutine()
    {
        isPressing = true;


        // =================================================
        // 按钮向下
        //
        // 使用 localPosition：
        // Y 减小 = 向下
        // =================================================

        Vector3 pressedPosition =
            originalLocalPosition +
            Vector3.down * pressDistance;


        // =================================================
        // 1. 慢慢按下
        // =================================================

        while (
            Vector3.Distance(
                buttonPart.localPosition,
                pressedPosition
            ) > 0.0001f
        )
        {
            buttonPart.localPosition =
                Vector3.MoveTowards(
                    buttonPart.localPosition,
                    pressedPosition,
                    pressSpeed * Time.deltaTime
                );

            yield return null;
        }


        buttonPart.localPosition =
            pressedPosition;


        // =================================================
        // 2. 按到底 → 打开电梯门
        // =================================================

        if (elevatorDoor != null)
        {
            elevatorDoor.OpenDoor();

            Debug.Log(
                "[ElevatorButton] 按钮按下，开始打开电梯门。"
            );
        }
        else
        {
            Debug.LogWarning(
                "[ElevatorButton] Elevator Door 没有拖入！"
            );
        }


        // =================================================
        // 3. 按钮保持一下
        // =================================================

        yield return new WaitForSeconds(
            holdTime
        );


        // =================================================
        // 4. 按钮弹回来
        // =================================================

        while (
            Vector3.Distance(
                buttonPart.localPosition,
                originalLocalPosition
            ) > 0.0001f
        )
        {
            buttonPart.localPosition =
                Vector3.MoveTowards(
                    buttonPart.localPosition,
                    originalLocalPosition,
                    pressSpeed * Time.deltaTime
                );

            yield return null;
        }


        buttonPart.localPosition =
            originalLocalPosition;

        isPressing = false;


        Debug.Log(
            "[ElevatorButton] 按钮弹回完成。"
        );
    }
}