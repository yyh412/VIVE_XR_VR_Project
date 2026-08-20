using System.Collections;
using UnityEngine;

public class StoryPaperLanding : MonoBehaviour
{
    [Header("Target")]
    public Transform landingPoint;

    [Header("Timing")]
    public float freeFlyTime = 0.8f;
    public float moveToTargetDuration = 0.6f;

    [Header("Landing")]
    public float heightOffset = 0.01f;

    private Rigidbody rb;
    private bool started = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
            Debug.LogWarning("NPC_PickupPaper 没有 Rigidbody");
            yield break;
        }

        if (landingPoint == null)
        {
            Debug.LogWarning("NPCPaperLandingPoint 没有设置");
            yield break;
        }

        // 先正常飞一会
        yield return new WaitForSeconds(freeFlyTime);

        // 停止物理
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

        while (elapsed < moveToTargetDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / moveToTargetDuration
                );

            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    t
                );

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;

        Debug.Log("NPC pickup paper landed at target");
    }
}