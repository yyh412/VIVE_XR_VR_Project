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
        // 2. 播放 Thank You 语音
        // ==================================================

        if (
            audioSource != null &&
            thankYouClip != null
        )
        {
            audioSource.clip =
                thankYouClip;

            audioSource.Play();


            // 等语音真正播放完
            while (audioSource.isPlaying)
            {
                yield return null;
            }
        }
        else
        {
            // 没有语音时的备用等待时间
            yield return new WaitForSeconds(
                2f
            );
        }


        // ==================================================
        // 3. 语音结束 → 关闭文本框
        // ==================================================

        if (speechBubble != null)
        {
            speechBubble.SetActive(false);

            Debug.Log(
                "NPC文本框关闭"
            );
        }


        // ==================================================
        // 4. Thank You 完成
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