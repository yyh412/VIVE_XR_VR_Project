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

    [Header("转身设置")]
    public float turnDuration = 0.6f;

    [Header("Talking 动画时间")]
    public float talkingDuration = 3.93f;

    private void OnTriggerEnter(Collider other)
    {
        // 只有第一阶段才能触发
        if (carHelpManager == null)
            return;

        if (!carHelpManager.IsStage(
            CarHelpManager.CarHelpStage.WaitingForHelp))
        {
            return;
        }

        Debug.Log("正确进入 HelpTrigger: " + other.name);

        carHelpManager.SetStage(
            CarHelpManager.CarHelpStage.Talking);

        StartCoroutine(TurnAndTalk());
    }

    private IEnumerator TurnAndTalk()
    {
        if (driverTransform == null ||
            playerTransform == null)
        {
            yield break;
        }

        // =========================
        // 1. Driver 转向玩家
        // =========================

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

        // =========================
        // 2. Talking
        // =========================

        if (driverAnimator != null)
        {
            driverAnimator.SetTrigger("Talk");
        }

        // =========================
        // 3. 等 3.93 秒
        // =========================

        yield return new WaitForSeconds(
            talkingDuration
        );

        // =========================
        // 4. 蓝色 HandPrint 出现
        // =========================

        if (carPushInteraction != null)
        {
            carPushInteraction.ShowHandPrint();
        }

        // =========================
        // 5. 解锁 DriverPushPoint
        // =========================

        carHelpManager.SetStage(
            CarHelpManager.CarHelpStage.WaitingForPushPoint
        );
    }
}