using System.Collections;
using UnityEngine;
using TMPro;

public class DriverSpeechBubble : MonoBehaviour
{
    [Header("玩家相机")]
    public Transform playerCamera;

    [Header("Driver 根物体")]
    public Transform driverRoot;

    [Header("真正显示/隐藏的气泡物体")]
    public GameObject bubbleVisual;

    [Header("字幕文字")]
    public TMP_Text dialogueText;


    // ======================================================
    // Driver Audio
    // ======================================================

    [Header("Driver语音 Audio Source")]
    public AudioSource driverAudioSource;

    [Header("第一次求助语音 First")]
    public AudioClip firstVoiceClip;


    // ======================================================
    // 四次字幕位置
    // ======================================================

    [Header("第一次求助字幕位置")]
    public Vector3 helpOffset =
        new Vector3(0.75f, 1.8f, 0f);

    [Header("第二次 Keep pushing 字幕位置")]
    public Vector3 encourageOffset =
        new Vector3(1.0f, 1.7f, 0f);

    [Header("第三次感谢字幕位置")]
    public Vector3 thankOffset =
        new Vector3(0.9f, 1.8f, 0f);

    [Header("第四次 Good luck 字幕位置")]
    public Vector3 goodLuckOffset =
        new Vector3(0.9f, 1.8f, 0f);


    // ======================================================
    // 字幕朝向
    // ======================================================

    [Header("向玩家方向推出一点")]
    public float moveTowardPlayer = 0.15f;


    // ======================================================
    // 四次字幕文字
    // ======================================================

    [Header("第一次求助文字")]
    [TextArea]
    public string helpMessage =
        "My wheel's stuck!\nCan you help me push?";

    [Header("第二次鼓励文字")]
    [TextArea]
    public string encourageMessage =
        "Keep pushing!";

    [Header("第三次感谢文字")]
    [TextArea]
    public string thankMessage =
        "Thanks!\nHop in, I'll give you a ride.";

    [Header("第四次告别文字")]
    [TextArea]
    public string goodLuckMessage =
        "Good luck!";


    // ======================================================
    // Keep pushing 字幕显示时间
    // ======================================================

    [Header("Keep pushing 显示时间")]
    public float encourageDisplayDuration = 1.5f;


    // ======================================================
    // 当前状态
    // ======================================================

    private Vector3 currentOffset;
    private Coroutine hideCoroutine;


    // ======================================================
    // Start
    // ======================================================

    private void Start()
    {
        currentOffset = helpOffset;

        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(false);
        }
    }


    // ======================================================
    // 字幕跟随 Driver + 面向玩家
    // ======================================================

    private void LateUpdate()
    {
        if (driverRoot == null)
            return;

        if (playerCamera == null)
            return;


        // 1. 跟随 Driver
        Vector3 bubblePosition =
            driverRoot.position +
            currentOffset;


        // 2. 稍微朝玩家推出
        Vector3 towardPlayer =
            playerCamera.position -
            bubblePosition;

        if (towardPlayer.sqrMagnitude > 0.001f)
        {
            bubblePosition +=
                towardPlayer.normalized *
                moveTowardPlayer;
        }


        transform.position =
            bubblePosition;


        // 3. 始终面向玩家
        Vector3 direction =
            transform.position -
            playerCamera.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    direction
                );
        }
    }


    // ======================================================
    // 第一次：求助
    // ======================================================

    public void ShowHelpMessage()
    {
        StopHideCoroutine();

        currentOffset =
            helpOffset;


        if (dialogueText != null)
        {
            dialogueText.text =
                helpMessage;
        }


        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(true);
        }


        if (driverAudioSource != null &&
            firstVoiceClip != null)
        {
            driverAudioSource.Stop();

            driverAudioSource.clip =
                firstVoiceClip;

            driverAudioSource.Play();


            Debug.Log(
                "播放 First：My wheel's stuck! Can you help me push?"
            );
        }
        else
        {
            Debug.LogWarning(
                "First语音没有设置 AudioSource 或 AudioClip"
            );
        }


        Debug.Log(
            "显示第一次求助字幕"
        );
    }


    // ======================================================
    // 第二次：Keep pushing
    // ======================================================

    public void ShowEncourageMessage()
    {
        StopHideCoroutine();

        currentOffset =
            encourageOffset;


        if (dialogueText != null)
        {
            dialogueText.text =
                encourageMessage;
        }


        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(true);
        }


        Debug.Log(
            "显示第二次字幕：Keep pushing!"
        );


        hideCoroutine =
            StartCoroutine(
                HideAfterDelay(
                    encourageDisplayDuration
                )
            );
    }


    // ======================================================
    // 第三次：感谢 + 邀请上车
    // ======================================================

    public void ShowThankMessage()
    {
        StopHideCoroutine();

        currentOffset =
            thankOffset;


        if (dialogueText != null)
        {
            dialogueText.text =
                thankMessage;
        }


        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(true);
        }


        Debug.Log(
            "显示感谢字幕：Thanks! Hop in, I'll give you a ride."
        );
    }


    // ======================================================
    // 第四次：玩家下车后 Good luck
    // ======================================================

    public void ShowGoodLuckMessage()
    {
        StopHideCoroutine();

        currentOffset =
            goodLuckOffset;


        if (dialogueText != null)
        {
            dialogueText.text =
                goodLuckMessage;
        }


        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(true);
        }


        Debug.Log(
            "显示告别字幕：Good luck!"
        );
    }


    // ======================================================
    // 隐藏字幕
    // ======================================================

    public void HideBubble()
    {
        StopHideCoroutine();

        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(false);
        }
    }


    // ======================================================
    // 自动隐藏
    // ======================================================

    private IEnumerator HideAfterDelay(
        float duration)
    {
        yield return
            new WaitForSeconds(duration);


        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(false);
        }


        hideCoroutine = null;
    }


    // ======================================================
    // 停止之前的隐藏 Coroutine
    // ======================================================

    private void StopHideCoroutine()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(
                hideCoroutine
            );

            hideCoroutine = null;
        }
    }
}