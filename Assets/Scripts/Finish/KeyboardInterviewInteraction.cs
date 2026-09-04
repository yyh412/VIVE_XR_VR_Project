using System.Collections;
using UnityEngine;
using TMPro;

public class KeyboardInterviewInteraction : MonoBehaviour
{
    [Header("玩家 Main Camera")]
    [Tooltip("拖入 XR Origin 下面的 Main Camera")]
    public Transform playerHead;

    [Header("交互距离")]
    public float interactionDistance = 5f;

    [Header("E 提示")]
    [Tooltip("拖入单独的 [E] Interview UI")]
    public GameObject interviewPrompt;

    [Header("面试官字幕气泡")]
    public GameObject speechBubble;

    [Header("字幕文字")]
    public TMP_Text speechText;

    [Header("面试官语音")]
    public AudioSource audioSource;

    [Tooltip("之前帮助过他时播放")]
    public AudioClip helpedBeforeClip;

    [Tooltip("之前没有帮助过他时播放")]
    public AudioClip normalWelcomeClip;

    [Header("最终结算")]
    public InterviewResultManager resultManager;

    [Header("语音结束后额外等待")]
    public float extraWaitAfterSpeech = 0.3f;


    private bool interactionStarted = false;


    // =====================================================
    // Start
    // =====================================================

    private void Start()
    {
        if (interviewPrompt != null)
            interviewPrompt.SetActive(false);

        if (speechBubble != null)
            speechBubble.SetActive(false);
    }


    // =====================================================
    // Update
    // =====================================================

    private void Update()
    {
        // 已经开始面试后，不再重复
        if (interactionStarted)
        {
            if (interviewPrompt != null)
                interviewPrompt.SetActive(false);

            return;
        }


        // 如果游戏已经失败 / 已经结算
        if (resultManager != null &&
            resultManager.IsFinished())
        {
            if (interviewPrompt != null)
                interviewPrompt.SetActive(false);

            return;
        }


        if (playerHead == null)
            return;


        float distance =
            Vector3.Distance(
                playerHead.position,
                transform.position
            );


        bool closeEnough =
            distance <= interactionDistance;


        // =================================================
        // 5 米内显示 [E] Interview
        // =================================================

        if (interviewPrompt != null)
        {
            interviewPrompt.SetActive(
                closeEnough
            );
        }


        // =================================================
        // 按 E 开始面试
        // =================================================

        if (closeEnough &&
            Input.GetKeyDown(KeyCode.E))
        {
            StartInterview();
        }
    }


    // =====================================================
    // 开始面试
    // =====================================================

    private void StartInterview()
    {
        if (interactionStarted)
            return;


        // 如果已经失败
        if (resultManager != null &&
            resultManager.IsFinished())
        {
            return;
        }


        interactionStarted = true;


        if (interviewPrompt != null)
            interviewPrompt.SetActive(false);


        // 玩家已经按时到达
        // 立刻停止倒计时
        if (resultManager != null)
        {
            resultManager.BeginResult();
        }


        StartCoroutine(
            InterviewRoutine()
        );
    }


    // =====================================================
    // 面试官说话
    // =====================================================

    private IEnumerator InterviewRoutine()
    {
        string dialogue = "";

        AudioClip clipToPlay = null;


        // =================================================
        // 如果之前帮助过这个掉文件的人
        // =================================================

        if (HelpRecord.HelpedMansuit)
        {
            dialogue =
                "Thanks again for helping me earlier.";

            clipToPlay =
                helpedBeforeClip;
        }
        else
        {
            dialogue =
                "Welcome to the interview.";

            clipToPlay =
                normalWelcomeClip;
        }


        // =================================================
        // 显示字幕
        // =================================================

        if (speechText != null)
        {
            speechText.text =
                dialogue;
        }


        if (speechBubble != null)
        {
            speechBubble.SetActive(true);
        }


        Debug.Log(
            "[Interview] " +
            dialogue
        );


        // =================================================
        // 播放语音
        // =================================================

        if (audioSource != null &&
            clipToPlay != null)
        {
            audioSource.Stop();

            audioSource.clip =
                clipToPlay;

            audioSource.Play();


            yield return new WaitWhile(
                () => audioSource.isPlaying
            );
        }
        else
        {
            // 没有语音时默认显示2秒
            yield return new WaitForSeconds(
                2f
            );
        }


        // 额外等待一点
        if (extraWaitAfterSpeech > 0f)
        {
            yield return new WaitForSeconds(
                extraWaitAfterSpeech
            );
        }


        // =================================================
        // 隐藏字幕
        // =================================================

        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }


        // =================================================
        // 面试官说完
        // 开始显示 SUCCESS
        // =================================================

        if (resultManager != null)
        {
            resultManager.InterviewerFinishedSpeaking();
        }
        else
        {
            Debug.LogWarning(
                "[KeyboardInterviewInteraction] " +
                "没有设置 InterviewResultManager。"
            );
        }
    }
}