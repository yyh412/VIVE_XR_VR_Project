using System.Collections;
using UnityEngine;

public class StoryPaperLanding : MonoBehaviour
{
    [Header("Target")]
    public Transform landingPoint;

    [Header("Timing")]
    [Tooltip("纸张刚飞出后，保留一点真实物理飞行时间")]
    public float freeFlyTime = 0.15f;

    [Tooltip("纸张漂向固定落点所需时间")]
    public float moveToTargetDuration = 2.2f;

    [Header("Landing")]
    [Tooltip("最终落点高于 Landing Point 的距离，防止纸陷入地面")]
    public float heightOffset = 0.05f;

    [Header("Floating")]
    [Tooltip("左右飘动幅度")]
    public float swayAmount = 0.08f;

    [Tooltip("上下轻微浮动幅度")]
    public float floatAmount = 0.04f;

    [Tooltip("飘动速度")]
    public float swaySpeed = 2.2f;

    [Tooltip("纸在空中的弧线高度")]
    public float arcHeight = 0.55f;

    [Header("After Landing")]
    [Tooltip("落地以后是否恢复为可抓取状态")]
    public bool enablePhysicsAfterLanding = true;

    [Tooltip("落地后恢复物理前等待时间")]
    public float physicsRestoreDelay = 0.1f;

    [Header("Landing Glow")]
    [Tooltip("纸落地后才开启的灯光对象")]
    public GameObject landingGlow;

    private Rigidbody rb;
    private bool started = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // 游戏开始时先关闭灯光
        if (landingGlow != null)
        {
            landingGlow.SetActive(false);
        }
    }

    public void StartControlledLanding()
    {
        if (started)
            return;

        started = true;

        StartCoroutine(ControlledLanding());
    }

    private IEnumerator ControlledLanding()
    {
        if (rb == null)
        {
            Debug.LogWarning(gameObject.name + " 没有 Rigidbody");
            yield break;
        }

        if (landingPoint == null)
        {
            Debug.LogWarning(gameObject.name + " 没有设置 Landing Point");
            yield break;
        }

        // -------------------------------------------------
        // 1. 先保留一点真实飞行
        // -------------------------------------------------

        yield return new WaitForSeconds(freeFlyTime);

        // -------------------------------------------------
        // 2. 脚本接管纸张
        // -------------------------------------------------

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;
        rb.isKinematic = true;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 targetPosition =
            landingPoint.position +
            Vector3.up * heightOffset;

        Quaternion targetRotation =
            landingPoint.rotation;

        float elapsed = 0f;

        // 每张纸的漂移相位不同
        float randomPhase =
            Random.Range(0f, Mathf.PI * 2f);

        // -------------------------------------------------
        // 3. 飘向目标位置
        // -------------------------------------------------

        while (elapsed < moveToTargetDuration)
        {
            elapsed += Time.deltaTime;

            float rawT =
                Mathf.Clamp01(
                    elapsed / moveToTargetDuration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    rawT
                );

            // 基础移动
            Vector3 currentPosition =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    smoothT
                );

            // 空中弧线
            float arc =
                Mathf.Sin(rawT * Mathf.PI) *
                arcHeight;

            currentPosition +=
                Vector3.up * arc;

            // 越接近落点，漂动越弱
            float fade =
                1f - rawT;

            // 左右漂动
            float sway =
                Mathf.Sin(
                    elapsed * swaySpeed +
                    randomPhase
                ) *
                swayAmount *
                fade;

            // 前后漂动
            float forwardSway =
                Mathf.Cos(
                    elapsed *
                    swaySpeed *
                    0.8f +
                    randomPhase
                ) *
                swayAmount *
                0.5f *
                fade;

            // 上下轻微浮动
            float floating =
                Mathf.Sin(
                    elapsed *
                    swaySpeed *
                    1.3f +
                    randomPhase
                ) *
                floatAmount *
                fade;

            currentPosition +=
                transform.right *
                sway;

            currentPosition +=
                transform.forward *
                forwardSway;

            currentPosition +=
                Vector3.up *
                floating;

            transform.position =
                currentPosition;

            // 慢慢调整到 Landing Point 的方向
            transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    smoothT
                );

            yield return null;
        }

        // -------------------------------------------------
        // 4. 最终固定到落点
        // -------------------------------------------------

        transform.position =
            targetPosition;

        transform.rotation =
            targetRotation;

        rb.velocity =
            Vector3.zero;

        rb.angularVelocity =
            Vector3.zero;

        // -------------------------------------------------
        // 5. 纸真正落地以后开启灯光
        // -------------------------------------------------

        if (landingGlow != null)
        {
            landingGlow.SetActive(true);
        }

        // -------------------------------------------------
        // 6. 恢复可抓取的 Rigidbody 状态
        // -------------------------------------------------

        if (enablePhysicsAfterLanding)
        {
            yield return new WaitForSeconds(
                physicsRestoreDelay
            );

            rb.isKinematic = false;
            rb.useGravity = true;

            rb.velocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;
        }

        Debug.Log(
            gameObject.name +
            " landed, glow enabled, ready for pickup."
        );
    }
}