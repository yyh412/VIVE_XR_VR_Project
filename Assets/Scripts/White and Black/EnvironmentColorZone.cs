using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentColorZone : MonoBehaviour
{
    [Header("环境颜色管理器")]
    public EnvironmentColorManager environmentColorManager;


    [Header("当前恢复区域")]
    [Tooltip("拖入这个物体自己的 Box Collider")]
    public BoxCollider zoneCollider;


    [Header("Town")]
    [Tooltip("拖入 town 根物体")]
    public Transform townRoot;


    [Header("玩家")]
    [Tooltip("拖入 XR Origin / XR Rig")]
    public Transform playerTransform;


    [Header("排除区域")]
    [Tooltip("这些 Cube 范围内的环境不会被当前 Zone 恢复彩色")]
    public BoxCollider[] excludedZones;


    [Header("颜色扩散设置")]
    [Tooltip("整个颜色扩散持续多少秒")]
    public float spreadDuration = 4f;

    [Tooltip("每次检测恢复之间的最小间隔")]
    public float minimumStepDelay = 0.02f;


    [Header("调试")]
    public bool showDebugLog = false;


    private bool hasRestored = false;
    private bool isRestoring = false;


    // =====================================================
    // 外部调用
    // Push Stop / Thank You 完成以后调用这里
    // =====================================================

    public void RestoreColorInZone()
    {
        if (hasRestored)
            return;

        if (isRestoring)
            return;


        if (environmentColorManager == null)
        {
            Debug.LogError(
                "[EnvironmentColorZone] 没有设置 Environment Color Manager。"
            );

            return;
        }


        if (zoneCollider == null)
        {
            Debug.LogError(
                "[EnvironmentColorZone] 没有设置 Zone Collider。"
            );

            return;
        }


        if (townRoot == null)
        {
            Debug.LogError(
                "[EnvironmentColorZone] 没有设置 Town Root。"
            );

            return;
        }


        if (playerTransform == null)
        {
            Debug.LogError(
                "[EnvironmentColorZone] 没有设置 Player Transform。"
            );

            return;
        }


        StartCoroutine(
            RestoreColorSpread()
        );
    }


    // =====================================================
    // 从玩家向外扩散恢复颜色
    // =====================================================

    private IEnumerator RestoreColorSpread()
    {
        isRestoring = true;


        Renderer[] allRenderers =
            townRoot.GetComponentsInChildren<Renderer>(true);


        List<RendererDistance> targets =
            new List<RendererDistance>();


        Vector3 playerPosition =
            playerTransform.position;


        // =================================================
        // 1. 找出当前 Cube 范围里的所有 Renderer
        // =================================================

        for (int i = 0;
             i < allRenderers.Length;
             i++)
        {
            Renderer r =
                allRenderers[i];


            if (r == null)
                continue;


            // ---------------------------------------------
            // 当前 Renderer 是否与当前恢复 Cube 相交
            // ---------------------------------------------

            if (!zoneCollider.bounds.Intersects(r.bounds))
                continue;


            // ---------------------------------------------
            // Renderer 中心是否位于排除区域
            // ---------------------------------------------

            Vector3 rendererCenter =
                r.bounds.center;


            if (IsInsideExcludedZone(rendererCenter))
            {
                if (showDebugLog)
                {
                    Debug.Log(
                        "[EnvironmentColorZone] 跳过排除区域：" +
                        r.name
                    );
                }

                continue;
            }


            // ---------------------------------------------
            // 计算与玩家距离
            // ---------------------------------------------

            float distance =
                Vector3.Distance(
                    playerPosition,
                    rendererCenter
                );


            RendererDistance item =
                new RendererDistance();

            item.renderer =
                r;

            item.distance =
                distance;


            targets.Add(
                item
            );
        }


        // =================================================
        // 2. 距离排序
        // 最近的先恢复
        // =================================================

        targets.Sort(
            (a, b) =>
                a.distance.CompareTo(b.distance)
        );


        if (targets.Count == 0)
        {
            hasRestored = true;
            isRestoring = false;


            if (showDebugLog)
            {
                Debug.Log(
                    "[EnvironmentColorZone] 当前区域没有可以恢复的环境。"
                );
            }


            yield break;
        }


        // =================================================
        // 3. 找最远距离
        // =================================================

        float maxDistance =
            targets[
                targets.Count - 1
            ].distance;


        // 防止 maxDistance = 0
        if (maxDistance < 0.001f)
        {
            maxDistance =
                0.001f;
        }


        // 防止 Duration 设置为0
        float duration =
            Mathf.Max(
                0.01f,
                spreadDuration
            );


        float startTime =
            Time.time;


        int currentIndex =
            0;


        // =================================================
        // 4. 从玩家位置向外扩散
        // =================================================

        while (
            currentIndex <
            targets.Count
        )
        {
            float elapsed =
                Time.time -
                startTime;


            float progress =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );


            float currentDistance =
                maxDistance *
                progress;


            // ---------------------------------------------
            // 当前扩散半径以内的 Renderer 全部恢复
            // ---------------------------------------------

            while (
                currentIndex <
                    targets.Count &&
                targets[currentIndex].distance <=
                    currentDistance
            )
            {
                Renderer targetRenderer =
                    targets[currentIndex].renderer;


                if (targetRenderer != null)
                {
                    environmentColorManager.RestoreRenderer(
                        targetRenderer
                    );


                    if (showDebugLog)
                    {
                        Debug.Log(
                            "[环境颜色扩散] " +
                            targetRenderer.name +
                            "  距离：" +
                            targets[currentIndex]
                                .distance
                                .ToString("F2")
                        );
                    }
                }


                currentIndex++;
            }


            // ---------------------------------------------
            // 已经全部恢复
            // ---------------------------------------------

            if (
                currentIndex >=
                targets.Count
            )
            {
                break;
            }


            yield return new WaitForSeconds(
                Mathf.Max(
                    0.001f,
                    minimumStepDelay
                )
            );
        }


        // =================================================
        // 5. 防止最后因为时间误差漏一个
        // =================================================

        while (
            currentIndex <
            targets.Count
        )
        {
            Renderer targetRenderer =
                targets[currentIndex].renderer;


            if (targetRenderer != null)
            {
                environmentColorManager.RestoreRenderer(
                    targetRenderer
                );
            }


            currentIndex++;
        }


        hasRestored =
            true;

        isRestoring =
            false;


        Debug.Log(
            "[EnvironmentColorZone] 环境颜色扩散完成。"
        );
    }


    // =====================================================
    // 判断一个点是否位于排除区域
    // =====================================================

    private bool IsInsideExcludedZone(
        Vector3 worldPoint)
    {
        if (excludedZones == null)
            return false;


        for (int i = 0;
             i < excludedZones.Length;
             i++)
        {
            BoxCollider excluded =
                excludedZones[i];


            if (excluded == null)
                continue;


            if (IsPointInsideBoxCollider(
                excluded,
                worldPoint))
            {
                return true;
            }
        }


        return false;
    }


    // =====================================================
    // 判断世界坐标点是否在 BoxCollider 内
    //
    // 不只用 bounds.Contains，
    // 所以 Cube 有旋转时也可以正确判断
    // =====================================================

    private bool IsPointInsideBoxCollider(
        BoxCollider box,
        Vector3 worldPoint)
    {
        if (box == null)
            return false;


        Transform boxTransform =
            box.transform;


        // 世界坐标转成 BoxCollider 本地坐标
        Vector3 localPoint =
            boxTransform.InverseTransformPoint(
                worldPoint
            );


        Vector3 localCenter =
            box.center;


        Vector3 halfSize =
            box.size * 0.5f;


        Vector3 difference =
            localPoint -
            localCenter;


        bool insideX =
            Mathf.Abs(difference.x) <=
            halfSize.x;


        bool insideY =
            Mathf.Abs(difference.y) <=
            halfSize.y;


        bool insideZ =
            Mathf.Abs(difference.z) <=
            halfSize.z;


        return
            insideX &&
            insideY &&
            insideZ;
    }


    // =====================================================
    // 重置
    // =====================================================

    public void ResetZone()
    {
        StopAllCoroutines();

        hasRestored =
            false;

        isRestoring =
            false;
    }


    // =====================================================
    // 内部数据
    // =====================================================

    private class RendererDistance
    {
        public Renderer renderer;

        public float distance;
    }
}