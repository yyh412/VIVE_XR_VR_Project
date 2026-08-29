using UnityEngine;

public class DriverFarewellLookAt : MonoBehaviour
{
    [Header("Driver")]
    public Animator animator;

    [Header("玩家头部")]
    [Tooltip("拖入 XR Origin / Camera Offset / Main Camera")]
    public Transform playerHead;


    // ======================================================
    // Look At 参数
    // ======================================================

    [Header("看向玩家参数")]

    [Range(0f, 1f)]
    public float lookWeight = 0.75f;

    [Range(0f, 1f)]
    public float bodyWeight = 0.05f;

    [Range(0f, 1f)]
    public float headWeight = 0.65f;

    [Range(0f, 1f)]
    public float eyesWeight = 0f;

    [Tooltip("越接近1，头越不容易扭到极端角度")]
    [Range(0f, 1f)]
    public float clampWeight = 0.75f;


    // ======================================================
    // 平滑
    // ======================================================

    [Header("平滑")]

    public float lookInSpeed = 2.5f;
    public float lookOutSpeed = 2f;


    // ======================================================
    // 看向范围限制
    // ======================================================

    [Header("看向范围限制")]

    [Tooltip("玩家最多在Driver正前方左右多少度内才看")]
    [Range(20f, 120f)]
    public float maxLookAngle = 65f;

    [Tooltip("超过这个距离就不继续追着看")]
    public float maxLookDistance = 4f;

    [Tooltip("距离太近时避免头部疯狂旋转")]
    public float minLookDistance = 0.5f;


    // ======================================================
    // 目标位置微调
    // ======================================================

    [Header("目标位置微调")]

    public Vector3 targetOffset =
        Vector3.zero;


    // ======================================================
    // 调试
    // ======================================================

    [Header("调试")]

    public bool isLooking = false;


    // ======================================================
    // 内部
    // ======================================================

    private float currentWeight = 0f;


    private void Start()
    {
        if (animator == null)
        {
            Debug.LogError(
                "DriverFarewellLookAt：Animator 没有设置！"
            );
        }

        if (playerHead == null)
        {
            Debug.LogError(
                "DriverFarewellLookAt：Player Head 没有设置！"
            );
        }
    }


    private void Update()
    {
        float targetWeight = 0f;


        if (isLooking &&
            CanLookAtPlayer())
        {
            targetWeight =
                lookWeight;
        }


        float speed =
            targetWeight > currentWeight
            ? lookInSpeed
            : lookOutSpeed;


        currentWeight =
            Mathf.MoveTowards(
                currentWeight,
                targetWeight,
                speed * Time.deltaTime
            );
    }


    private bool CanLookAtPlayer()
    {
        if (playerHead == null)
            return false;


        Vector3 toPlayer =
            playerHead.position -
            transform.position;


        float distance =
            toPlayer.magnitude;


        if (distance >
            maxLookDistance)
        {
            return false;
        }


        if (distance <
            minLookDistance)
        {
            return false;
        }


        toPlayer.y = 0f;


        if (toPlayer.sqrMagnitude <
            0.001f)
        {
            return false;
        }


        float angle =
            Vector3.Angle(
                transform.forward,
                toPlayer.normalized
            );


        if (angle >
            maxLookAngle)
        {
            return false;
        }


        return true;
    }


    private void OnAnimatorIK(
        int layerIndex)
    {
        if (animator == null)
            return;


        if (playerHead == null ||
            currentWeight <= 0.001f)
        {
            animator.SetLookAtWeight(
                0f
            );

            return;
        }


        Vector3 targetPosition =
            playerHead.position +
            targetOffset;


        animator.SetLookAtWeight(
            currentWeight,
            bodyWeight,
            headWeight,
            eyesWeight,
            clampWeight
        );


        animator.SetLookAtPosition(
            targetPosition
        );
    }


    public void StartLookingAtPlayer()
    {
        isLooking = true;

        Debug.Log(
            "Driver开始自然看向玩家"
        );
    }


    public void StopLookingAtPlayer()
    {
        isLooking = false;

        Debug.Log(
            "Driver停止看玩家"
        );
    }
}