using UnityEngine;
using TMPro;

public class GameCountdown : MonoBehaviour
{
    public static GameCountdown Instance;

    [Header("倒计时文字")]
    public TMP_Text countdownText;

    [Header("总时间（秒）")]
    public float totalTime = 420f; // 7分钟

    [Header("最后一分钟音乐")]
    public AudioSource finalMinuteAudio;

    [Header("普通颜色")]
    public Color normalColor = Color.white;

    [Header("最后一分钟颜色")]
    public Color warningColor = Color.red;

    [Header("是否开始倒计时")]
    public bool countdownStarted = true;


    // =====================================================
    // 跨场景保存的数据
    // =====================================================

    private static float sharedRemainingTime;

    private static bool timerInitialized = false;

    private static bool timerFinished = false;

    private static bool finalMinuteStarted = false;


    // =====================================================
    // 给其他脚本读取
    // =====================================================

    public float RemainingTime
    {
        get { return sharedRemainingTime; }
    }

    public bool IsFinished
    {
        get { return timerFinished; }
    }


    // =====================================================
    // Awake
    // =====================================================

    private void Awake()
    {
        // 当前场景里的 GameCountdown
        Instance = this;
    }


    // =====================================================
    // Start
    // =====================================================

    private void Start()
    {
        // 只有第一次进入游戏时才从7分钟开始
        if (!timerInitialized)
        {
            sharedRemainingTime = totalTime;

            timerInitialized = true;

            timerFinished = false;

            finalMinuteStarted = false;
        }

        // 当前场景自己的音乐
        if (finalMinuteAudio != null)
        {
            finalMinuteAudio.Stop();

            finalMinuteAudio.playOnAwake = false;
        }

        // 如果切换到 Office 时已经进入最后一分钟
        // Office 的 AudioSource 继续播放警告音乐
        if (sharedRemainingTime <= 60f &&
            sharedRemainingTime > 0f &&
            !timerFinished)
        {
            finalMinuteStarted = true;

            if (finalMinuteAudio != null)
            {
                finalMinuteAudio.Play();
            }
        }

        UpdateDisplay();
    }


    // =====================================================
    // Update
    // =====================================================

    private void Update()
    {
        if (!countdownStarted)
            return;

        if (timerFinished)
            return;


        sharedRemainingTime -= Time.deltaTime;


        // =================================================
        // 最后一分钟
        // =================================================

        if (sharedRemainingTime <= 60f &&
            !finalMinuteStarted)
        {
            finalMinuteStarted = true;

            if (finalMinuteAudio != null)
            {
                finalMinuteAudio.Play();
            }
        }


        // =================================================
        // 时间归零
        // =================================================

        if (sharedRemainingTime <= 0f)
        {
            sharedRemainingTime = 0f;

            timerFinished = true;

            if (finalMinuteAudio != null)
            {
                finalMinuteAudio.Stop();
            }
        }


        UpdateDisplay();
    }


    // =====================================================
    // 更新文字
    // =====================================================

    private void UpdateDisplay()
    {
        if (countdownText == null)
            return;


        int totalSeconds =
            Mathf.CeilToInt(sharedRemainingTime);

        int minutes =
            totalSeconds / 60;

        int seconds =
            totalSeconds % 60;


        countdownText.text =
            string.Format(
                "{0:00}:{1:00}",
                minutes,
                seconds
            );


        // 最后一分钟改变颜色
        if (sharedRemainingTime <= 60f)
        {
            countdownText.color =
                warningColor;
        }
        else
        {
            countdownText.color =
                normalColor;
        }
    }


    // =====================================================
    // 判断是否按时到达
    // =====================================================

    public bool ArrivedOnTime()
    {
        return sharedRemainingTime > 0f;
    }


    // =====================================================
    // 最终结算：停止倒计时
    // =====================================================

    public void StopCountdown()
    {
        timerFinished = true;

        if (finalMinuteAudio != null)
        {
            finalMinuteAudio.Stop();
        }

        UpdateDisplay();
    }


    // =====================================================
    // 暂停
    // =====================================================

    public void PauseCountdown()
    {
        countdownStarted = false;
    }


    // =====================================================
    // 继续
    // =====================================================

    public void ResumeCountdown()
    {
        if (!timerFinished)
        {
            countdownStarted = true;
        }
    }


    // =====================================================
    // 开始新游戏时重置
    // =====================================================

    public static void ResetCountdown()
    {
        sharedRemainingTime = 0f;

        timerInitialized = false;

        timerFinished = false;

        finalMinuteStarted = false;
    }
}