using System.Collections;
using UnityEngine;

public class NPCTalkingAudio : MonoBehaviour
{
    [Header("NPC Animator")]
    public Animator npcAnimator;

    [Header("Talking状态名称")]
    public string talkingStateName = "Talking";

    [Header("语音")]
    public AudioSource audioSource;
    public AudioClip thankYouClip;

    [Header("对话框")]
    public GameObject speechBubble;

    [Header("帮助完成后环境恢复彩色")]
    [Tooltip("拖入 MansuitColorZone")]
    public EnvironmentColorZone mansuitColorZone;

    [Header("背景音乐")]
    [Tooltip("拖入 BackgroundMusic 上的 BackgroundMusicManager")]
    public BackgroundMusicManager backgroundMusicManager;

    private bool hasStartedTalking = false;


    private void Start()
    {
        // 游戏开始时隐藏文本框
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }
    }


    private void Update()
    {
        if (npcAnimator == null)
            return;


        AnimatorStateInfo state =
            npcAnimator.GetCurrentAnimatorStateInfo(0);


        // 第一次进入 Talking
        if (
            state.IsName(talkingStateName) &&
            !hasStartedTalking
        )
        {
            hasStartedTalking = true;

            StartCoroutine(
                TalkingSequence()
            );
        }
    }


    private IEnumerator TalkingSequence()
    {
        // ==================================================
        // 1. 显示文本框
        // ==================================================

        if (speechBubble != null)
        {
            speechBubble.SetActive(true);

            Debug.Log(
                "NPC文本框显示"
            );
        }


        // ==================================================
        // 2. Mansuit开始说话
        // → 背景音乐平滑降低
        // ==================================================

        if (backgroundMusicManager != null)
        {
            backgroundMusicManager.LowerMusic();

            Debug.Log(
                "Mansuit开始说话 → 背景音乐降低"
            );
        }
        else
        {
            Debug.LogWarning(
                "NPCTalkingAudio 没有设置 Background Music Manager"
            );
        }


        // ==================================================
        // 3. 播放 Thank You 语音
        // ==================================================

        if (
            audioSource != null &&
            thankYouClip != null
        )
        {
            audioSource.clip =
                thankYouClip;

            audioSource.Play();


            // 等语音真正播放结束
            while (audioSource.isPlaying)
            {
                yield return null;
            }
        }
        else
        {
            // 如果没有语音
            // 使用2秒备用等待时间
            yield return new WaitForSeconds(
                2f
            );
        }


        // ==================================================
        // 4. Thank You结束
        // → 关闭文本框
        // ==================================================

        if (speechBubble != null)
        {
            speechBubble.SetActive(false);

            Debug.Log(
                "NPC文本框关闭"
            );
        }


        // ==================================================
        // 5. Mansuit说话结束
        // → 背景音乐平滑恢复
        // ==================================================

        if (backgroundMusicManager != null)
        {
            backgroundMusicManager.RestoreMusic();

            Debug.Log(
                "Mansuit说话结束 → 背景音乐恢复"
            );
        }


        // ==================================================
        // 6. 帮助完成
        // → Mansuit周围环境开始恢复彩色
        // ==================================================

        if (mansuitColorZone != null)
        {
            mansuitColorZone.RestoreColorInZone();

            Debug.Log(
                "Mansuit Thank You结束 → 周围环境开始恢复彩色"
            );
        }
        else
        {
            Debug.LogWarning(
                "NPCTalkingAudio 没有设置 Mansuit Color Zone"
            );
        }
    }
}