using System.Collections;
using UnityEngine;

public class PickingUpPaperController : MonoBehaviour
{
    [Header("Character")]
    public Animator characterAnimator;


    [Header("Animation States")]

    // Animator 里的起身动画 State 名字
    public string gettingUpStateName = "Getting Up";

    // Animator 里的捡纸动画 State 名字
    public string pickingUpStateName = "Picking Up Paper";


    [Header("Timing")]

    // Getting Up 播放到多少比例时开始切换
    [Range(0f, 1f)]
    public float startPickingAt = 0.93f;

    // 两个动画之间的平滑过渡时间
    [Range(0.01f, 0.5f)]
    public float transitionDuration = 0.15f;


    [Header("Position Alignment")]

    // 是否修正动画之间的水平位置
    public bool alignHorizontalPosition = true;

    // 是否修正高度
    // 你现在两个动画 Y 高度已经分别调过，
    // 所以建议先关闭
    public bool alignVerticalPosition = false;


    private bool started = false;


    // ==========================================================
    // 每帧检查 Getting Up 是否快结束
    // ==========================================================

    private void Update()
    {
        if (started)
            return;

        if (characterAnimator == null)
            return;


        AnimatorStateInfo state =
            characterAnimator.GetCurrentAnimatorStateInfo(0);


        // 当前必须正在播放 Getting Up
        if (!state.IsName(gettingUpStateName))
            return;


        // 到达指定位置以后开始捡纸动画
        if (state.normalizedTime >= startPickingAt)
        {
            started = true;

            StartCoroutine(
                StartPickingUpSequence()
            );
        }
    }


    // ==========================================================
    // Getting Up
    // →
    // 平滑过渡
    // →
    // Picking Up Paper
    // ==========================================================

    private IEnumerator StartPickingUpSequence()
    {
        if (characterAnimator == null)
            yield break;


        // ==========================================
        // 获取 Hips
        // ==========================================

        Transform hips =
            characterAnimator.GetBoneTransform(
                HumanBodyBones.Hips
            );


        if (hips == null)
        {
            Debug.LogWarning(
                "找不到 Hips，请确认人物 Rig 是 Humanoid"
            );

            yield break;
        }


        Transform characterRoot =
            characterAnimator.transform;


        // ==========================================
        // 记录 Getting Up 当前 Hips 世界坐标
        // ==========================================

        Vector3 getUpEndHipsPosition =
            hips.position;


        Debug.Log(
            "Getting Up End Hips = "
            + getUpEndHipsPosition
        );


        // ==========================================
        // 平滑切换到 Picking Up Paper
        // ==========================================

        characterAnimator.CrossFade(
            pickingUpStateName,
            transitionDuration,
            0,
            0f
        );


        // ==========================================
        // 等 Animator 真正开始进入 Picking Up Paper
        // ==========================================

        float waitTime = 0f;


        while (
            !characterAnimator
                .GetCurrentAnimatorStateInfo(0)
                .IsName(pickingUpStateName)
        )
        {
            waitTime += Time.deltaTime;


            // 防止 State 名字写错以后无限等待
            if (waitTime > 2f)
            {
                Debug.LogWarning(
                    "没有进入 Picking Up Paper。请检查 Animator State 名字。"
                );

                yield break;
            }


            yield return null;
        }


        // ==========================================
        // 再等一帧
        // 让新动画的骨骼位置稳定
        // ==========================================

        yield return null;


        // ==========================================
        // 获取 Picking Up Paper 当前 Hips
        // ==========================================

        Vector3 pickingStartHipsPosition =
            hips.position;


        Debug.Log(
            "Picking Start Hips = "
            + pickingStartHipsPosition
        );


        // ==========================================
        // 算两个动画的坐标差
        // ==========================================

        Vector3 correction =
            getUpEndHipsPosition
            - pickingStartHipsPosition;


        // ==========================================
        // 根据需要决定修哪些方向
        // ==========================================

        if (!alignHorizontalPosition)
        {
            correction.x = 0f;
            correction.z = 0f;
        }


        if (!alignVerticalPosition)
        {
            correction.y = 0f;
        }


        Debug.Log(
            "Picking Correction = "
            + correction
        );


        // ==========================================
        // 对齐人物根节点
        // ==========================================

        characterRoot.position +=
            correction;


        Debug.Log(
            "Picking Up Paper transition finished"
        );
    }
}