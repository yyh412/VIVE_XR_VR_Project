using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class TripTrigger : MonoBehaviour
{
    [Header("Character")]
    public Animator characterAnimator;
    public NavMeshAgent agent;
    public MonoBehaviour movementScript;


    // ==========================================================
    // Trip Ground
    // ==========================================================

    [Header("Trip Ground Settings")]

    [Range(0f, 1f)]
    public float startLoweringAt = 0.15f;

    public float loweringDuration = 0.65f;

    // 趴地时人物整体向下多少
    public float lyingRootOffset = -0.35f;


    // ==========================================================
    // Getting Up
    // ==========================================================

    [Header("Getting Up Settings")]

    // 趴地多久再起来
    public float getUpDelay = 1.0f;

    // Animator State 名称
    public string gettingUpStateName = "Getting Up";

    // 起身完成后进入的蹲姿
    public string crouchingStateName = "Crouching Idle";

    // 起身过程中恢复 Y 高度需要多久
    public float restoreHeightDuration = 1.3f;

    // Getting Up 播放到多少比例切换蹲姿
    [Range(0f, 1f)]
    public float enterCrouchingAt = 0.95f;


    // ==========================================================
    // Briefcase
    // ==========================================================

    [Header("Briefcase Settings")]

    public Transform briefcase;

    public Rigidbody briefcaseRigidbody;

    public Animator briefcaseAnimator;

    public PaperScatter paperScatter;

    [Range(0f, 1f)]
    public float releaseBriefcaseAt = 0.35f;

    public float openBriefcaseDelay = 0.15f;


    // ==========================================================
    // Internal
    // ==========================================================

    private bool triggered = false;

    // 摔倒前 Animator 根对象的世界坐标
    private Vector3 originalCharacterPosition;


    // ==========================================================
    // Trigger
    // ==========================================================

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;


        Animator hitAnimator =
            other.GetComponentInParent<Animator>();


        // 只允许指定人物触发
        if (hitAnimator != characterAnimator)
            return;


        triggered = true;


        // ==========================================
        // 记录摔倒前位置
        // ==========================================

        if (characterAnimator != null)
        {
            originalCharacterPosition =
                characterAnimator.transform.position;
        }


        // ==========================================
        // 停止 NavMesh
        // ==========================================

        if (agent != null)
        {
            agent.isStopped = true;

            agent.ResetPath();

            agent.updatePosition = false;

            agent.updateRotation = false;
        }


        // ==========================================
        // 停止移动脚本
        // ==========================================

        if (movementScript != null)
        {
            movementScript.enabled = false;
        }


        // ==========================================
        // 播放摔倒动画
        // ==========================================

        if (characterAnimator != null)
        {
            characterAnimator.SetTrigger("Trip");
        }


        // ==========================================
        // 同时执行
        // ==========================================

        StartCoroutine(
            SmoothLowerToGround()
        );


        StartCoroutine(
            ReleaseBriefcaseDuringTrip()
        );


        StartCoroutine(
            GetUpSequence()
        );
    }


    // ==========================================================
    // 摔倒过程中人物平滑贴地
    // ==========================================================

    private IEnumerator SmoothLowerToGround()
    {
        if (characterAnimator == null)
            yield break;


        // ==========================================
        // 等进入 Tripping
        // ==========================================

        while (
            !characterAnimator
                .GetCurrentAnimatorStateInfo(0)
                .IsName("Tripping")
        )
        {
            yield return null;
        }


        // ==========================================
        // 等动画播放到指定比例
        // ==========================================

        while (
            characterAnimator
                .GetCurrentAnimatorStateInfo(0)
                .normalizedTime
            < startLoweringAt
        )
        {
            yield return null;
        }


        Transform root =
            characterAnimator.transform;


        float startY =
            root.position.y;


        float targetY =
            originalCharacterPosition.y
            + lyingRootOffset;


        float elapsed = 0f;


        // ==========================================
        // 平滑下降
        // ==========================================

        while (
            elapsed < loweringDuration
        )
        {
            elapsed += Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed / loweringDuration
                );


            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );


            Vector3 position =
                root.position;


            position.y =
                Mathf.Lerp(
                    startY,
                    targetY,
                    t
                );


            root.position =
                position;


            yield return null;
        }


        // ==========================================
        // 最终高度
        // ==========================================

        Vector3 finalPosition =
            root.position;


        finalPosition.y =
            targetY;


        root.position =
            finalPosition;
    }


    // ==========================================================
    // Tripping → Getting Up
    //
    // 关键：
    // 直接移动 Animator 根对象
    // 让 Getting Up 第一帧 Hips
    // 对齐 Tripping 最后一帧 Hips
    // ==========================================================

    private IEnumerator GetUpSequence()
    {
        if (characterAnimator == null)
            yield break;


        // ==========================================
        // 等进入 Tripping
        // ==========================================

        while (
            !characterAnimator
                .GetCurrentAnimatorStateInfo(0)
                .IsName("Tripping")
        )
        {
            yield return null;
        }


        // ==========================================
        // 等 Tripping 接近最后一帧
        // ==========================================

        while (
            characterAnimator
                .GetCurrentAnimatorStateInfo(0)
                .normalizedTime
            < 0.98f
        )
        {
            yield return null;
        }


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
                "找不到 Hips，请确认 Rig = Humanoid"
            );

            yield break;
        }


        // ==========================================
        // 1.
        // 记录 Tripping 最后一帧
        // Hips 世界坐标
        // ==========================================

        Vector3 tripEndHipsPosition =
            hips.position;


        Debug.Log(
            "Trip End Hips = "
            + tripEndHipsPosition
        );


        // ==========================================
        // 冻结最后一帧
        // ==========================================

        characterAnimator.speed = 0f;


        yield return new WaitForSeconds(
            getUpDelay
        );


        characterAnimator.speed = 1f;


        // ==========================================
        // 2.
        // 切换到 Getting Up 第一帧
        // ==========================================

        characterAnimator.Play(
            gettingUpStateName,
            0,
            0f
        );


        // 强制立即计算
        characterAnimator.Update(
            0.0001f
        );


        // ==========================================
        // 3.
        // 获取 Getting Up 第一帧 Hips
        // ==========================================

        Vector3 getUpStartHipsPosition =
            hips.position;


        Debug.Log(
            "Get Up Start Hips = "
            + getUpStartHipsPosition
        );


        // ==========================================
        // 4.
        // 算出两个 Hips 的差值
        // ==========================================

        Vector3 correction =
            tripEndHipsPosition
            - getUpStartHipsPosition;


        Debug.Log(
            "Full Correction = "
            + correction
        );


        // ==========================================
        // 5.
        // ★ 直接移动 Animator 根对象
        //
        // 不再移动 VisualOffset
        // ==========================================

        Transform root =
            characterAnimator.transform;


        root.position +=
            correction;


        // ==========================================
        // 6.
        // 再强制更新一次
        // ==========================================

        characterAnimator.Update(
            0.0001f
        );


        Debug.Log(
            "Root moved to align Getting Up"
        );


        // ==========================================
        // 开始恢复高度
        // ==========================================

        StartCoroutine(
            SmoothRestoreHeight()
        );


        // ==========================================
        // 等 Getting Up 播放
        // ==========================================

        while (
            characterAnimator
                .GetCurrentAnimatorStateInfo(0)
                .IsName(gettingUpStateName)
            &&
            characterAnimator
                .GetCurrentAnimatorStateInfo(0)
                .normalizedTime
            < enterCrouchingAt
        )
        {
            yield return null;
        }


        // ==========================================
        // 进入蹲姿
        // ==========================================

        characterAnimator.CrossFade(
            crouchingStateName,
            0.08f
        );


        Debug.Log(
            "Character entered Crouching Idle"
        );
    }


    // ==========================================================
    // 起身过程中恢复人物 Y 高度
    // ==========================================================

    private IEnumerator SmoothRestoreHeight()
    {
        if (characterAnimator == null)
            yield break;


        Transform root =
            characterAnimator.transform;


        float startY =
            root.position.y;


        float targetY =
            originalCharacterPosition.y;


        float elapsed = 0f;


        while (
            elapsed < restoreHeightDuration
        )
        {
            elapsed += Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed
                    / restoreHeightDuration
                );


            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );


            Vector3 position =
                root.position;


            position.y =
                Mathf.Lerp(
                    startY,
                    targetY,
                    t
                );


            root.position =
                position;


            yield return null;
        }


        // ==========================================
        // 最后确保高度准确
        // ==========================================

        Vector3 finalPosition =
            root.position;


        finalPosition.y =
            targetY;


        root.position =
            finalPosition;


        Debug.Log(
            "Character Height Restored"
        );
    }


    // ==========================================================
    // 公文包脱手 → 打开 → 文件散落
    // ==========================================================

    private IEnumerator ReleaseBriefcaseDuringTrip()
    {
        if (characterAnimator == null)
            yield break;


        // ==========================================
        // 等进入 Tripping
        // ==========================================

        while (
            !characterAnimator
                .GetCurrentAnimatorStateInfo(0)
                .IsName("Tripping")
        )
        {
            yield return null;
        }


        // ==========================================
        // 等到松手时间
        // ==========================================

        while (
            characterAnimator
                .GetCurrentAnimatorStateInfo(0)
                .normalizedTime
            < releaseBriefcaseAt
        )
        {
            yield return null;
        }


        // ==========================================
        // 检查公文包
        // ==========================================

        if (briefcase == null)
        {
            Debug.LogWarning(
                "Briefcase 没有设置"
            );

            yield break;
        }


        if (briefcaseRigidbody == null)
        {
            Debug.LogWarning(
                "Briefcase Rigidbody 没有设置"
            );

            yield break;
        }


        // ==========================================
        // 脱离人物
        // ==========================================

        briefcase.SetParent(
            null,
            true
        );


        // ==========================================
        // 开启物理
        // ==========================================

        briefcaseRigidbody.isKinematic =
            false;


        briefcaseRigidbody.useGravity =
            true;


        Debug.Log(
            "Briefcase Released"
        );


        // ==========================================
        // 等待打开
        // ==========================================

        yield return new WaitForSeconds(
            openBriefcaseDelay
        );


        // ==========================================
        // 打开公文包
        // ==========================================

        if (briefcaseAnimator != null)
        {
            briefcaseAnimator.SetTrigger(
                "Open"
            );


            Debug.Log(
                "Briefcase Open Triggered"
            );
        }
        else
        {
            Debug.LogWarning(
                "Briefcase Animator 没有设置"
            );
        }


        // ==========================================
        // 文件散落
        // ==========================================

        if (paperScatter != null)
        {
            paperScatter.Scatter();


            Debug.Log(
                "Paper Scatter Triggered"
            );
        }
        else
        {
            Debug.LogWarning(
                "Paper Scatter 没有设置"
            );
        }
    }
}