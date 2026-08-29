using UnityEngine;

public class DriverPushHandIK : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("左右手目标")]
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    [Header("位置权重")]
    [Range(0f, 1f)]
    public float positionWeight = 1f;

    [Header("旋转权重")]
    [Range(0f, 1f)]
    public float rotationWeight = 0f;

    [Header("游戏开始是否开启 IK")]
    public bool enableIKOnStart = true;

    private bool ikEnabled = false;


    private void Start()
    {
        ikEnabled = enableIKOnStart;
    }


    // ======================================================
    // 开启推车双手 IK
    // ======================================================

    public void EnablePushHandIK()
    {
        ikEnabled = true;

        Debug.Log("Push Hand IK 开启");
    }


    // ======================================================
    // 关闭推车双手 IK
    // ======================================================

    public void DisablePushHandIK()
    {
        ikEnabled = false;

        // 只在关闭这一刻清一次
        ResetHandIK();

        Debug.Log("Push Hand IK 关闭 → 不再干扰 Driving IK");
    }


    // ======================================================
    // Animator IK
    // ======================================================

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null)
            return;

        // 非常重要：
        // 关闭后什么都不做。
        // 不能每帧 ResetHandIK，
        // 否则会把 DriverDrivingIK 的结果覆盖掉。
        if (!ikEnabled)
            return;

        ApplyHandIK();
    }


    // ======================================================
    // 推车双手 IK
    // ======================================================

    private void ApplyHandIK()
    {
        // -------------------------
        // 左手
        // -------------------------

        if (leftHandTarget != null)
        {
            animator.SetIKPositionWeight(
                AvatarIKGoal.LeftHand,
                positionWeight
            );

            animator.SetIKPosition(
                AvatarIKGoal.LeftHand,
                leftHandTarget.position
            );

            animator.SetIKRotationWeight(
                AvatarIKGoal.LeftHand,
                rotationWeight
            );

            if (rotationWeight > 0f)
            {
                animator.SetIKRotation(
                    AvatarIKGoal.LeftHand,
                    leftHandTarget.rotation
                );
            }
        }


        // -------------------------
        // 右手
        // -------------------------

        if (rightHandTarget != null)
        {
            animator.SetIKPositionWeight(
                AvatarIKGoal.RightHand,
                positionWeight
            );

            animator.SetIKPosition(
                AvatarIKGoal.RightHand,
                rightHandTarget.position
            );

            animator.SetIKRotationWeight(
                AvatarIKGoal.RightHand,
                rotationWeight
            );

            if (rotationWeight > 0f)
            {
                animator.SetIKRotation(
                    AvatarIKGoal.RightHand,
                    rightHandTarget.rotation
                );
            }
        }
    }


    // ======================================================
    // 只在主动关闭时调用
    // ======================================================

    private void ResetHandIK()
    {
        if (animator == null)
            return;

        animator.SetIKPositionWeight(
            AvatarIKGoal.LeftHand,
            0f
        );

        animator.SetIKRotationWeight(
            AvatarIKGoal.LeftHand,
            0f
        );

        animator.SetIKPositionWeight(
            AvatarIKGoal.RightHand,
            0f
        );

        animator.SetIKRotationWeight(
            AvatarIKGoal.RightHand,
            0f
        );
    }


    public bool IsPushIKEnabled()
    {
        return ikEnabled;
    }
}