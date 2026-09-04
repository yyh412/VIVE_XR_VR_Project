using UnityEngine;

public class InterviewResultManager : MonoBehaviour
{
    [Header("结算背景图片")]
    public GameObject successImage;
    public GameObject failedImage;

    [Header("面试官说完后，延迟显示结果")]
    public float resultDelay = 2f;


    // =====================================================
    // Interview Manager
    // =====================================================

    [Header("Interview Manager 结果")]
    public GameObject mansuitCheck;
    public GameObject mansuitCross;


    // =====================================================
    // Driver
    // =====================================================

    [Header("Driver 结果")]
    public GameObject driverCheck;
    public GameObject driverCross;


    // =====================================================
    // Man in Wheelchair
    // =====================================================

    [Header("Man in Wheelchair 结果")]
    public GameObject oldmanCheck;
    public GameObject oldmanCross;


    // =====================================================
    // 帮助人数
    // =====================================================

    [Header("帮助人数图片")]
    public GameObject count0;
    public GameObject count1;
    public GameObject count2;
    public GameObject count3;


    private bool resultStarted = false;
    private bool resultScheduled = false;
    private bool resultDisplayed = false;

    // 玩家进入终点的时候是否按时
    private bool arrivedOnTime = false;


    // =====================================================
    // Awake
    // =====================================================

    private void Awake()
    {
        HideAllResultUI();
    }


    // =====================================================
    // 一开始隐藏所有结算UI
    // =====================================================

    private void HideAllResultUI()
    {
        if (successImage != null)
            successImage.SetActive(false);

        if (failedImage != null)
            failedImage.SetActive(false);


        // Mansuit
        if (mansuitCheck != null)
            mansuitCheck.SetActive(false);

        if (mansuitCross != null)
            mansuitCross.SetActive(false);


        // Driver
        if (driverCheck != null)
            driverCheck.SetActive(false);

        if (driverCross != null)
            driverCross.SetActive(false);


        // Oldman
        if (oldmanCheck != null)
            oldmanCheck.SetActive(false);

        if (oldmanCross != null)
            oldmanCross.SetActive(false);


        // Count
        if (count0 != null)
            count0.SetActive(false);

        if (count1 != null)
            count1.SetActive(false);

        if (count2 != null)
            count2.SetActive(false);

        if (count3 != null)
            count3.SetActive(false);
    }


    // =====================================================
    // 玩家按时到达面试官
    // =====================================================

    public void BeginResult()
    {
        if (resultStarted)
            return;


        resultStarted = true;


        if (GameCountdown.Instance == null)
        {
            Debug.LogWarning(
                "[InterviewResultManager] 没有找到 GameCountdown.Instance"
            );

            return;
        }


        // 记录玩家到达时有没有剩余时间
        arrivedOnTime =
            GameCountdown.Instance.RemainingTime > 0f;


        float arrivalTime =
            GameCountdown.Instance.RemainingTime;


        // 到达以后立刻停止倒计时
        GameCountdown.Instance.StopCountdown();


        Debug.Log(
            "[InterviewResultManager] 玩家到达终点。" +
            "剩余时间：" + arrivalTime +
            "，是否按时：" + arrivedOnTime
        );
    }


    // =====================================================
    // 面试官说完话
    // =====================================================

    public void InterviewerFinishedSpeaking()
    {
        if (!resultStarted)
        {
            Debug.LogWarning(
                "[InterviewResultManager] 还没有 BeginResult，不能结算。"
            );

            return;
        }


        if (resultScheduled || resultDisplayed)
            return;


        resultScheduled = true;


        Invoke(
            nameof(DisplayResult),
            resultDelay
        );
    }


    // =====================================================
    // 时间归零
    // =====================================================

    public void TimeRanOut()
    {
        // 已经进入面试流程以后，不允许再判失败
        if (resultStarted || resultDisplayed)
            return;


        resultStarted = true;
        resultDisplayed = true;
        arrivedOnTime = false;


        CancelInvoke();


        // 显示 FAILED
        if (successImage != null)
            successImage.SetActive(false);

        if (failedImage != null)
            failedImage.SetActive(true);


        // 同时显示帮助结果
        ShowHelpResult();


        Debug.Log(
            "[InterviewResultManager] 时间归零，显示 FAILED。"
        );
    }


    // =====================================================
    // 正常到达后的结果
    // =====================================================

    private void DisplayResult()
    {
        if (resultDisplayed)
            return;


        resultDisplayed = true;


        if (arrivedOnTime)
        {
            if (successImage != null)
                successImage.SetActive(true);

            if (failedImage != null)
                failedImage.SetActive(false);


            Debug.Log(
                "[InterviewResultManager] 显示 SUCCESS"
            );
        }
        else
        {
            if (successImage != null)
                successImage.SetActive(false);

            if (failedImage != null)
                failedImage.SetActive(true);


            Debug.Log(
                "[InterviewResultManager] 显示 FAILED"
            );
        }


        // 和 SUCCESS / FAILED 同时出现
        ShowHelpResult();
    }


    // =====================================================
    // 显示帮助结果
    // =====================================================

    private void ShowHelpResult()
    {
        int helpedCount = 0;


        // =================================================
        // Interview Manager
        // =================================================

        if (HelpRecord.HelpedMansuit)
        {
            if (mansuitCheck != null)
                mansuitCheck.SetActive(true);

            if (mansuitCross != null)
                mansuitCross.SetActive(false);

            helpedCount++;
        }
        else
        {
            if (mansuitCheck != null)
                mansuitCheck.SetActive(false);

            if (mansuitCross != null)
                mansuitCross.SetActive(true);
        }


        // =================================================
        // Driver
        // =================================================

        if (HelpRecord.HelpedDriver)
        {
            if (driverCheck != null)
                driverCheck.SetActive(true);

            if (driverCross != null)
                driverCross.SetActive(false);

            helpedCount++;
        }
        else
        {
            if (driverCheck != null)
                driverCheck.SetActive(false);

            if (driverCross != null)
                driverCross.SetActive(true);
        }


        // =================================================
        // Man in Wheelchair
        // =================================================

        if (HelpRecord.HelpedOldman)
        {
            if (oldmanCheck != null)
                oldmanCheck.SetActive(true);

            if (oldmanCross != null)
                oldmanCross.SetActive(false);

            helpedCount++;
        }
        else
        {
            if (oldmanCheck != null)
                oldmanCheck.SetActive(false);

            if (oldmanCross != null)
                oldmanCross.SetActive(true);
        }


        // =================================================
        // 先关闭所有 Count
        // =================================================

        if (count0 != null)
            count0.SetActive(false);

        if (count1 != null)
            count1.SetActive(false);

        if (count2 != null)
            count2.SetActive(false);

        if (count3 != null)
            count3.SetActive(false);


        // =================================================
        // 根据帮助人数显示对应数字
        // =================================================

        switch (helpedCount)
        {
            case 0:

                if (count0 != null)
                    count0.SetActive(true);

                break;


            case 1:

                if (count1 != null)
                    count1.SetActive(true);

                break;


            case 2:

                if (count2 != null)
                    count2.SetActive(true);

                break;


            case 3:

                if (count3 != null)
                    count3.SetActive(true);

                break;
        }


        Debug.Log(
            "[InterviewResultManager] 帮助人数：" +
            helpedCount +
            " / 3"
        );
    }


    // =====================================================
    // 给其他脚本判断是否已经结束
    // =====================================================

    public bool IsFinished()
    {
        return resultDisplayed;
    }
}