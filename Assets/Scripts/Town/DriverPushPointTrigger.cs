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
        // 游戏开始时记录 Driver 原来的正确推车朝向
        if (driverTransform != null)
        {
            originalPushRotation =
                driverTransform.rotation;
        }
    }


    // ======================================================
    // VR 原来的触发流程
    // ======================================================

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


        Debug.Log(
            "玩家到达 DriverPushPoint"
        );


        StartPushSequence(false);
    }


    // ======================================================
    // Desktop：
    // Driver 的 E 调用这里
    // ======================================================

    public void DesktopStartPush()
    {
        if (hasStarted)
            return;


        if (carHelpManager == null)
        {
            Debug.LogWarning(
                "DriverPushPointTrigger：没有设置 CarHelpManager！"
            );

            return;
        }


        Debug.Log(
            "[Desktop] 按 E → 开始 Driver 推车流程"
        );


        // true = Desktop
        StartPushSequence(true);
    }


    // ======================================================
    // VR / Desktop 共用入口
    // ======================================================

    private void StartPushSequence(
        bool desktopMode)
    {
        if (hasStarted)
            return;


        hasStarted = true;


        Debug.Log(
            desktopMode
                ? "[Desktop] 开始 DriverPushPoint 推车流程"
                : "[VR] 开始 DriverPushPoint 推车流程"
        );


        // ==================================================
        // 设置 ReadyToPush
        // ==================================================

        if (carHelpManager != null)
        {
            carHelpManager.SetStage(
                CarHelpManager.CarHelpStage.ReadyToPush
            );
        }


        StartCoroutine(
            TurnBackAndPush(
                desktopMode
            )
        );
    }


    // ======================================================
    // 转回正确方向 + Pushing + IK
    // ======================================================

    private IEnumerator TurnBackAndPush(
        bool desktopMode)
    {
        // ==================================================
        // 1. 求助字幕消失
        // ==================================================

        if (speechBubble != null)
        {
            speechBubble.HideBubble();


            Debug.Log(
                "开始推车 → 求助字幕消失"
            );
        }


        // ==================================================
        // 2. 转身过程中关闭推车手 IK
        // ==================================================

        if (driverPushHandIK != null)
        {
            driverPushHandIK.DisablePushHandIK();
        }


        // ==================================================
        // 3. Driver 转回游戏开始时记录的正确推车方向
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


            // 最后强制精确回到原来的方向
            driverTransform.rotation =
                originalPushRotation;
        }


        Debug.Log(
            "Driver 已转回原来的正确推车方向"
        );


        // ==================================================
        // 4. 使用原来的 Push Trigger
        // ==================================================

        if (driverAnimator != null)
        {
            // 防止残留 StopPush
            driverAnimator.ResetTrigger(
                "StopPush"
            );


            driverAnimator.SetTrigger(
                "Push"
            );


            Debug.Log(
                "Push Trigger 已发送 → 进入 Pushing"
            );
        }


        // ==================================================
        // 5. 等 Animator 真正进入 Push
        // ==================================================

        yield return new WaitForSeconds(
            ikEnableDelay
        );


        // ==================================================
        // 6. 开启 Driver 双手 IK
        // ==================================================

        if (driverPushHandIK != null)
        {
            driverPushHandIK.EnablePushHandIK();


            Debug.Log(
                "Pushing 开始 → 推车双手 IK 开启"
            );
        }


        // ==================================================
        // 7. 检查 CarPushInteraction
        // ==================================================

        if (carPushInteraction == null)
        {
            Debug.LogWarning(
                "DriverPushPointTrigger：没有设置 CarPushInteraction！"
            );

            yield break;
        }


        // ==================================================
        // 8. Desktop / VR 分开
        // ==================================================

        if (desktopMode)
        {
            // ==============================================
            // Desktop：
            // 没有真实VR双手，所以不能等手靠近。
            //
            // 直接执行 DesktopCompletePush：
            // Pushing
            // → Keep pushing
            // → Sigh
            // → StopPush
            // → Push Stop
            // → 车移动
            // ==============================================

            Debug.Log(
                "[Desktop] 方向和IK已准备好 → 自动继续完成推车"
            );


            carPushInteraction.DesktopCompletePush();
        }
        else
        {
            // ==============================================
            // VR：
            // 保留原来的真实手部推车检测
            // ==============================================

            carPushInteraction.EnablePushInteraction();


            Debug.Log(
                "[VR] 开启玩家手部推车检测"
            );
        }
    }


    // ======================================================
    // 调试 / 重置游戏
    // ======================================================

    public void ResetPushPoint()
    {
        hasStarted = false;


        if (driverPushHandIK != null)
        {
            driverPushHandIK.DisablePushHandIK();
        }


        if (driverTransform != null)
        {
            driverTransform.rotation =
                originalPushRotation;
        }


        Debug.Log(
            "DriverPushPointTrigger 已重置"
        );
    }
}