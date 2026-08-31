using UnityEngine;

public class CombinedEyeGazeRay : MonoBehaviour
{
    [Header("左右眼 Gaze")]
    public Transform leftGaze;
    public Transform rightGaze;

    [Header("最大检测距离")]
    public float maxDistance = 100f;

    [Header("近 / 中 / 远距离容错")]
    [Tooltip("0-3米的眼动容错")]
    public float nearRadius = 0.06f;

    [Tooltip("3-7米的眼动容错")]
    public float middleRadius = 0.15f;

    [Tooltip("7米以后的眼动容错")]
    public float farRadius = 0.35f;

    [Tooltip("近距离结束位置")]
    public float nearDistance = 3f;

    [Tooltip("中距离结束位置")]
    public float middleDistance = 7f;

    [Header("Layer")]
    public LayerMask gazeLayerMask = ~0;

    [Header("忽略玩家自身")]
    public Transform playerRoot;

    [Header("眼动落点")]
    public GameObject gazePoint;

    public bool hidePointWhenNoHit = true;

    [Header("调试")]
    public bool showDebugRay = true;
    public bool showHitLog = false;


    public bool HasHit { get; private set; }

    public RaycastHit CurrentHit { get; private set; }

    public Vector3 GazeOrigin { get; private set; }

    public Vector3 GazeDirection { get; private set; }


    private void Update()
    {
        if (leftGaze == null || rightGaze == null)
        {
            ClearHit();
            return;
        }


        // ============================================
        // 1. 左右眼中间作为射线起点
        // ============================================

        GazeOrigin =
            (leftGaze.position +
             rightGaze.position) * 0.5f;


        // ============================================
        // 2. 合成真实双眼注视方向
        // ============================================

        Vector3 combinedDirection =
            leftGaze.forward.normalized +
            rightGaze.forward.normalized;


        if (combinedDirection.sqrMagnitude < 0.0001f)
        {
            ClearHit();
            return;
        }


        GazeDirection =
            combinedDirection.normalized;


        RaycastHit hit;


        // ============================================
        // 3. 第一优先：
        // 精确的真实眼动中心射线
        // ============================================

        if (TryRaycast(out hit))
        {
            SetHit(hit);
            DrawDebug();
            return;
        }


        // ============================================
        // 4. 近距离
        // ============================================

        if (TrySphereCastSection(
            0f,
            nearDistance,
            nearRadius,
            out hit))
        {
            SetHit(hit);
            DrawDebug();
            return;
        }


        // ============================================
        // 5. 中距离
        // ============================================

        if (TrySphereCastSection(
            nearDistance,
            middleDistance,
            middleRadius,
            out hit))
        {
            SetHit(hit);
            DrawDebug();
            return;
        }


        // ============================================
        // 6. 远距离
        // ============================================

        if (TrySphereCastSection(
            middleDistance,
            maxDistance,
            farRadius,
            out hit))
        {
            SetHit(hit);
            DrawDebug();
            return;
        }


        ClearHit();
        DrawDebug();
    }


    // ================================================
    // 精确 Raycast
    // ================================================

    private bool TryRaycast(out RaycastHit validHit)
    {
        RaycastHit[] hits =
            Physics.RaycastAll(
                GazeOrigin,
                GazeDirection,
                maxDistance,
                gazeLayerMask,
                QueryTriggerInteraction.Ignore
            );


        return FindValidHit(
            hits,
            out validHit
        );
    }


    // ================================================
    // 某一距离区间 SphereCast
    // ================================================

    private bool TrySphereCastSection(
        float startDistance,
        float endDistance,
        float radius,
        out RaycastHit validHit)
    {
        validHit =
            new RaycastHit();


        if (endDistance <= startDistance)
            return false;


        Vector3 sectionOrigin =
            GazeOrigin +
            GazeDirection * startDistance;


        float sectionLength =
            endDistance -
            startDistance;


        RaycastHit[] hits =
            Physics.SphereCastAll(
                sectionOrigin,
                radius,
                GazeDirection,
                sectionLength,
                gazeLayerMask,
                QueryTriggerInteraction.Ignore
            );


        return FindValidHit(
            hits,
            out validHit
        );
    }


    // ================================================
    // 从命中列表中找最近有效物体
    // ================================================

    private bool FindValidHit(
        RaycastHit[] hits,
        out RaycastHit validHit)
    {
        validHit =
            new RaycastHit();


        if (hits == null ||
            hits.Length == 0)
        {
            return false;
        }


        System.Array.Sort(
            hits,
            (a, b) =>
                a.distance.CompareTo(b.distance)
        );


        for (int i = 0;
             i < hits.Length;
             i++)
        {
            Collider col =
                hits[i].collider;


            if (col == null)
                continue;


            // 忽略玩家自己
            if (playerRoot != null)
            {
                Transform t =
                    col.transform;


                if (t == playerRoot ||
                    t.IsChildOf(playerRoot))
                {
                    continue;
                }
            }


            // 忽略眼动显示小球本身
            if (gazePoint != null)
            {
                Transform t =
                    col.transform;


                if (t == gazePoint.transform ||
                    t.IsChildOf(gazePoint.transform))
                {
                    continue;
                }
            }


            validHit =
                hits[i];

            return true;
        }


        return false;
    }


    // ================================================
    // 保存当前命中
    // ================================================

    private void SetHit(
        RaycastHit hit)
    {
        HasHit =
            true;


        CurrentHit =
            hit;


        if (showHitLog &&
            hit.collider != null)
        {
            Debug.Log(
                "[Eye Gaze Hit] " +
                hit.collider.name +
                "  Distance: " +
                Vector3.Distance(
                    GazeOrigin,
                    hit.point
                ).ToString("F2")
            );
        }


        // 可视化落点
        if (gazePoint != null)
        {
            if (!gazePoint.activeSelf)
            {
                gazePoint.SetActive(true);
            }


            gazePoint.transform.position =
                hit.point +
                hit.normal * 0.005f;
        }
    }


    // ================================================
    // 清空
    // ================================================

    private void ClearHit()
    {
        HasHit =
            false;


        if (gazePoint != null &&
            hidePointWhenNoHit)
        {
            gazePoint.SetActive(false);
        }
    }


    // ================================================
    // Debug
    // ================================================

    private void DrawDebug()
    {
        if (!showDebugRay)
            return;


        Debug.DrawRay(
            GazeOrigin,
            GazeDirection * maxDistance
        );
    }


    public GameObject GetCurrentObject()
    {
        if (!HasHit ||
            CurrentHit.collider == null)
        {
            return null;
        }


        return CurrentHit.collider.gameObject;
    }
}