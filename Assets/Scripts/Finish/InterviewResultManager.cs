using UnityEngine;

public class InterviewResultManager : MonoBehaviour
{
    [Header("结算图片")]
    public GameObject successImage;
    public GameObject failedImage;

    [Header("欢迎后延迟显示结果")]
    public float resultDelay = 2f;

    private bool hasShownResult = false;

    private void Start()
    {
        if (successImage != null)
            successImage.SetActive(false);

        if (failedImage != null)
            failedImage.SetActive(false);
    }

    public void ShowResult()
    {
        if (hasShownResult)
            return;

        hasShownResult = true;

        Invoke(nameof(CheckResult), resultDelay);
    }

    private void CheckResult()
    {
        if (GameCountdown.Instance == null)
        {
            Debug.LogWarning("没有找到 GameCountdown.Instance");
            return;
        }

        // 先判断玩家是否按时到达
        bool arrivedOnTime =
            GameCountdown.Instance.RemainingTime > 0f;

        // 结算后停止倒计时
        GameCountdown.Instance.StopCountdown();

        // 显示成功 / 失败
        if (arrivedOnTime)
        {
            if (successImage != null)
                successImage.SetActive(true);

            if (failedImage != null)
                failedImage.SetActive(false);
        }
        else
        {
            if (successImage != null)
                successImage.SetActive(false);

            if (failedImage != null)
                failedImage.SetActive(true);
        }
    }
}