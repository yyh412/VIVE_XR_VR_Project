using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class TripTrigger : MonoBehaviour
{
    [Header("Character")]
    public Animator characterAnimator;
    public NavMeshAgent agent;
    public MonoBehaviour movementScript;

    [Header("Trip Ground Settings")]

    // 摔倒动画播放到多少比例后，开始向地面下降
    [Range(0f, 1f)]
    public float startLoweringAt = 0.15f;

    // 从当前高度平滑下降到地面需要多久
    public float loweringDuration = 0.65f;

    // 趴下以后角色根节点相对于 NavMesh 地面的高度
    public float lyingRootOffset = -0.35f;

    private bool triggered = false;

    // 记录人物正常站立时相对于地面的高度
    private float standingRootOffset = 0f;
    private bool standingOffsetRecorded = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        // 只让指定 NPC 触发
        Animator hitAnimator = other.GetComponentInParent<Animator>();

        if (hitAnimator != characterAnimator)
            return;

        triggered = true;

        RecordStandingHeight();

        // 停止 NavMesh 移动
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();

            // 摔倒以后不再让 NavMeshAgent 强制修改位置与朝向
            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        // 停止原来的路径移动脚本
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }

        // 播放摔倒动画
        characterAnimator.SetTrigger("Trip");

        // 摔倒过程中平滑贴近地面
        StartCoroutine(SmoothLowerToGround());
    }

    private IEnumerator SmoothLowerToGround()
    {
        // 等 Animator 真正进入 Tripping
        while (!characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Tripping"))
        {
            yield return null;
        }

        // 等动画播放到开始下降的时间
        while (characterAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime
               < startLoweringAt)
        {
            yield return null;
        }

        Transform characterRoot = characterAnimator.transform;

        // 找当前附近的 NavMesh 地面
        if (!NavMesh.SamplePosition(
            characterRoot.position,
            out NavMeshHit hit,
            2f,
            NavMesh.AllAreas))
        {
            yield break;
        }

        float startY = characterRoot.position.y;
        float targetY = hit.position.y + lyingRootOffset;

        float elapsed = 0f;

        while (elapsed < loweringDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / loweringDuration);

            // 平滑开始、平滑结束
            t = Mathf.SmoothStep(0f, 1f, t);

            Vector3 position = characterRoot.position;
            position.y = Mathf.Lerp(startY, targetY, t);

            characterRoot.position = position;

            yield return null;
        }

        // 最后一帧确保到目标高度
        Vector3 finalPosition = characterRoot.position;
        finalPosition.y = targetY;
        characterRoot.position = finalPosition;
    }

    private void RecordStandingHeight()
    {
        if (NavMesh.SamplePosition(
            characterAnimator.transform.position,
            out NavMeshHit hit,
            2f,
            NavMesh.AllAreas))
        {
            standingRootOffset =
                characterAnimator.transform.position.y - hit.position.y;

            standingOffsetRecorded = true;
        }
    }

    // 后面爬起来时调用
    public void RestoreStandingHeight()
    {
        if (!standingOffsetRecorded)
            return;

        Transform characterRoot = characterAnimator.transform;

        if (NavMesh.SamplePosition(
            characterRoot.position,
            out NavMeshHit hit,
            2f,
            NavMesh.AllAreas))
        {
            Vector3 position = characterRoot.position;

            position.y = hit.position.y + standingRootOffset;

            characterRoot.position = position;
        }
    }
}