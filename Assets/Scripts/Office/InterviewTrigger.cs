using UnityEngine;

public class InterviewTrigger : MonoBehaviour
{
    [Header("玩家 Main Camera")]
    public Transform playerHead;

    [Header("面试官欢迎")]
    public InterviewWelcome interviewWelcome;

    [Header("最终结算")]
    public InterviewResultManager resultManager;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        if (other.transform == playerHead ||
            other.transform.IsChildOf(playerHead.root))
        {
            hasTriggered = true;

            // 面试官欢迎
            if (interviewWelcome != null)
            {
                interviewWelcome.PlayWelcome();
            }

            // 最终结算
            if (resultManager != null)
            {
                resultManager.ShowResult();
            }
        }
    }
}