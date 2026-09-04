using System.Collections;
using UnityEngine;

public class InterviewWelcome : MonoBehaviour
{
    [Header("对话框")]
    public GameObject speechBubble;

    [Header("欢迎语音")]
    public AudioSource audioSource;
    public AudioClip welcomeVoice;

    [Header("最终结算")]
    [Tooltip("拖入场景里的 InterviewResultManager")]
    public InterviewResultManager resultManager;

    private bool hasTriggered = false;


    // =====================================================
    // Start
    // =====================================================

    private void Start()
    {
        // 游戏开始时隐藏对话框
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }
    }


    // =====================================================
    // InterviewTrigger 调用
    // =====================================================

    public void PlayWelcome()
    {
        // 防止重复进入
        if (hasTriggered)
            return;

        hasTriggered = true;


        // =================================================
        // 玩家进入终点的一瞬间：
        //
        // 1. 记录是否按时
        // 2. 立刻停止倒计时
        // =================================================

        if (resultManager != null)
        {
            resultManager.BeginResult();
        }
        else
        {
            Debug.LogWarning(
                "[InterviewWelcome] Result Manager 没有拖入。"
            );
        }


        // =================================================
        // 然后开始面试官欢迎流程
        // =================================================

        StartCoroutine(
            WelcomeRoutine()
        );
    }


    // =====================================================
    // 面试官完整欢迎流程
    // =====================================================

    private IEnumerator WelcomeRoutine()
    {
        Debug.Log(
            "[InterviewWelcome] 开始欢迎流程。"
        );


        // =================================================
        // 1. 显示对话框
        // =================================================

        if (speechBubble != null)
        {
            speechBubble.SetActive(true);
        }


        // =================================================
        // 2. 播放欢迎语音
        // =================================================

        if (audioSource != null &&
            welcomeVoice != null)
        {
            audioSource.Stop();

            audioSource.clip =
                welcomeVoice;

            audioSource.Play();


            Debug.Log(
                "[InterviewWelcome] 面试官开始说话。"
            );


            // =================================================
            // 等语音真正播放结束
            // =================================================

            while (audioSource.isPlaying)
            {
                yield return null;
            }


            Debug.Log(
                "[InterviewWelcome] 面试官说完了。"
            );
        }
        else
        {
            Debug.LogWarning(
                "[InterviewWelcome] Audio Source 或 Welcome Voice 没有设置。"
            );


            // 没有语音的话，模拟说话2秒
            yield return new WaitForSeconds(2f);
        }


        // =================================================
        // 3. 说完关闭对话框
        // =================================================

        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }


        // =================================================
        // 4. 告诉 ResultManager：
        //
        // 面试官已经说完
        // ResultManager 自己再等待 2 秒
        // =================================================

        if (resultManager != null)
        {
            resultManager.InterviewerFinishedSpeaking();
        }
    }
}