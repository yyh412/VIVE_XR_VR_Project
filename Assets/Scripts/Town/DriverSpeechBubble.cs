using UnityEngine;
using TMPro;
using System.Collections;

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

    [Header("第一次求助字幕位置")]
    public Vector3 helpOffset =
        new Vector3(0.75f, 1.8f, 0f);

    [Header("第二次 Keep pushing 字幕位置")]
    public Vector3 encourageOffset =
        new Vector3(1.0f, 1.7f, 0f);

    [Header("向玩家方向推出一点")]
    public float moveTowardPlayer = 0.15f;

    [Header("第一次求助文字")]
    [TextArea]
    public string helpMessage =
        "My wheel's stuck!\nCan you help me push?";

    [Header("第二次文字")]
    [TextArea]
    public string encourageMessage =
        "Keep pushing!";

    [Header("Keep pushing 显示时间")]
    public float encourageDisplayDuration = 1.5f;

    private Vector3 currentOffset;
    private Coroutine hideCoroutine;

    private void Start()
    {
        currentOffset = helpOffset;

        // 注意：
        // 不关闭 DriverSpeechBubble 自己
        // 只隐藏里面的视觉气泡
        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (driverRoot == null || playerCamera == null)
            return;

        // =========================================
        // 位置
        // =========================================
        Vector3 bubblePosition =
            driverRoot.position + currentOffset;

        // 稍微往玩家方向推出
        Vector3 towardPlayer =
            playerCamera.position - bubblePosition;

        if (towardPlayer.sqrMagnitude > 0.001f)
        {
            bubblePosition +=
                towardPlayer.normalized *
                moveTowardPlayer;
        }

        transform.position = bubblePosition;

        // =========================================
        // 始终朝向玩家
        // =========================================
        Vector3 direction =
            transform.position -
            playerCamera.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }
    }

    // =============================================
    // 第一次：HelpTrigger
    // =============================================
    public void ShowHelpMessage()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        currentOffset = helpOffset;

        if (dialogueText != null)
        {
            dialogueText.text = helpMessage;
        }

        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(true);
        }

        Debug.Log("显示第一次求助字幕");
    }

    // =============================================
    // 玩家到达 DriverPushPoint
    // =============================================
    public void HideBubble()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(false);
        }

        Debug.Log("隐藏 Driver 字幕");
    }

    // =============================================
    // 第二次：推满2秒
    // =============================================
    public void ShowEncourageMessage()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        currentOffset = encourageOffset;

        if (dialogueText != null)
        {
            dialogueText.text = encourageMessage;
        }

        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(true);
        }

        Debug.Log("显示第二次字幕：Keep pushing!");

        hideCoroutine =
            StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(
            encourageDisplayDuration
        );

        if (bubbleVisual != null)
        {
            bubbleVisual.SetActive(false);
        }

        hideCoroutine = null;
    }
}