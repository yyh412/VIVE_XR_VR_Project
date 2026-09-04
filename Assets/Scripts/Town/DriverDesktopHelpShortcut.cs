using UnityEngine;
using UnityEngine.InputSystem;

public class DriverDesktopHelpShortcut : MonoBehaviour
{
    // ======================================================
    // 模式
    // ======================================================

    [Header("电脑模式")]
    [Tooltip("勾选 = 电脑模式；不勾选 = VR模式")]
    public bool desktopMode = true;


    // ======================================================
    // Driver
    // ======================================================

    [Header("Driver")]
    [Tooltip("拖入 driver3")]
    public Transform driver;


    // ======================================================
    // 玩家
    // ======================================================

    [Header("玩家")]
    [Tooltip("拖入 XR Origin (XR Rig)")]
    public Transform player;


    // ======================================================
    // 原来的 DriverPushPoint 流程
    // ======================================================

    [Header("Driver 原来的推车起点流程")]
    [Tooltip("拖入 DriverPushPoint 上的 DriverPushPointTrigger")]
    public DriverPushPointTrigger driverPushPointTrigger;


    // ======================================================
    // E 提示
    // ======================================================

    [Header("帮助提示 UI")]
    [Tooltip("只拖 DriverHelpKeyHint，不要拖 MansuitHelpKeyHint")]
    public GameObject helpKeyHint;


    // ======================================================
    // 距离
    // ======================================================

    [Header("触发距离")]
    [Tooltip("Driver恢复彩色后，玩家在这个距离以内才显示E")]
    public float helpDistance = 10f;


    // ======================================================
    // 调试
    // ======================================================

    [Header("调试")]
    public bool showDebugLog = true;


    // ======================================================
    // 内部状态
    // ======================================================

    private bool driverUnlocked = false;
    private bool helpStarted = false;


    // ======================================================
    // Start
    // ======================================================

    private void Start()
    {
        if (helpKeyHint != null)
        {
            helpKeyHint.SetActive(false);
        }

        driverUnlocked = false;
        helpStarted = false;
    }


    // ======================================================
    // Update
    // ======================================================

    private void Update()
    {
        if (!desktopMode)
            return;


        if (helpStarted)
        {
            HideHelpHint();
            return;
        }


        // 必须先看过Driver并恢复彩色
        if (!driverUnlocked)
        {
            HideHelpHint();
            return;
        }


        if (player == null || driver == null)
        {
            HideHelpHint();
            return;
        }


        float distance =
            Vector3.Distance(
                player.position,
                driver.position
            );


        // ==================================================
        // 10米以内
        // ==================================================

        if (distance <= helpDistance)
        {
            ShowHintInternal();


            if (
                helpKeyHint != null &&
                helpKeyHint.activeSelf &&
                Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame
            )
            {
                StartDriverHelp();
            }
        }

        // ==================================================
        // 超过10米
        // ==================================================

        else
        {
            HideHelpHint();
        }
    }


    // ======================================================
    // DriverGazeTarget调用
    // Driver恢复彩色以后解锁E
    // ======================================================

    public void ShowHelpHint()
    {
        if (!desktopMode)
            return;

        if (helpStarted)
            return;


        driverUnlocked = true;


        if (showDebugLog)
        {
            Debug.Log(
                "[Driver E] Driver恢复彩色 → 解锁Driver自己的E"
            );
        }
    }


    // ======================================================
    // 显示Driver自己的E
    // ======================================================

    private void ShowHintInternal()
    {
        if (
            helpKeyHint != null &&
            !helpKeyHint.activeSelf
        )
        {
            helpKeyHint.SetActive(true);


            if (showDebugLog)
            {
                Debug.Log(
                    "[Driver E] 进入10米范围 → 显示DriverHelpKeyHint"
                );
            }
        }
    }


    // ======================================================
    // 隐藏Driver自己的E
    // ======================================================

    private void HideHelpHint()
    {
        if (
            helpKeyHint != null &&
            helpKeyHint.activeSelf
        )
        {
            helpKeyHint.SetActive(false);


            if (showDebugLog)
            {
                Debug.Log(
                    "[Driver E] DriverHelpKeyHint隐藏"
                );
            }
        }
    }


    // ======================================================
    // 按E帮助Driver
    // ======================================================

    private void StartDriverHelp()
    {
        if (helpStarted)
            return;


        // ==================================================
        // 必须设置 DriverPushPointTrigger
        // ==================================================

        if (driverPushPointTrigger == null)
        {
            Debug.LogWarning(
                "DriverDesktopHelpShortcut：没有设置 DriverPushPointTrigger！"
            );

            return;
        }


        helpStarted = true;


        // 一按E立即隐藏Driver自己的提示
        HideHelpHint();


        if (showDebugLog)
        {
            Debug.Log(
                "[Driver E] 按E → 调用原来的DriverPushPoint流程"
            );
        }


        // ==================================================
        // ★关键
        //
        // 不再直接调用 CarPushInteraction
        //
        // 先走原来的：
        // Driver转回正确方向
        // → Push Trigger
        // → 开IK
        // → CarPushInteraction
        // ==================================================

        driverPushPointTrigger.DesktopStartPush();
    }


    // ======================================================
    // 调试 / 重置
    // ======================================================

    public void ResetDesktopHelp()
    {
        driverUnlocked = false;
        helpStarted = false;

        HideHelpHint();


        if (showDebugLog)
        {
            Debug.Log(
                "[Driver E] Desktop帮助状态已重置"
            );
        }
    }
}