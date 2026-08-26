using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CarPushInteraction : MonoBehaviour
{
    [Header("流程控制")]
    public CarHelpManager carHelpManager;

    [Header("Driver")]
    public Animator driverAnimator;

    [Header("字幕气泡")]
    public DriverSpeechBubble speechBubble;

    [Header("推车语音")]
    public AudioSource driverAudioSource;

    [Header("2秒后的语音 Second")]
    public AudioClip secondVoiceClip;

    [Header("Second结束后的用力声音 Sigh")]
    public AudioClip sighVoiceClip;

    [Header("左右手的位置")]
    public Transform leftHand;
    public Transform rightHand;

    [Header("左右控制器")]
    public XRBaseController leftController;
    public XRBaseController rightController;

    [Header("手印物体")]
    public GameObject handPrint;

    [Header("手印材质")]
    public Material blueMaterial;
    public Material redMaterial;

    [Header("检测设置")]
    public float handDetectDistance = 0.30f;

    [Header("持续推多久后说 Keep pushing")]
    public float encourageDelay = 2.0f;

    [Header("持续震动设置")]
    [Range(0f, 1f)]
    public float hapticStrength = 0.35f;

    public float hapticPulseDuration = 0.08f;
    public float hapticInterval = 0.06f;

    private bool interactionEnabled = false;
    private bool handIsOnTarget = false;

    private bool encourageTriggered = false;
    private bool pushFinished = false;

    private float pushTimer = 0f;
    private float hapticTimer = 0f;

    private Renderer[] handPrintRenderers;

    private Coroutine sighCoroutine;


    private void Start()
    {
        if (handPrint != null)
        {
            handPrintRenderers =
                handPrint.GetComponentsInChildren<Renderer>(true);

            handPrint.SetActive(false);
        }

        ResetInternalState();
    }


    private void Update()
    {
        // 还没有到 DriverPushPoint
        if (!interactionEnabled)
            return;

        if (carHelpManager == null)
            return;

        // 只有 ReadyToPush / Pushing 才允许推
        if (!carHelpManager.IsStage(
                CarHelpManager.CarHelpStage.ReadyToPush) &&
            !carHelpManager.IsStage(
                CarHelpManager.CarHelpStage.Pushing))
        {
            return;
        }

        bool leftNear =
            IsHandNear(leftHand);

        bool rightNear =
            IsHandNear(rightHand);

        bool anyHandNear =
            leftNear || rightNear;


        // ==================================================
        // 玩家正在推
        // ==================================================
        if (anyHandNear)
        {
            // 第一次把手放上去
            if (!handIsOnTarget)
            {
                handIsOnTarget = true;

                pushTimer = 0f;
                hapticTimer = 0f;

                encourageTriggered = false;
                pushFinished = false;

                carHelpManager.SetStage(
                    CarHelpManager.CarHelpStage.Pushing
                );

                // 蓝色 → 红色
                SetHandPrintMaterial(
                    redMaterial
                );

                Debug.Log(
                    "开始推车 → 手印变红"
                );
            }

            // 持续震动
            UpdateHaptics(
                leftNear,
                rightNear
            );

            // 只有还没有进入 Second 阶段时
            // 才继续累计前面的2秒
            if (!encourageTriggered)
            {
                pushTimer +=
                    Time.deltaTime;
            }

            // ==================================================
            // 连续推满2秒
            // ==================================================
            if (pushTimer >= encourageDelay &&
                !encourageTriggered)
            {
                encourageTriggered = true;

                DriverEncourage();
            }
        }

        // ==================================================
        // 玩家手移开
        // ==================================================
        else
        {
            if (handIsOnTarget &&
                !pushFinished)
            {
                ResetPush();
            }
        }
    }


    // ======================================================
    // 2秒后：
    // Keep pushing字幕 + Second
    // ======================================================
    private void DriverEncourage()
    {
        Debug.Log(
            "Driver: Keep pushing!"
        );

        // 显示字幕
        if (speechBubble != null)
        {
            speechBubble.ShowEncourageMessage();
        }

        // 播放 Second
        if (driverAudioSource != null &&
            secondVoiceClip != null)
        {
            driverAudioSource.Stop();

            driverAudioSource.clip =
                secondVoiceClip;

            driverAudioSource.Play();

            Debug.Log(
                "播放 Second：Keep pushing!"
            );

            if (sighCoroutine != null)
            {
                StopCoroutine(
                    sighCoroutine
                );
            }

            sighCoroutine =
                StartCoroutine(
                    PlaySighAfterSecond()
                );
        }
        else
        {
            Debug.LogWarning(
                "Second AudioSource 或 AudioClip 没有设置！"
            );
        }
    }


    // ======================================================
    // 等 Second 完整播放结束
    // → 播放 Sigh
    // → 立即进入 Push Stop
    // ======================================================
    private IEnumerator PlaySighAfterSecond()
    {
        // 等 Second 播完
        while (driverAudioSource != null &&
               driverAudioSource.isPlaying)
        {
            // 如果玩家这时候松手
            // ResetPush 会停止这个 Coroutine
            yield return null;
        }

        // 玩家已经松手
        if (!handIsOnTarget)
        {
            sighCoroutine = null;
            yield break;
        }

        // 已经进入成功流程
        if (pushFinished)
        {
            sighCoroutine = null;
            yield break;
        }

        // =========================================
        // 播放 Sigh
        // =========================================
        if (driverAudioSource != null &&
            sighVoiceClip != null)
        {
            driverAudioSource.clip =
                sighVoiceClip;

            driverAudioSource.Play();

            Debug.Log(
                "Second结束 → 开始播放 Sigh"
            );
        }
        else
        {
            Debug.LogWarning(
                "Sigh AudioSource 或 AudioClip 没有设置！"
            );
        }

        // =========================================
        // Sigh 开始的同时进入 Push Stop
        // =========================================
        pushFinished = true;

        PushSucceeded();

        sighCoroutine = null;
    }


    // ======================================================
    // 推车成功 → Push Stop
    // ======================================================
    private void PushSucceeded()
    {
        Debug.Log(
            "Sigh开始 → Driver切换到 Push Stop"
        );

        // 注意：
        // 这里不能 driverAudioSource.Stop()
        // 否则 Sigh 刚开始就会被关掉

        interactionEnabled = false;

        // 隐藏手印
        if (handPrint != null)
        {
            handPrint.SetActive(false);
        }

        // Keep pushing 字幕如果还在
        // 这里关闭
        if (speechBubble != null)
        {
            speechBubble.HideBubble();
        }

        // Push_InPlace → Push Stop
        if (driverAnimator != null)
        {
            driverAnimator.SetTrigger(
                "StopPush"
            );

            Debug.Log(
                "StopPush Trigger 已发送"
            );
        }
        else
        {
            Debug.LogWarning(
                "Driver Animator 没有设置！"
            );
        }

        // 后续汽车 / Driver Root Motion / 四轮转动
        // 继续交给 PushStopCarSync

        if (carHelpManager != null)
        {
            carHelpManager.SetStage(
                CarHelpManager.CarHelpStage.Finished
            );
        }
    }


    // ======================================================
    // 持续震动
    // ======================================================
    private void UpdateHaptics(
        bool leftNear,
        bool rightNear)
    {
        hapticTimer -=
            Time.deltaTime;

        if (hapticTimer > 0f)
            return;

        if (leftNear &&
            leftController != null)
        {
            leftController.SendHapticImpulse(
                hapticStrength,
                hapticPulseDuration
            );
        }

        if (rightNear &&
            rightController != null)
        {
            rightController.SendHapticImpulse(
                hapticStrength,
                hapticPulseDuration
            );
        }

        hapticTimer =
            hapticInterval;
    }


    // ======================================================
    // 检测手是否靠近 PushHandTarget
    // ======================================================
    private bool IsHandNear(
        Transform hand)
    {
        if (hand == null)
            return false;

        float distance =
            Vector3.Distance(
                hand.position,
                transform.position
            );

        return distance <=
               handDetectDistance;
    }


    // ======================================================
    // 中途松手
    // ======================================================
    private void ResetPush()
    {
        handIsOnTarget = false;

        pushTimer = 0f;
        hapticTimer = 0f;

        encourageTriggered = false;
        pushFinished = false;

        // 停止 Second / Sigh
        if (driverAudioSource != null)
        {
            driverAudioSource.Stop();
        }

        if (sighCoroutine != null)
        {
            StopCoroutine(
                sighCoroutine
            );

            sighCoroutine = null;
        }

        // 红色 → 蓝色
        SetHandPrintMaterial(
            blueMaterial
        );

        // 隐藏 Keep pushing
        if (speechBubble != null)
        {
            speechBubble.HideBubble();
        }

        Debug.Log(
            "手移开 → 停止语音 → 恢复蓝色 → 重新从2秒开始"
        );
    }


    // ======================================================
    // Driver Talking结束
    // → 蓝色手印出现
    // ======================================================
    public void ShowHandPrint()
    {
        if (handPrint == null)
            return;

        handPrint.SetActive(true);

        SetHandPrintMaterial(
            blueMaterial
        );

        Debug.Log(
            "Talking结束 → 蓝色HandPrint出现"
        );
    }


    // ======================================================
    // 玩家到 DriverPushPoint
    // ======================================================
    public void EnablePushInteraction()
    {
        interactionEnabled = true;

        ResetInternalState();

        SetHandPrintMaterial(
            blueMaterial
        );

        Debug.Log(
            "到达DriverPushPoint → 开始检测手"
        );
    }


    // ======================================================
    // 内部状态重置
    // ======================================================
    private void ResetInternalState()
    {
        handIsOnTarget = false;

        encourageTriggered = false;
        pushFinished = false;

        pushTimer = 0f;
        hapticTimer = 0f;

        if (sighCoroutine != null)
        {
            StopCoroutine(
                sighCoroutine
            );

            sighCoroutine = null;
        }
    }


    // ======================================================
    // 蓝 / 红材质切换
    // ======================================================
    private void SetHandPrintMaterial(
        Material material)
    {
        if (material == null ||
            handPrintRenderers == null)
        {
            return;
        }

        foreach (
            Renderer renderer
            in handPrintRenderers)
        {
            renderer.material =
                material;
        }
    }
}