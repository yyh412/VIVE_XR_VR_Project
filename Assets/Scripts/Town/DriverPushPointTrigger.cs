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

    [Header("转向设置")]
    public float turnDuration = 0.5f;

    private Quaternion originalPushRotation;

    private void Start()
    {
        if (driverTransform != null)
        {
            originalPushRotation =
                driverTransform.rotation;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (carHelpManager == null)
            return;

        // 只有 Driver 已经说完话以后
        // 这个点才有效
        if (!carHelpManager.IsStage(
            CarHelpManager.CarHelpStage.WaitingForPushPoint))
        {
            Debug.Log(
                "现在还不能触发 DriverPushPoint"
            );

            return;
        }

        Debug.Log(
            "正确进入 DriverPushPoint: " +
            other.name
        );

        carHelpManager.SetStage(
            CarHelpManager.CarHelpStage.ReadyToPush
        );

        StartCoroutine(TurnBackAndPush());
    }

    private IEnumerator TurnBackAndPush()
    {
        // Driver 转回原来的推车方向
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

        // 开始 Push
        if (driverAnimator != null)
        {
            driverAnimator.SetTrigger("Push");
        }

        // 开启手部检测
        if (carPushInteraction != null)
        {
            carPushInteraction.EnablePushInteraction();
        }
    }
}