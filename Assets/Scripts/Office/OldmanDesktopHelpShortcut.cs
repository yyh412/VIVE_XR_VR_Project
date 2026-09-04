using UnityEngine;
using UnityEngine.InputSystem;

public class OldmanDesktopHelpShortcut : MonoBehaviour
{
    [Header("Desktop")]
    public bool desktopMode = true;

    [Header("角色")]
    public Transform player;
    public Transform oldman;

    [Header("距离")]
    public float helpDistance = 5f;

    [Header("提示")]
    public GameObject helpKeyHint;

    [Header("箱子任务")]
    public BoxTaskManager boxTaskManager;

    private bool helpUnlocked = false;
    private bool helpCompleted = false;

    void Start()
    {
        if (helpKeyHint != null)
            helpKeyHint.SetActive(false);
    }

    void Update()
    {
        if (!desktopMode)
            return;

        if (!helpUnlocked || helpCompleted)
            return;

        if (player == null || oldman == null)
            return;

        float distance = Vector3.Distance(player.position, oldman.position);
        bool inRange = distance <= helpDistance;

        if (helpKeyHint != null)
            helpKeyHint.SetActive(inRange);

        if (!inRange)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            CompleteHelp();
        }
    }

    public void ShowHelpHint()
    {
        if (!desktopMode || helpCompleted)
            return;

        helpUnlocked = true;
    }

    private void CompleteHelp()
    {
        if (helpCompleted)
            return;

        helpCompleted = true;

        if (helpKeyHint != null)
            helpKeyHint.SetActive(false);

        if (boxTaskManager != null)
        {
            boxTaskManager.DesktopCompleteTask();
        }
    }

    public void ResetDesktopHelp()
    {
        helpUnlocked = false;
        helpCompleted = false;

        if (helpKeyHint != null)
            helpKeyHint.SetActive(false);
    }
}