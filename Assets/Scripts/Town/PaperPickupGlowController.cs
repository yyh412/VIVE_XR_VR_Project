using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PaperPickupGlowController : MonoBehaviour
{
    [Header("这张纸的蓝色灯光")]
    public GameObject paperGlow;

    [Header("NPC站起来")]
    [Tooltip("只有需要触发NPC站起来的纸才填写")]
    public Animator npcAnimator;

    [Tooltip("Animator里的Trigger名字")]
    public string standUpTriggerName = "StandUp";

    [Tooltip("这张纸是否要触发NPC站起来")]
    public bool triggerStandUp = false;

    private XRGrabInteractable grabInteractable;
    private bool completed = false;
    private bool hasTriggeredStandUp = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnPickedUp);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnPickedUp);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    private void OnPickedUp(SelectEnterEventArgs args)
    {
        // 拿起来以后，关闭纸张蓝色提示
        if (paperGlow != null)
            paperGlow.SetActive(false);

        // 第二张纸：第一次拿起来时让NPC站起来
        if (
            triggerStandUp &&
            !hasTriggeredStandUp &&
            npcAnimator != null
        )
        {
            hasTriggeredStandUp = true;

            npcAnimator.SetTrigger(standUpTriggerName);

            Debug.Log("玩家拿起第二张纸，NPC开始Standing");
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        // 没完成的话，松手以后重新亮起
        if (!completed && paperGlow != null)
            paperGlow.SetActive(true);
    }

    public void MarkCompleted()
    {
        completed = true;

        if (paperGlow != null)
            paperGlow.SetActive(false);
    }
}