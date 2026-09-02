using System.Collections;
using UnityEngine;

public class ElevatorDoorController : MonoBehaviour
{
    // =====================================================
    // 左门零件
    // =====================================================

    [Header("左门所有零件")]
    [Tooltip("把所有需要向左移动的门零件拖进来")]
    public Transform[] leftDoorParts;


    // =====================================================
    // 右门零件
    // =====================================================

    [Header("右门所有零件")]
    [Tooltip("把所有需要向右移动的门零件拖进来")]
    public Transform[] rightDoorParts;


    // =====================================================
    // 开门设置
    // =====================================================

    [Header("开门设置")]

    [Tooltip("左门打开方向，默认世界坐标向左")]
    public Vector3 leftOpenDirection = Vector3.left;

    [Tooltip("右门打开方向，默认世界坐标向右")]
    public Vector3 rightOpenDirection = Vector3.right;

    [Tooltip("左右门各自移动多少米")]
    public float openDistance = 1.2f;

    [Tooltip("开门速度")]
    public float openSpeed = 1.5f;


    // =====================================================
    // 状态
    // =====================================================

    [Header("状态")]

    [Tooltip("当前门是否已经打开")]
    public bool isOpen = false;

    [Tooltip("当前门是否正在移动")]
    public bool isMoving = false;


    // =====================================================
    // 内部记录
    // =====================================================

    private Vector3[] leftClosedPositions;
    private Vector3[] rightClosedPositions;

    private Vector3[] leftOpenPositions;
    private Vector3[] rightOpenPositions;


    // =====================================================
    // Start
    // =====================================================

    private void Start()
    {
        SaveInitialPositions();
    }


    // =====================================================
    // 保存所有门零件初始位置
    // =====================================================

    private void SaveInitialPositions()
    {
        // -------------------------
        // 左门
        // -------------------------

        if (leftDoorParts != null)
        {
            leftClosedPositions =
                new Vector3[leftDoorParts.Length];

            leftOpenPositions =
                new Vector3[leftDoorParts.Length];


            for (int i = 0; i < leftDoorParts.Length; i++)
            {
                if (leftDoorParts[i] == null)
                    continue;


                // 使用世界坐标
                leftClosedPositions[i] =
                    leftDoorParts[i].position;


                leftOpenPositions[i] =
                    leftClosedPositions[i] +
                    leftOpenDirection.normalized *
                    openDistance;
            }
        }


        // -------------------------
        // 右门
        // -------------------------

        if (rightDoorParts != null)
        {
            rightClosedPositions =
                new Vector3[rightDoorParts.Length];

            rightOpenPositions =
                new Vector3[rightDoorParts.Length];


            for (int i = 0; i < rightDoorParts.Length; i++)
            {
                if (rightDoorParts[i] == null)
                    continue;


                rightClosedPositions[i] =
                    rightDoorParts[i].position;


                rightOpenPositions[i] =
                    rightClosedPositions[i] +
                    rightOpenDirection.normalized *
                    openDistance;
            }
        }
    }


    // =====================================================
    // 打开电梯门
    // 给 ElevatorButton 调用
    // =====================================================

    public void OpenDoor()
    {
        if (isOpen)
            return;

        if (isMoving)
            return;


        StartCoroutine(
            OpenDoorRoutine()
        );
    }


    // =====================================================
    // 开门动画
    // =====================================================

    private IEnumerator OpenDoorRoutine()
    {
        isMoving = true;


        // -------------------------
        // 当前开始位置
        // -------------------------

        Vector3[] leftStartPositions =
            new Vector3[leftDoorParts != null ? leftDoorParts.Length : 0];

        Vector3[] rightStartPositions =
            new Vector3[rightDoorParts != null ? rightDoorParts.Length : 0];


        if (leftDoorParts != null)
        {
            for (int i = 0; i < leftDoorParts.Length; i++)
            {
                if (leftDoorParts[i] != null)
                {
                    leftStartPositions[i] =
                        leftDoorParts[i].position;
                }
            }
        }


        if (rightDoorParts != null)
        {
            for (int i = 0; i < rightDoorParts.Length; i++)
            {
                if (rightDoorParts[i] != null)
                {
                    rightStartPositions[i] =
                        rightDoorParts[i].position;
                }
            }
        }


        float progress = 0f;


        // =================================================
        // 慢慢打开
        // =================================================

        while (progress < 1f)
        {
            progress +=
                Time.deltaTime *
                openSpeed;


            float smoothProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(progress)
                );


            // -------------------------
            // 左门一起移动
            // -------------------------

            if (leftDoorParts != null)
            {
                for (int i = 0; i < leftDoorParts.Length; i++)
                {
                    if (leftDoorParts[i] == null)
                        continue;


                    leftDoorParts[i].position =
                        Vector3.Lerp(
                            leftStartPositions[i],
                            leftOpenPositions[i],
                            smoothProgress
                        );
                }
            }


            // -------------------------
            // 右门一起移动
            // -------------------------

            if (rightDoorParts != null)
            {
                for (int i = 0; i < rightDoorParts.Length; i++)
                {
                    if (rightDoorParts[i] == null)
                        continue;


                    rightDoorParts[i].position =
                        Vector3.Lerp(
                            rightStartPositions[i],
                            rightOpenPositions[i],
                            smoothProgress
                        );
                }
            }


            yield return null;
        }


        // =================================================
        // 最后强制放到准确位置
        // =================================================

        if (leftDoorParts != null)
        {
            for (int i = 0; i < leftDoorParts.Length; i++)
            {
                if (leftDoorParts[i] != null)
                {
                    leftDoorParts[i].position =
                        leftOpenPositions[i];
                }
            }
        }


        if (rightDoorParts != null)
        {
            for (int i = 0; i < rightDoorParts.Length; i++)
            {
                if (rightDoorParts[i] != null)
                {
                    rightDoorParts[i].position =
                        rightOpenPositions[i];
                }
            }
        }


        isOpen = true;
        isMoving = false;


        Debug.Log(
            "[ElevatorDoorController] 电梯门打开完成。"
        );
    }


    // =====================================================
    // 测试用：重新关闭
    // 后面如果需要关门可以直接调用
    // =====================================================

    public void CloseDoor()
    {
        if (!isOpen)
            return;

        if (isMoving)
            return;


        StartCoroutine(
            CloseDoorRoutine()
        );
    }


    // =====================================================
    // 关门动画
    // =====================================================

    private IEnumerator CloseDoorRoutine()
    {
        isMoving = true;


        Vector3[] leftStartPositions =
            new Vector3[leftDoorParts != null ? leftDoorParts.Length : 0];

        Vector3[] rightStartPositions =
            new Vector3[rightDoorParts != null ? rightDoorParts.Length : 0];


        if (leftDoorParts != null)
        {
            for (int i = 0; i < leftDoorParts.Length; i++)
            {
                if (leftDoorParts[i] != null)
                {
                    leftStartPositions[i] =
                        leftDoorParts[i].position;
                }
            }
        }


        if (rightDoorParts != null)
        {
            for (int i = 0; i < rightDoorParts.Length; i++)
            {
                if (rightDoorParts[i] != null)
                {
                    rightStartPositions[i] =
                        rightDoorParts[i].position;
                }
            }
        }


        float progress = 0f;


        while (progress < 1f)
        {
            progress +=
                Time.deltaTime *
                openSpeed;


            float smoothProgress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(progress)
                );


            // -------------------------
            // 左门关闭
            // -------------------------

            if (leftDoorParts != null)
            {
                for (int i = 0; i < leftDoorParts.Length; i++)
                {
                    if (leftDoorParts[i] == null)
                        continue;


                    leftDoorParts[i].position =
                        Vector3.Lerp(
                            leftStartPositions[i],
                            leftClosedPositions[i],
                            smoothProgress
                        );
                }
            }


            // -------------------------
            // 右门关闭
            // -------------------------

            if (rightDoorParts != null)
            {
                for (int i = 0; i < rightDoorParts.Length; i++)
                {
                    if (rightDoorParts[i] == null)
                        continue;


                    rightDoorParts[i].position =
                        Vector3.Lerp(
                            rightStartPositions[i],
                            rightClosedPositions[i],
                            smoothProgress
                        );
                }
            }


            yield return null;
        }


        // 最后固定位置
        if (leftDoorParts != null)
        {
            for (int i = 0; i < leftDoorParts.Length; i++)
            {
                if (leftDoorParts[i] != null)
                {
                    leftDoorParts[i].position =
                        leftClosedPositions[i];
                }
            }
        }


        if (rightDoorParts != null)
        {
            for (int i = 0; i < rightDoorParts.Length; i++)
            {
                if (rightDoorParts[i] != null)
                {
                    rightDoorParts[i].position =
                        rightClosedPositions[i];
                }
            }
        }


        isOpen = false;
        isMoving = false;


        Debug.Log(
            "[ElevatorDoorController] 电梯门关闭完成。"
        );
    }
}