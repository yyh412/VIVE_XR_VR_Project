using UnityEngine;

public class BoxTaskManager : MonoBehaviour
{
    [Header("两个箱子")]
    public MovableBoxTask boxA;
    public MovableBoxTask boxB;

    [Header("老人引导流程")]
    public OldmanEscortController oldmanEscortController;

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

        Debug.Log("[BoxTaskManager] 两个箱子任务开始。");
    }


    // ======================================================
    // 单个箱子完成时调用
    // ======================================================

    public void BoxCompleted(MovableBoxTask completedBox)
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


        // 两个箱子必须全部完成
        if (aDone && bDone)
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
            "[BoxTaskManager] 两个箱子全部完成，开始老人引导流程。"
        );


        if (oldmanEscortController != null)
        {
            oldmanEscortController.StartEscortSequence();
        }
        else
        {
            Debug.LogWarning(
                "[BoxTaskManager] OldmanEscortController 没有拖入。"
            );
        }
    }
}