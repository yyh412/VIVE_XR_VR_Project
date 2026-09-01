using UnityEngine;
using TMPro;

public class GameCountdown : MonoBehaviour
{
    [Header("倒计时文字")]
    public TMP_Text countdownText;

    [Header("总时间（秒）")]
    public float totalTime = 600f; // 10分钟

    [Header("最后一分钟音乐")]
    public AudioSource finalMinuteAudio;

    [Header("普通颜色")]
    public Color normalColor = Color.white;

    [Header("最后一分钟颜色")]
    public Color warningColor = Color.red;

    private float remainingTime;
    private bool finalMinuteStarted = false;
    private bool finished = false;

    void Start()
    {
        remainingTime = totalTime;

        if (finalMinuteAudio != null)
        {
            finalMinuteAudio.Stop();
            finalMinuteAudio.playOnAwake = false;
        }

        UpdateDisplay();
    }

    void Update()
    {
        if (finished)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 60f && !finalMinuteStarted)
        {
            finalMinuteStarted = true;

            if (finalMinuteAudio != null)
                finalMinuteAudio.Play();
        }

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            finished = true;

            if (finalMinuteAudio != null)
                finalMinuteAudio.Stop();
        }

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (countdownText == null)
            return;

        int totalSeconds = Mathf.CeilToInt(remainingTime);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        countdownText.text =
            string.Format("{0:00}:{1:00}", minutes, seconds);

        if (remainingTime <= 60f)
            countdownText.color = warningColor;
        else
            countdownText.color = normalColor;
    }
}