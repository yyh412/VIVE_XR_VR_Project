using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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

    private float remainingTime;
    private bool finalMinuteStarted = false;
    private bool finished = false;


    public float RemainingTime
    {
        get { return remainingTime; }
    }

    public bool IsFinished
    {
        get { return finished; }
    }


    private void Awake()
    {
        // 已经存在一个计时器，就删除新场景重复的那个
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 整个 GameCountdown 对象跨场景存在
        DontDestroyOnLoad(gameObject);
    }


    private void Start()
    {
        // 只会在第一次创建这个计时器时执行
        remainingTime = totalTime;

        if (finalMinuteAudio != null)
        {
            finalMinuteAudio.Stop();
            finalMinuteAudio.playOnAwake = false;
        }

        UpdateDisplay();
    }


    private void Update()
    {
        if (finished)
            return;

        remainingTime -= Time.deltaTime;

        // 最后一分钟
        if (remainingTime <= 60f && !finalMinuteStarted)
        {
            finalMinuteStarted = true;

            if (finalMinuteAudio != null)
            {
                finalMinuteAudio.Play();
            }
        }

        // 到 0
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            finished = true;

            if (finalMinuteAudio != null)
            {
                finalMinuteAudio.Stop();
            }
        }

        UpdateDisplay();
    }


    private void UpdateDisplay()
    {
        if (countdownText == null)
            return;

        int totalSeconds = Mathf.CeilToInt(remainingTime);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        countdownText.text =
            string.Format("{0:00}:{1:00}", minutes, seconds);

        if (remainingTime <= 60f)
        {
            countdownText.color = warningColor;
        }
        else
        {
            countdownText.color = normalColor;
        }
    }


    // Office 场景如果有新的 TMP 倒计时文字
    public void SetCountdownText(TMP_Text newText)
    {
        countdownText = newText;
        UpdateDisplay();
    }


    public bool ArrivedOnTime()
    {
        return remainingTime > 0f;
    }


    public void StopCountdown()
    {
        finished = true;

        if (finalMinuteAudio != null)
        {
            finalMinuteAudio.Stop();
        }

        UpdateDisplay();
    }
}