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


    private bool ikEnabled = true;


    private void Start()
    {
        ikEnabled = enableIKOnStart;
    }


    // ======================================================
    // 开启推车手 IK
    // ======================================================

    public void EnablePushHandIK()
    {
        ikEnabled = true;

        Debug.Log(
            "Push Hand IK 开启"
        );
    }


    // ======================================================
    // 关闭推车手 IK
    // ======================================================

    public void DisablePushHandIK()
    {
        ikEnabled = false;

        ResetHandIK();

        Debug.Log(
            "Push Hand IK 关闭"
        );
    }


    // ======================================================
    // Animator IK
    // ======================================================

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null)
            return;


        // =========================================
        // 不再判断 Animator State
        //
        // 只要流程说 IK 开着，
        // Pushing / Transition / Push Stop
        // 全部保持手的位置
        // =========================================

        if (!ikEnabled)
        {
            ResetHandIK();
            return;
        }


        ApplyHandIK();
    }


    // ======================================================
    // 应用左右手 IK
    // ======================================================

    private void ApplyHandIK()
    {
        // =========================================
        // 左手
        // =========================================

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
        else
        {
            animator.SetIKPositionWeight(
                AvatarIKGoal.LeftHand,
                0f
            );

            animator.SetIKRotationWeight(
                AvatarIKGoal.LeftHand,
                0f
            );
        }


        // =========================================
        // 右手
        // =========================================

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
        else
        {
            animator.SetIKPositionWeight(
                AvatarIKGoal.RightHand,
                0f
            );

            animator.SetIKRotationWeight(
                AvatarIKGoal.RightHand,
                0f
            );
        }
    }


    // ======================================================
    // 清除双手 IK
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
}