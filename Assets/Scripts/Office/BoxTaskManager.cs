using UnityEngine;

public class BoxTaskManager : MonoBehaviour
{
    [Header("两个箱子")]
    public MovableBoxTask boxA;
    public MovableBoxTask boxB;

    [Header("老人引导流程")]
    public OldmanEscortController oldmanEscortController;

    [Header("环境恢复彩色")]
    [Tooltip("拖入场景里的 EnvironmentColorManager")]
    public EnvironmentColorManager environmentColorManager;

    [Header("完成状态")]
    public bool taskStarted = false;
    public bool allCompleted = false;


    // ======================================================
    // 老人第一次说完话后调用
    // ======================================================

    public void BeginBoxTask()
    {
        if (taskStarted)
            return;

        taskStarted = true;

        if (boxA != null)
            boxA.BeginTask();

        if (boxB != null)
            boxB.BeginTask();

        Debug.Log(
            "[BoxTaskManager] 两个箱子任务开始。"
        );
    }


    // ======================================================
    // 单个箱子完成时调用
    // ======================================================

    public void BoxCompleted(
        MovableBoxTask completedBox)
    {
        if (allCompleted)
            return;


        bool aDone =
            boxA != null &&
            boxA.placed;

        bool bDone =
            boxB != null &&
            boxB.placed;


        Debug.Log(
            "[BoxTaskManager] 箱子完成状态：A = " +
            aDone +
            " / B = " +
            bDone
        );


        // ==================================================
        // 两个箱子必须全部完成
        // ==================================================

        if (aDone && bDone)
        {
            allCompleted = true;

            HelpCompleted();
        }
    }


    // ======================================================
    // Keyboard 模式
    // 按 E 后自动完成两个箱子
    // ======================================================

    public void DesktopCompleteTask()
    {
        if (allCompleted)
            return;


        Debug.Log(
            "[BoxTaskManager] Keyboard：开始自动完成两个箱子。"
        );


        // 标记任务已经开始
        taskStarted = true;


        // ==================================================
        // Box A
        // 自动移动到自己的 Socket
        // ==================================================

        if (boxA != null)
        {
            if (!boxA.placed)
            {
                boxA.DesktopCompleteBox();
            }
        }
        else
        {
            Debug.LogWarning(
                "[BoxTaskManager] Box A 没有拖入。"
            );
        }


        // ==================================================
        // Box B
        // 自动移动到自己的 Socket
        // ==================================================

        if (boxB != null)
        {
            if (!boxB.placed)
            {
                boxB.DesktopCompleteBox();
            }
        }
        else
        {
            Debug.LogWarning(
                "[BoxTaskManager] Box B 没有拖入。"
            );
        }


        // ==================================================
        // 再检查一次两个箱子的状态
        // ==================================================

        bool aDone =
            boxA != null &&
            boxA.placed;

        bool bDone =
            boxB != null &&
            boxB.placed;


        Debug.Log(
            "[BoxTaskManager] Keyboard完成检查：A = " +
            aDone +
            " / B = " +
            bDone
        );


        // 正常情况下第二个箱子的 DesktopCompleteBox()
        // 已经通过 BoxCompleted() 触发 HelpCompleted()
        //
        // 这里是保险检查
        if (!allCompleted &&
            aDone &&
            bDone)
        {
            allCompleted = true;

            HelpCompleted();
        }
    }


    // ======================================================
    // 两个箱子全部完成
    // ======================================================

    private void HelpCompleted()
    {
        Debug.Log(
            "[BoxTaskManager] 两个箱子全部完成。"
        );


        // ==================================================
        // 0. 记录：玩家帮助过轮椅人物
        // ==================================================

        HelpRecord.HelpedOldman = true;

        Debug.Log(
            "[HelpRecord] HelpedOldman = TRUE"
        );


        // ==================================================
        // 1. Office 环境恢复彩色
        // ==================================================

        if (environmentColorManager != null)
        {
            environmentColorManager
                .RestoreAllEnvironment();

            Debug.Log(
                "[BoxTaskManager] Office 环境已经恢复彩色。"
            );
        }
        else
        {
            Debug.LogWarning(
                "[BoxTaskManager] EnvironmentColorManager 没有拖入。"
            );
        }


        // ==================================================
        // 2. 老人开始感谢 + 引导流程
        // ==================================================

        if (oldmanEscortController != null)
        {
            Debug.Log(
                "[BoxTaskManager] 开始老人感谢和引导流程。"
            );

            oldmanEscortController
                .StartEscortSequence();
        }
        else
        {
            Debug.LogWarning(
                "[BoxTaskManager] OldmanEscortController 没有拖入。"
            );
        }
    }
}