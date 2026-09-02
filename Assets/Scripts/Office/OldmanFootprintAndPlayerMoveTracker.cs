using UnityEngine;
using UnityEngine.AI;

public class OldmanFootprintAndPlayerMoveTracker : MonoBehaviour
{
    // =====================================================
    // 玩家真实移动检测
    // =====================================================

    [Header("玩家头显")]
    [Tooltip("拖入 XR Origin / Camera Offset / Main Camera")]
    public Transform playerHead;

    [Header("XR Origin")]
    [Tooltip("拖入 XR Origin 根物体")]
    public Transform xrOrigin;


    // =====================================================
    // 现实移动放大
    // =====================================================

    [Header("现实移动放大")]

    [Tooltip("是否开启现实移动距离放大")]
    public bool enableMovementAmplification = true;

    [Tooltip("2 = 现实走1米，VR中总共约走2米")]
    public float movementMultiplier = 2f;

    [Tooltip("过滤非常轻微的头显抖动。建议0.003～0.01")]
    public float playerMoveThreshold = 0.005f;

    [Tooltip("如果单帧变化超过这个值，认为是重定位/校准，不进行放大")]
    public float maxPhysicalDeltaPerFrame = 0.3f;


    // =====================================================
    // 玩家移动数据显示
    // =====================================================

    [Header("玩家真实移动数据显示")]

    [Tooltip("这一帧玩家现实中移动了多少米")]
    public float currentRealMoveDistance = 0f;

    [Tooltip("累计检测到的现实移动距离")]
    public float totalRealMoveDistance = 0f;

    [Tooltip("当前是否检测到玩家真实移动")]
    public bool playerIsMoving = false;


    // =====================================================
    // 老人脚印
    // =====================================================

    [Header("老人")]
    [Tooltip("拖入 Oldman 根物体")]
    public Transform oldmanRoot;

    [Header("老人 NavMeshAgent")]
    [Tooltip("拖入 Oldman 上面的 NavMeshAgent")]
    public NavMeshAgent oldmanAgent;

    [Header("脚印 Prefab")]
    [Tooltip("一个 Prefab 本身就是一整对脚印")]
    public GameObject footprintPrefab;

    [Header("脚印间距")]
    [Tooltip("老人每移动多少米生成一整对脚印")]
    public float footprintDistance = 1.2f;

    [Header("向后偏移")]
    [Tooltip("脚印生成在老人后方多少米")]
    public float backOffset = 0.15f;


    // =====================================================
    // 地面检测
    // =====================================================

    [Header("地面检测")]
    public LayerMask groundMask = ~0;

    [Tooltip("从多高的位置向下射线检测地面")]
    public float raycastHeight = 1.5f;

    [Tooltip("向下检测多远")]
    public float raycastDistance = 5f;

    [Tooltip("脚印稍微高于地面，避免闪烁")]
    public float groundOffset = 0.02f;


    // =====================================================
    // 脚印旋转
    // =====================================================

    [Header("脚印额外旋转")]
    [Tooltip("如果 Prefab 方向不对，在这里调整")]
    public Vector3 footprintRotationOffset = Vector3.zero;


    // =====================================================
    // Debug
    // =====================================================

    [Header("Debug")]
    public bool showDebugLog = false;


    // =====================================================
    // 玩家内部变量
    // =====================================================

    private Vector3 lastPlayerLocalPosition;
    private bool hasPlayerPosition = false;


    // =====================================================
    // 老人内部变量
    // =====================================================

    private Vector3 lastOldmanPosition;
    private bool hasOldmanPosition = false;


    // =====================================================
    // Update
    // =====================================================

    private void Update()
    {
        TrackPlayerRealMovement();

        TrackOldmanMovement();
    }


    // =====================================================
    // 玩家真实物理移动检测 + 2倍移动
    // =====================================================

    private void TrackPlayerRealMovement()
    {
        if (playerHead == null ||
            xrOrigin == null)
        {
            return;
        }


        // =================================================
        // 获取头显相对于 XR Origin 的本地位置
        //
        // 关键：
        // XR Origin 被我们自己移动以后，
        // 不会被再次当成玩家真实移动。
        // =================================================

        Vector3 currentLocalPosition =
            xrOrigin.InverseTransformPoint(
                playerHead.position
            );


        // 只计算水平移动
        currentLocalPosition.y = 0f;


        // =================================================
        // 第一次运行，只保存头显位置
        // =================================================

        if (!hasPlayerPosition)
        {
            lastPlayerLocalPosition =
                currentLocalPosition;

            hasPlayerPosition = true;

            return;
        }


        // =================================================
        // 计算这一帧头显在现实 Tracking Space 中
        // 实际移动了多少
        // =================================================

        Vector3 localDelta =
            currentLocalPosition -
            lastPlayerLocalPosition;


        // 不要上下移动
        localDelta.y = 0f;


        float distance =
            localDelta.magnitude;


        currentRealMoveDistance = 0f;
        playerIsMoving = false;


        // =================================================
        // 防止重新校准 / Tracking 突然跳动
        // =================================================

        if (distance >
            maxPhysicalDeltaPerFrame)
        {
            if (showDebugLog)
            {
                Debug.LogWarning(
                    "[PlayerMove] 检测到异常大位移，" +
                    "可能是头显重定位，本帧忽略：" +
                    distance.ToString("F3") +
                    " m"
                );
            }


            lastPlayerLocalPosition =
                currentLocalPosition;

            return;
        }


        // =================================================
        // 有真实移动
        // =================================================

        if (distance >=
            playerMoveThreshold)
        {
            currentRealMoveDistance =
                distance;

            totalRealMoveDistance +=
                distance;

            playerIsMoving =
                true;


            // =================================================
            // 现实移动放大
            //
            // 注意：
            //
            // 玩家现实已经走了 1 倍。
            //
            // movementMultiplier = 2
            //
            // 所以 XR Origin 只需要额外移动：
            //
            // 2 - 1 = 1 倍
            //
            // 最终：
            //
            // 现实 1m
            // +
            // XR Origin 额外 1m
            // =
            // VR 中约 2m
            // =================================================

            if (enableMovementAmplification &&
                movementMultiplier > 1f)
            {
                // 把 Tracking Space 的移动方向
                // 转成世界空间方向
                Vector3 worldDelta =
                    xrOrigin.TransformVector(
                        localDelta
                    );


                worldDelta.y = 0f;


                Vector3 extraMovement =
                    worldDelta *
                    (movementMultiplier - 1f);


                // 移动整个 XR Origin
                xrOrigin.position +=
                    extraMovement;


                if (showDebugLog)
                {
                    Debug.Log(
                        "[PlayerMove] 现实移动：" +
                        distance.ToString("F3") +
                        " m | 放大倍数：" +
                        movementMultiplier.ToString("F1") +
                        "x | XR Origin额外移动：" +
                        extraMovement.magnitude.ToString("F3") +
                        " m"
                    );
                }
            }
        }


        // =================================================
        // 保存当前现实头显位置
        // =================================================

        lastPlayerLocalPosition =
            currentLocalPosition;
    }


    // =====================================================
    // 检测老人移动
    // =====================================================

    private void TrackOldmanMovement()
    {
        if (oldmanRoot == null)
            return;


        if (footprintPrefab == null)
            return;


        Vector3 currentPosition =
            oldmanRoot.position;


        // 不计算上下高度
        currentPosition.y = 0f;


        // =================================================
        // 第一次只记录老人位置
        // =================================================

        if (!hasOldmanPosition)
        {
            lastOldmanPosition =
                currentPosition;

            hasOldmanPosition =
                true;


            if (showDebugLog)
            {
                Debug.Log(
                    "[Footprint] 开始记录 Oldman 位置：" +
                    currentPosition
                );
            }


            return;
        }


        float distance =
            Vector3.Distance(
                currentPosition,
                lastOldmanPosition
            );


        // =================================================
        // 老人移动够指定距离
        // 生成一整对脚印
        // =================================================

        if (distance >=
            footprintDistance)
        {
            SpawnFootprintPair();


            lastOldmanPosition =
                currentPosition;
        }
    }


    // =====================================================
    // 生成一整对脚印
    // =====================================================

    private void SpawnFootprintPair()
    {
        if (oldmanRoot == null ||
            footprintPrefab == null)
        {
            return;
        }


        // =================================================
        // 老人当前方向
        // =================================================

        Vector3 forward =
            oldmanRoot.forward;


        forward.y = 0f;


        if (forward.sqrMagnitude <
            0.001f)
        {
            forward =
                Vector3.forward;
        }


        forward.Normalize();


        // =================================================
        // 你的 Footprint Variant
        // 本身就是一整对脚印
        //
        // 所以不再做左右偏移
        // =================================================

        Vector3 targetPosition =
            oldmanRoot.position -
            forward * backOffset;


        // =================================================
        // 从上往下检测地面
        // =================================================

        Vector3 rayStart =
            targetPosition +
            Vector3.up *
            raycastHeight;


        RaycastHit hit;


        bool foundGround =
            Physics.Raycast(
                rayStart,
                Vector3.down,
                out hit,
                raycastDistance,
                groundMask,
                QueryTriggerInteraction.Ignore
            );


        // =================================================
        // 找不到地面
        // =================================================

        if (!foundGround)
        {
            if (showDebugLog)
            {
                Debug.LogWarning(
                    "[Footprint] 没有检测到地面。"
                );
            }


            return;
        }


        // =================================================
        // 脚印位置
        // =================================================

        Vector3 spawnPosition =
            hit.point +
            hit.normal *
            groundOffset;


        // =================================================
        // 脚印方向
        // =================================================

        Vector3 groundForward =
            Vector3.ProjectOnPlane(
                forward,
                hit.normal
            );


        if (groundForward.sqrMagnitude <
            0.001f)
        {
            groundForward =
                Vector3.forward;
        }


        groundForward.Normalize();


        Quaternion directionRotation =
            Quaternion.LookRotation(
                groundForward,
                hit.normal
            );


        Quaternion finalRotation =
            directionRotation *
            Quaternion.Euler(
                footprintRotationOffset
            );


        // =================================================
        // 生成脚印
        // =================================================

        GameObject newFootprint =
            Instantiate(
                footprintPrefab,
                spawnPosition,
                finalRotation
            );


        newFootprint.SetActive(true);


        if (showDebugLog)
        {
            Debug.Log(
                "[Footprint] 成功生成一对脚印：" +
                newFootprint.name
            );
        }
    }


    // =====================================================
    // 玩家移动检测清零
    // =====================================================

    public void ResetPlayerDistance()
    {
        totalRealMoveDistance =
            0f;

        currentRealMoveDistance =
            0f;

        playerIsMoving =
            false;

        hasPlayerPosition =
            false;
    }


    // =====================================================
    // 重置老人脚印
    // =====================================================

    public void ResetFootprints()
    {
        hasOldmanPosition =
            false;
    }
}