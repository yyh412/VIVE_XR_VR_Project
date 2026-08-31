using UnityEngine;

public class CombinedEyeGazeRay : MonoBehaviour
{
    [Header("测试")]
    public int testNumber = 123;

    [Header("左右眼 Gaze")]
    public Transform leftGaze;
    public Transform rightGaze;

    [Header("射线设置")]
    public float maxDistance = 100f;

    [Tooltip("哪些 Layer 可以被眼睛看到")]
    public LayerMask gazeLayerMask = ~0;

    [Header("可视化落点")]
    public GameObject gazePoint;

    [Tooltip("射线没有碰到物体时是否隐藏落点")]
    public bool hidePointWhenNoHit = true;

    [Header("调试")]
    public bool showDebugRay = true;

    // 当前是否击中物体
    public bool HasHit { get; private set; }

    // 当前击中的信息
    public RaycastHit CurrentHit { get; private set; }

    // 当前视线起点
    public Vector3 GazeOrigin { get; private set; }

    // 当前视线方向
    public Vector3 GazeDirection { get; private set; }


    private void Update()
    {
        // =====================================================
        // 1. 检查左右眼引用
        // =====================================================

        if (leftGaze == null || rightGaze == null)
        {
            HasHit = false;

            if (gazePoint != null && hidePointWhenNoHit)
            {
                gazePoint.SetActive(false);
            }

            return;
        }


        // =====================================================
        // 2. 左右眼中间位置作为射线起点
        // =====================================================

        Vector3 leftPosition = leftGaze.position;
        Vector3 rightPosition = rightGaze.position;

        GazeOrigin = (leftPosition + rightPosition) * 0.5f;


        // =====================================================
        // 3. 左右眼方向取平均
        // =====================================================

        Vector3 leftDirection = leftGaze.forward.normalized;
        Vector3 rightDirection = rightGaze.forward.normalized;

        Vector3 combinedDirection =
            leftDirection + rightDirection;


        // 防止方向异常
        if (combinedDirection.sqrMagnitude < 0.0001f)
        {
            HasHit = false;

            if (gazePoint != null && hidePointWhenNoHit)
            {
                gazePoint.SetActive(false);
            }

            return;
        }


        GazeDirection =
            combinedDirection.normalized;


        // =====================================================
        // 4. 发射眼动射线
        // =====================================================

        RaycastHit hit;

        bool didHit = Physics.Raycast(
            GazeOrigin,
            GazeDirection,
            out hit,
            maxDistance,
            gazeLayerMask,
            QueryTriggerInteraction.Ignore
        );


        if (didHit)
        {
            HasHit = true;
            CurrentHit = hit;


            // =============================================
            // 更新注视点
            // =============================================

            if (gazePoint != null)
            {
                if (!gazePoint.activeSelf)
                {
                    gazePoint.SetActive(true);
                }


                Vector3 pointPosition =
                    hit.point +
                    hit.normal * 0.005f;


                gazePoint.transform.position =
                    pointPosition;


                gazePoint.transform.rotation =
                    Quaternion.LookRotation(hit.normal);
            }
        }
        else
        {
            HasHit = false;


            if (gazePoint != null)
            {
                if (hidePointWhenNoHit)
                {
                    gazePoint.SetActive(false);
                }
                else
                {
                    gazePoint.SetActive(true);

                    gazePoint.transform.position =
                        GazeOrigin +
                        GazeDirection *
                        maxDistance;
                }
            }
        }


        // =====================================================
        // 5. Scene 视图画调试射线
        // =====================================================

        if (showDebugRay)
        {
            Debug.DrawRay(
                GazeOrigin,
                GazeDirection * maxDistance
            );
        }
    }


    // =========================================================
    // 给其他脚本获取当前看到的物体
    // =========================================================

    public GameObject GetCurrentObject()
    {
        if (!HasHit)
        {
            return null;
        }

        if (CurrentHit.collider == null)
        {
            return null;
        }

        return CurrentHit.collider.gameObject;
    }
}