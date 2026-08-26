using System.Collections;
using UnityEngine;

public class DriverPushPointTrigger : MonoBehaviour
{
    [Header("流程控制")]
    public CarHelpManager carHelpManager;

    [Header("Driver")]
    public Animator driverAnimator;
    public Transform driverTransform;

    [Header("Driver 推车手 IK")]
    public DriverPushHandIK driverPushHandIK;

    [Header("推车交互")]
    public CarPushInteraction carPushInteraction;

    [Header("字幕气泡")]
    public DriverSpeechBubble speechBubble;

    [Header("转向设置")]
    public float turnDuration = 0.5f;

    [Header("切到 Push 后多久重新开启手 IK")]
    public float ikEnableDelay = 0.15f;

    private Quaternion originalPushRotation;

    private bool hasStarted = false;


    private void Start()
    {
        if (driverTransform != null)
        {
            originalPushRotation =
                driverTransform.rotation;
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (hasStarted)
            return;

        if (carHelpManager == null)
            return;


        // 必须等 Driver 已经说完话
        if (!carHelpManager.IsStage(
            CarHelpManager.CarHelpStage.WaitingForPushPoint))
        {
            return;
        }


        hasStarted = true;


        Debug.Log(
            "玩家到达 DriverPushPoint"
        );


        carHelpManager.SetStage(
            CarHelpManager.CarHelpStage.ReadyToPush
        );


        StartCoroutine(
            TurnBackAndPush()
        );
    }


    private IEnumerator TurnBackAndPush()
    {
        // ==================================================
        // 1. 求助字幕消失
        // ==================================================

        if (speechBubble != null)
        {
            speechBubble.HideBubble();


            Debug.Log(
                "到达 DriverPushPoint → 求助字幕消失"
            );
        }


        // ==================================================
        // 2. 再保险：
        // 转身过程中保持推车手 IK 关闭
        // ==================================================

        if (driverPushHandIK != null)
        {
            driverPushHandIK.DisablePushHandIK();
        }


        // ==================================================
        // 3. Driver 转回原来的推车方向
        // ==================================================

        if (driverTransform != null)
        {
            Quaternion startRotation =
                driverTransform.rotation;


            float time = 0f;


            while (time < turnDuration)
            {
                time += Time.deltaTime;


                float t =
                    Mathf.Clamp01(
                        time / turnDuration
                    );


                float smoothT =
                    t * t *
                    (3f - 2f * t);


                driverTransform.rotation =
                    Quaternion.Slerp(
                        startRotation,
                        originalPushRotation,
                        smoothT
                    );


                yield return null;
            }


            driverTransform.rotation =
                originalPushRotation;
        }


        Debug.Log(
            "Driver 已转回原来的推车方向"
        );


        // ==================================================
        // 4. 切回 Push_InPlace
        // ==================================================

        if (driverAnimator != null)
        {
            driverAnimator.SetTrigger(
                "Push"
            );


            Debug.Log(
                "Push Trigger → Push_InPlace"
            );
        }


        // ==================================================
        // 5. 稍等一下
        // 让 Animator 真正进入 Push 状态
        //
        // 否则刚 SetTrigger 就开 IK，
        // Animator 可能仍在 Talking Transition
        // ==================================================

        yield return new WaitForSeconds(
            ikEnableDelay
        );


        // ==================================================
        // 6. 重新开启 Driver 推车双手 IK
        // ==================================================

        if (driverPushHandIK != null)
        {
            driverPushHandIK.EnablePushHandIK();


            Debug.Log(
                "Push_InPlace开始 → 推车手 IK 重新开启"
            );
        }


        // ==================================================
        // 7. 开启玩家手部推车检测
        // ==================================================

        if (carPushInteraction != null)
        {
            carPushInteraction.EnablePushInteraction();


            Debug.Log(
                "开启玩家手部推车检测"
            );
        }
    }
}