using UnityEngine;

public class InterviewWelcome : MonoBehaviour
{
    [Header("对话框")]
    public GameObject speechBubble;

    [Header("欢迎语音")]
    public AudioSource audioSource;
    public AudioClip welcomeVoice;

    private bool hasTriggered = false;

    private void Start()
    {
        // 游戏开始时隐藏对话框
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }
    }

    public void PlayWelcome()
    {
        // 已经播放过就不再播放
        if (hasTriggered)
            return;

        hasTriggered = true;

        // 显示对话框
        if (speechBubble != null)
        {
            speechBubble.SetActive(true);
        }

        // 播放一次欢迎语音
        if (audioSource != null && welcomeVoice != null)
        {
            audioSource.PlayOneShot(welcomeVoice);
        }
    }
}