using UnityEngine;

public class DriverDrivingIK : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Entering Car 状态名")]
    public string enteringCarStateName = "Entering Car";

    [Header("Driving 状态名")]
    public string drivingStateName = "Driving";

    [Header("双手")]
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    [Header("双脚")]
    public Transform leftFootTarget;
    public Transform rightFootTarget;

    [Header("手 IK")]
    [Range(0f, 1f)]
    public float handPositionWeight = 1f;

    [Range(0f, 1f)]
    public float handRotationWeight = 0f;

    [Header("Driving 脚 IK")]
    [Range(0f, 1f)]
    public float footPositionWeight = 0.8f;

    [Range(0f, 1f)]
    public float footRotationWeight = 0f;


    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null)
            return;


        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(0);


        bool isEnteringCar =
            stateInfo.IsName(
                enteringCarStateName
            );


        bool isDriving =
            stateInfo.IsName(
                drivingStateName
            );


        // ==================================================
        // Entering Car
        // 只控制双手
        // 双脚完全使用原动画
        // ==================================================

        if (isEnteringCar)
        {
            ApplyHandIK();

            ResetFootIK();

            return;
        }


        // ==================================================
        // Driving
        // 双手 + 双脚全部控制
        // ==================================================

        if (isDriving)
        {
            ApplyHandIK();

            ApplyFootIK();

            return;
        }


        // ==================================================
        // 其他状态全部关闭
        // ==================================================

        ResetAllIK();
    }


    // ======================================================
    // 双手 IK
    // ======================================================

    private void ApplyHandIK()
    {
        // 左手
        if (leftHandTarget != null)
        {
            animator.SetIKPositionWeight(
                AvatarIKGoal.LeftHand,
                handPositionWeight
            );

            animator.SetIKPosition(
                AvatarIKGoal.LeftHand,
                leftHandTarget.position
            );

            animator.SetIKRotationWeight(
                AvatarIKGoal.LeftHand,
                handRotationWeight
            );

            if (handRotationWeight > 0f)
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


        // 右手
        if (rightHandTarget != null)
        {
            animator.SetIKPositionWeight(
                AvatarIKGoal.RightHand,
                handPositionWeight
            );

            animator.SetIKPosition(
                AvatarIKGoal.RightHand,
                rightHandTarget.position
            );

            animator.SetIKRotationWeight(
                AvatarIKGoal.RightHand,
                handRotationWeight
            );

            if (handRotationWeight > 0f)
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
    // 双脚 IK
    // 只在 Driving 使用
    // ======================================================

    private void ApplyFootIK()
    {
        // 左脚
        if (leftFootTarget != null)
        {
            animator.SetIKPositionWeight(
                AvatarIKGoal.LeftFoot,
                footPositionWeight
            );

            animator.SetIKPosition(
                AvatarIKGoal.LeftFoot,
                leftFootTarget.position
            );

            animator.SetIKRotationWeight(
                AvatarIKGoal.LeftFoot,
                footRotationWeight
            );

            if (footRotationWeight > 0f)
            {
                animator.SetIKRotation(
                    AvatarIKGoal.LeftFoot,
                    leftFootTarget.rotation
                );
            }
        }
        else
        {
            animator.SetIKPositionWeight(
                AvatarIKGoal.LeftFoot,
                0f
            );

            animator.SetIKRotationWeight(
                AvatarIKGoal.LeftFoot,
                0f
            );
        }


        // 右脚
        if (rightFootTarget != null)
        {
            animator.SetIKPositionWeight(
                AvatarIKGoal.RightFoot,
                footPositionWeight
            );

            animator.SetIKPosition(
                AvatarIKGoal.RightFoot,
                rightFootTarget.position
            );

            animator.SetIKRotationWeight(
                AvatarIKGoal.RightFoot,
                footRotationWeight
            );

            if (footRotationWeight > 0f)
            {
                animator.SetIKRotation(
                    AvatarIKGoal.RightFoot,
                    rightFootTarget.rotation
                );
            }
        }
        else
        {
            animator.SetIKPositionWeight(
                AvatarIKGoal.RightFoot,
                0f
            );

            animator.SetIKRotationWeight(
                AvatarIKGoal.RightFoot,
                0f
            );
        }
    }


    // ======================================================
    // 关闭脚 IK
    // ======================================================

    private void ResetFootIK()
    {
        animator.SetIKPositionWeight(
            AvatarIKGoal.LeftFoot,
            0f
        );

        animator.SetIKRotationWeight(
            AvatarIKGoal.LeftFoot,
            0f
        );

        animator.SetIKPositionWeight(
            AvatarIKGoal.RightFoot,
            0f
        );

        animator.SetIKRotationWeight(
            AvatarIKGoal.RightFoot,
            0f
        );
    }


    // ======================================================
    // 全部关闭
    // ======================================================

    private void ResetAllIK()
    {
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


        ResetFootIK();
    }
}