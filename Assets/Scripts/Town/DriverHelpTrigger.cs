using UnityEngine;
using System.Collections;

public class DriverHelpTrigger : MonoBehaviour
{
    [Header("流程控制")]
    public CarHelpManager carHelpManager;

    [Header("Driver")]
    public Animator driverAnimator;
    public Transform driverTransform;

    [Header("Driver 推车手 IK")]
    public DriverPushHandIK driverPushHandIK;

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

    [Header("触发方式")]
    [Tooltip("现在用眼动触发，所以建议关闭")]
    public bool allowAreaTrigger = false;

    private bool hasTriggered = false;


    // ==================================================
    // 原来的区域触发
    // 现在默认关闭
    // ==================================================

    private void OnTriggerEnter(Collider other)
    {
        if (!allowAreaTrigger)
            return;

        TriggerHelp();
    }


    // ==================================================
    // 新增：
    // 给眼动系统调用
    // ==================================================

    public void TriggerHelp()
    {
        if (hasTriggered)
            return;


        if (carHelpManager == null)
            return;


        if (!carHelpManager.IsStage(
            CarHelpManager.CarHelpStage.WaitingForHelp))
        {
            return;
        }


        hasTriggered = true;


        Debug.Log(
            "[DriverHelpTrigger] 眼动触发 Driver 求助流程。"
        );


        carHelpManager.SetStage(
            CarHelpManager.CarHelpStage.Talking
        );


        StartCoroutine(
            TurnAndTalk()
        );
    }


    private IEnumerator TurnAndTalk()
    {
        if (driverTransform == null ||
            playerTransform == null)
        {
            yield break;
        }


        // ==================================================
        // 0. Driver 转向玩家前关闭推车手 IK
        // ==================================================

        if (driverPushHandIK != null)
        {
            driverPushHandIK.DisablePushHandIK();

            Debug.Log(
                "Driver 开始求助 → 推车手 IK 关闭"
            );
        }


        // ==================================================
        // 1. Driver 转向玩家
        // ==================================================

        Vector3 direction =
            playerTransform.position -
            driverTransform.position;


        direction.y = 0f;


        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion startRotation =
                driverTransform.rotation;


            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction
                );


            float time = 0f;


            while (time < turnDuration)
            {
                time +=
                    Time.deltaTime;


                float t =
                    Mathf.Clamp01(
                        time /
                        turnDuration
                    );


                float smoothT =
                    t * t *
                    (3f - 2f * t);


                driverTransform.rotation =
                    Quaternion.Slerp(
                        startRotation,
                        targetRotation,
                        smoothT
                    );


                yield return null;
            }


            driverTransform.rotation =
                targetRotation;
        }


        // ==================================================
        // 2. 显示求助字幕
        // ==================================================

        if (speechBubble != null)
        {
            speechBubble.ShowHelpMessage();
        }


        // ==================================================
        // 3. Talking 动画
        // ==================================================

        if (driverAnimator != null)
        {
            driverAnimator.SetTrigger(
                "Talk"
            );
        }


        // ==================================================
        // 4. 等待 Talking 播放结束
        // ==================================================

        yield return new WaitForSeconds(
            talkingDuration
        );


        // ==================================================
        // 5. 显示蓝色手印
        // ==================================================

        if (carPushInteraction != null)
        {
            carPushInteraction.ShowHandPrint();
        }


        // ==================================================
        // 6. 进入等待玩家到 DriverPushPoint
        // ==================================================

        carHelpManager.SetStage(
            CarHelpManager.CarHelpStage.WaitingForPushPoint
        );


        Debug.Log(
            "Talking结束 → 蓝色HandPrint出现 → 等待玩家到DriverPushPoint"
        );
    }
}