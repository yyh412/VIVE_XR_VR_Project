using UnityEngine;

public class CombinedEyeGazeRay : MonoBehaviour
{
    [Header("左右眼 Gaze")]
    public Transform leftGaze;
    public Transform rightGaze;

    [Header("射线设置")]
    public float maxDistance = 100f;
    public LayerMask gazeLayerMask = ~0;

    [Header("忽略玩家自身")]
    [Tooltip("拖入 XR Origin / XR Rig 根物体")]
    public Transform playerRoot;

    [Header("可视化落点")]
    public GameObject gazePoint;
    public bool hidePointWhenNoHit = true;

    [Header("调试")]
    public bool showDebugRay = true;
    public bool showHitLog = true;

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

        // 左右眼中间位置
        GazeOrigin =
            (leftGaze.position + rightGaze.position) * 0.5f;

        // 左右眼平均方向
        Vector3 combinedDirection =
            leftGaze.forward.normalized +
            rightGaze.forward.normalized;

        if (combinedDirection.sqrMagnitude < 0.0001f)
        {
            ClearHit();
            return;
        }

        GazeDirection = combinedDirection.normalized;

        // 用 RaycastAll，因为我们需要跳过玩家自己的 Collider
        RaycastHit[] hits = Physics.RaycastAll(
            GazeOrigin,
            GazeDirection,
            maxDistance,
            gazeLayerMask,
            QueryTriggerInteraction.Ignore
        );

        if (hits == null || hits.Length == 0)
        {
            ClearHit();
            DrawDebug();
            return;
        }

        // 按距离排序
        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        RaycastHit validHit = new RaycastHit();
        bool foundValidHit = false;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i].collider;

            if (col == null)
                continue;

            // 忽略玩家自身
            if (playerRoot != null)
            {
                Transform t = col.transform;

                if (t == playerRoot || t.IsChildOf(playerRoot))
                {
                    continue;
                }
            }

            validHit = hits[i];
            foundValidHit = true;
            break;
        }

        if (!foundValidHit)
        {
            ClearHit();
            DrawDebug();
            return;
        }

        HasHit = true;
        CurrentHit = validHit;

        if (showHitLog)
        {
            Debug.Log(
                "[Eye Gaze Hit] " +
                validHit.collider.name
            );
        }

        // 更新注视点
        if (gazePoint != null)
        {
            if (!gazePoint.activeSelf)
            {
                gazePoint.SetActive(true);
            }

            gazePoint.transform.position =
                validHit.point +
                validHit.normal * 0.005f;
        }

        DrawDebug();
    }

    private void ClearHit()
    {
        HasHit = false;

        if (gazePoint != null && hidePointWhenNoHit)
        {
            gazePoint.SetActive(false);
        }
    }

    private void DrawDebug()
    {
        if (showDebugRay)
        {
            Debug.DrawRay(
                GazeOrigin,
                GazeDirection * maxDistance
            );
        }
    }

    public GameObject GetCurrentObject()
    {
        if (!HasHit || CurrentHit.collider == null)
            return null;

        return CurrentHit.collider.gameObject;
    }
}