using UnityEngine;

public class NPCTalkFacePlayer : MonoBehaviour
{
    [Header("NPC")]
    public Animator npcAnimator;

    [Header("玩家相机")]
    public Transform playerCamera;

    [Header("Talking状态名")]
    public string talkingStateName = "Talking";

    [Header("转身速度")]
    public float turnSpeed = 5f;

    private void Update()
    {
        if (npcAnimator == null || playerCamera == null)
            return;

        AnimatorStateInfo state =
            npcAnimator.GetCurrentAnimatorStateInfo(0);

        // 只有 Talking 时才转向玩家
        if (!state.IsName(talkingStateName))
            return;

        Vector3 direction =
            playerCamera.position - transform.position;

        // 不让人物抬头/低头，只水平旋转
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
    }
}