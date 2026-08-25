using UnityEngine;
using System.Collections;

public class DriverHelpTrigger : MonoBehaviour
{
    [Header("流程控制")]
    public CarHelpManager carHelpManager;

    [Header("Driver")]
    public Animator driverAnimator;
    public Transform driverTransform;

    [Header("Player")]
    public Transform playerTransform;

    [Header("推车交互")]
    public CarPushInteraction carPushInteraction;

    [Header("字幕气泡")]
    public DriverSpeechBubble speechBubble;

    [Header("转身设置")]
    public float turnDuration = 0.6f;

    [Header("Talking 动画时间")]
    public float talkingDuration = 3.93f;

    private void OnTriggerEnter(Collider other)
    {
        if (carHelpManager == null)
            return;

        if (!carHelpManager.IsStage(
            CarHelpManager.CarHelpStage.WaitingForHelp))
        {
            return;
        }

        Debug.Log("正确进入 HelpTrigger: " + other.name);

        carHelpManager.SetStage(
            CarHelpManager.CarHelpStage.Talking
        );

        StartCoroutine(TurnAndTalk());
    }

    private IEnumerator TurnAndTalk()
    {
        if (driverTransform == null ||
            playerTransform == null)
        {
            yield break;
        }

        // 1. Driver 转向玩家
        Vector3 direction =
            playerTransform.position -
            driverTransform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion startRotation =
                driverTransform.rotation;

            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            float time = 0f;

            while (time < turnDuration)
            {
                time += Time.deltaTime;

                float t =
                    Mathf.Clamp01(
                        time / turnDuration
                    );

                driverTransform.rotation =
                    Quaternion.Slerp(
                        startRotation,
                        targetRotation,
                        t
                    );

                yield return null;
            }

            driverTransform.rotation =
                targetRotation;
        }

        // 2. 显示求助字幕
        if (speechBubble != null)
        {
            speechBubble.ShowHelpMessage();
        }

        // 3. Talking 动画
        if (driverAnimator != null)
        {
            driverAnimator.SetTrigger("Talk");
        }

        // 4. 等 Talking 播放完成
        yield return new WaitForSeconds(
            talkingDuration
        );

        // 5. Talking 结束后，蓝色手印出现
        // 注意：这里不关闭求助字幕
        if (carPushInteraction != null)
        {
            carPushInteraction.ShowHandPrint();
        }

        // 6. 解锁 DriverPushPoint
        carHelpManager.SetStage(
            CarHelpManager.CarHelpStage.WaitingForPushPoint
        );

        Debug.Log(
            "Talking结束 → 蓝色HandPrint出现，求助字幕继续显示"
        );
    }
}