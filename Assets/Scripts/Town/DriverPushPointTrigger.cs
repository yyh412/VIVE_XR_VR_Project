using System.Collections;
using UnityEngine;

public class DriverPushPointTrigger : MonoBehaviour
{
    [Header("流程控制")]
    public CarHelpManager carHelpManager;

    [Header("Driver")]
    public Animator driverAnimator;
    public Transform driverTransform;

    [Header("推车交互")]
    public CarPushInteraction carPushInteraction;

    [Header("字幕气泡")]
    public DriverSpeechBubble speechBubble;

    [Header("转向设置")]
    public float turnDuration = 0.5f;

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

        StartCoroutine(TurnBackAndPush());
    }

    private IEnumerator TurnBackAndPush()
    {
        // 1. 玩家已经到推车位置
        //    第一段求助字幕现在才消失
        if (speechBubble != null)
        {
            speechBubble.HideBubble();

            Debug.Log(
                "到达 DriverPushPoint → 求助字幕消失"
            );
        }

        // 2. Driver 转回原来的推车方向
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

                driverTransform.rotation =
                    Quaternion.Slerp(
                        startRotation,
                        originalPushRotation,
                        t
                    );

                yield return null;
            }

            driverTransform.rotation =
                originalPushRotation;
        }

        // 3. Driver 开始 Push
        if (driverAnimator != null)
        {
            driverAnimator.SetTrigger("Push");

            Debug.Log(
                "Driver 转回车的方向 → Push_InPlace"
            );
        }

        // 4. 开启手部检测
        if (carPushInteraction != null)
        {
            carPushInteraction.EnablePushInteraction();

            Debug.Log(
                "开启手部推车检测"
            );
        }
    }
}