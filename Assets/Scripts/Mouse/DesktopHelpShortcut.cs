using UnityEngine;
using UnityEngine.InputSystem;

public class DesktopHelpShortcut : MonoBehaviour
{
    [Header("电脑模式")]
    public bool desktopMode = true;

    [Header("文件任务")]
    [Tooltip("拖入 PaperDropZone 上的 BriefcasePaperDropZone")]
    public BriefcasePaperDropZone briefcasePaperDropZone;

    [Header("帮助提示UI")]
    [Tooltip("只拖 HelpKeyHint")]
    public GameObject helpKeyHint;

    [Header("玩家")]
    [Tooltip("拖入 XR Origin (XR Rig)")]
    public Transform player;

    [Header("Mansuit")]
    [Tooltip("拖入 mansuit@Walking")]
    public Transform mansuit;

    [Header("触发距离")]
    [Tooltip("玩家距离 Mansuit 小于这个距离时，才允许按 E")]
    public float helpDistance = 5f;

    [Header("调试")]
    public bool showDebugLog = true;


    private bool mansuitUnlocked = false;
    private bool helpCompleted = false;


    private void Start()
    {
        if (helpKeyHint != null)
        {
            helpKeyHint.SetActive(false);
        }
    }


    private void Update()
    {
        if (!desktopMode)
            return;

        if (helpCompleted)
        {
            HideHelpHint();
            return;
        }


        // Mansuit还没有通过注视恢复彩色
        if (!mansuitUnlocked)
        {
            HideHelpHint();
            return;
        }


        if (player == null || mansuit == null)
        {
            HideHelpHint();
            return;
        }


        // ==================================================
        // 检查玩家和 Mansuit 的距离
        // ==================================================

        float distance =
            Vector3.Distance(
                player.position,
                mansuit.position
            );


        // 5米以内
        if (distance <= helpDistance)
        {
            ShowHintInternal();

            // 只有在范围内 E 才有效
            if (
                Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame
            )
            {
                CompleteHelp();
            }
        }

        // 超过5米
        else
        {
            HideHelpHint();
        }
    }


    // ======================================================
    // Mansuit恢复彩色以后调用
    // ======================================================

    public void ShowHelpHint()
    {
        if (helpCompleted)
            return;

        mansuitUnlocked = true;

        if (showDebugLog)
        {
            Debug.Log(
                "[Mansuit] 已恢复彩色 → 解锁 E 帮助"
            );
        }
    }


    // ======================================================
    // 显示提示
    // ======================================================

    private void ShowHintInternal()
    {
        if (helpKeyHint != null &&
            !helpKeyHint.activeSelf)
        {
            helpKeyHint.SetActive(true);
        }
    }


    // ======================================================
    // 隐藏提示
    // ======================================================

    private void HideHelpHint()
    {
        if (helpKeyHint != null &&
            helpKeyHint.activeSelf)
        {
            helpKeyHint.SetActive(false);
        }
    }


    // ======================================================
    // 按 E 完成帮助
    // ======================================================

    private void CompleteHelp()
    {
        if (briefcasePaperDropZone == null)
        {
            Debug.LogWarning(
                "DesktopHelpShortcut：没有设置 BriefcasePaperDropZone！"
            );

            return;
        }


        helpCompleted = true;

        HideHelpHint();


        briefcasePaperDropZone.DesktopCompleteAllPapers();


        if (showDebugLog)
        {
            Debug.Log(
                "[Mansuit] 按 E → 完成帮助"
            );
        }
    }
}