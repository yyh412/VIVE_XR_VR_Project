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

    private bool hasStartedTalking = false;

    private void Start()
    {
        // 游戏开始时隐藏文本框
        if (speechBubble != null)
            speechBubble.SetActive(false);
    }

    private void Update()
    {
        if (npcAnimator == null)
            return;

        AnimatorStateInfo state =
            npcAnimator.GetCurrentAnimatorStateInfo(0);

        // 第一次进入 Talking
        if (state.IsName(talkingStateName) && !hasStartedTalking)
        {
            hasStartedTalking = true;
            StartCoroutine(TalkingSequence());
        }
    }

    private IEnumerator TalkingSequence()
    {
        // 1. 显示文本框
        if (speechBubble != null)
        {
            speechBubble.SetActive(true);
            Debug.Log("NPC文本框显示");
        }

        // 2. 播放语音
        if (audioSource != null && thankYouClip != null)
        {
            audioSource.clip = thankYouClip;
            audioSource.Play();

            // 等语音说完
            yield return new WaitForSeconds(thankYouClip.length);
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        // 3. 关闭文本框
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
            Debug.Log("NPC文本框关闭");
        }
    }
}