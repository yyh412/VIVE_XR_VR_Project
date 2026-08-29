using UnityEngine;

public class DriverDrivingIK : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

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
    public float handRotationWeight = 1f;

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

        bool isDriving =
            stateInfo.IsName(drivingStateName);


        // ==================================================
        // 只有 Driving 才启用驾驶 IK
        // ==================================================

        if (isDriving)
        {
            ApplyHandIK();
            ApplyFootIK();
            return;
        }


        // ==================================================
        // Entering Car / Walking / Push / 其他状态
        //
        // 完全不碰 IK
        // 手脚全部使用动画自己的动作
        // ==================================================

        return;
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
    }


    // ======================================================
    // 双脚 IK
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
    }
}