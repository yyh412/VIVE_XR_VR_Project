using System.Collections.Generic;
using UnityEngine;

public class MansuitGazeTarget : MonoBehaviour
{
    // ======================================================
    // 控制模式
    // ======================================================

    [Header("控制模式")]
    [Tooltip("勾选 = 电脑鼠标模式；不勾选 = VR真实眼动模式")]
    public bool desktopMode = true;


    // ======================================================
    // VR真实眼动
    // ======================================================

    [Header("VR真实眼动射线")]
    public CombinedEyeGazeRay eyeGazeRay;


    // ======================================================
    // 电脑鼠标视线
    // ======================================================

    [Header("电脑鼠标视线")]
    public Camera desktopCamera;

    public DesktopMouseGaze desktopMouseGaze;

    public float desktopRayDistance = 100f;


    // ======================================================
    // 允许触发的目标
    // ======================================================

    [Header("允许触发的目标")]
    [Tooltip("例如 Mansuit 根物体")]
    public Transform[] gazeTargets;


    // ======================================================
    // 需要恢复彩色的物体
    // ======================================================

    [Header("需要恢复彩色的根物体")]
    public Transform[] colorRoots;


    // ======================================================
    // 黑白灰材质
    // ======================================================

    [Header("默认浅色材质")]
    [Tooltip("所有没有手动指定中灰/深灰的部分使用这个材质")]
    public Material grayLight;

    [Header("中灰材质")]
    public Material grayMid;

    [Header("深灰材质")]
    public Material grayDark;


    [Header("手动指定：中灰部件")]
    public Renderer[] midRenderers;

    [Header("手动指定：深灰部件")]
    public Renderer[] darkRenderers;


    // ======================================================
    // E帮助提示
    // ======================================================

    [Header("电脑帮助提示")]
    [Tooltip("拖入 XR Origin 上的 DesktopHelpShortcut")]
    public DesktopHelpShortcut desktopHelpShortcut;


    // ======================================================
    // 注视设置
    // ======================================================

    [Header("注视设置")]
    [Tooltip("持续看多久后恢复彩色")]
    public float requiredGazeTime = 1.0f;

    [Tooltip("短暂移开视线多少秒以内不清零")]
    public float gazeBreakTolerance = 0.5f;


    // ======================================================
    // 调试
    // ======================================================

    [Header("调试")]
    public bool showDebugLog = false;

    public bool showDesktopRay = true;


    // ======================================================
    // 内部变量
    // ======================================================

    private Renderer[] allColorRenderers;

    private Material[][] originalMaterials;

    private float gazeTimer = 0f;

    private float lookAwayTimer = 0f;

    private bool hasRevealedColor = false;

    private bool initialized = false;


    // ======================================================
    // Start
    // ======================================================

    private void Start()
    {
        // 收集所有需要恢复彩色的 Renderer
        CollectAllColorRenderers();


        if (
            allColorRenderers == null ||
            allColorRenderers.Length == 0
        )
        {
            Debug.LogError(
                "[MansuitGazeTarget] 没找到需要处理的 Renderer。"
            );

            return;
        }


        // 保存原始彩色材质
        SaveOriginalMaterials();


        // 游戏开始先变成黑白灰
        ApplyGrayscale();


        gazeTimer = 0f;

        lookAwayTimer = 0f;

        hasRevealedColor = false;

        initialized = true;


        if (showDebugLog)
        {
            Debug.Log(
                "[MansuitGazeTarget] 初始化完成"
            );
        }
    }


    // ======================================================
    // Update
    // ======================================================

    private void Update()
    {
        if (!initialized)
            return;


        // 已经恢复彩色以后
        // 不再重复检测
        if (hasRevealedColor)
            return;


        bool lookingAtMansuit =
            IsLookingAtEvent();


        // ==================================================
        // 正在看 Mansuit
        // ==================================================

        if (lookingAtMansuit)
        {
            lookAwayTimer = 0f;

            gazeTimer += Time.deltaTime;


            if (showDebugLog)
            {
                Debug.Log(
                    "[MansuitGazeTarget] 注视中：" +
                    gazeTimer.ToString("F2") +
                    " / " +
                    requiredGazeTime.ToString("F2")
                );
            }


            // 达到注视时间
            if (gazeTimer >= requiredGazeTime)
            {
                RevealColor();
            }
        }


        // ==================================================
        // 没有看 Mansuit
        // ==================================================

        else
        {
            lookAwayTimer += Time.deltaTime;


            // 超过容错时间才清零
            if (
                lookAwayTimer >
                gazeBreakTolerance
            )
            {
                gazeTimer = 0f;

                lookAwayTimer = 0f;
            }
        }
    }


    // ======================================================
    // 判断是否正在看事件
    // ======================================================

    private bool IsLookingAtEvent()
    {
        // 电脑模式
        if (desktopMode)
        {
            return IsDesktopLookingAtEvent();
        }


        // VR模式
        return IsVRLookingAtEvent();
    }


    // ======================================================
    // 电脑鼠标视线
    // ======================================================

    private bool IsDesktopLookingAtEvent()
    {
        if (desktopCamera == null)
        {
            if (showDebugLog)
            {
                Debug.LogWarning(
                    "[MansuitGazeTarget] Desktop Camera 没有设置"
                );
            }

            return false;
        }


        if (desktopMouseGaze == null)
        {
            if (showDebugLog)
            {
                Debug.LogWarning(
                    "[MansuitGazeTarget] Desktop Mouse Gaze 没有设置"
                );
            }

            return false;
        }


        // 红点当前屏幕位置
        Vector2 screenPosition =
            desktopMouseGaze.GetGazeScreenPosition();


        // 从 Main Camera
        // 穿过红点位置发射射线
        Ray ray =
            desktopCamera.ScreenPointToRay(
                screenPosition
            );


        if (showDesktopRay)
        {
            Debug.DrawRay(
                ray.origin,
                ray.direction *
                desktopRayDistance,
                Color.green
            );
        }


        RaycastHit hit;


        if (
            !Physics.Raycast(
                ray,
                out hit,
                desktopRayDistance
            )
        )
        {
            return false;
        }


        if (hit.collider == null)
            return false;


        Transform hitTransform =
            hit.collider.transform;


        if (showDebugLog)
        {
            Debug.Log(
                "[Desktop Gaze] 当前看到: "
                + hitTransform.name
            );
        }


        return IsTargetTransform(
            hitTransform
        );
    }


    // ======================================================
    // VR真实眼动
    // ======================================================

    private bool IsVRLookingAtEvent()
    {
        if (eyeGazeRay == null)
            return false;


        if (!eyeGazeRay.HasHit)
            return false;


        Collider hitCollider =
            eyeGazeRay.CurrentHit.collider;


        if (hitCollider == null)
            return false;


        Transform hitTransform =
            hitCollider.transform;


        if (showDebugLog)
        {
            Debug.Log(
                "[VR Eye Gaze] 当前看到: "
                + hitTransform.name
            );
        }


        return IsTargetTransform(
            hitTransform
        );
    }


    // ======================================================
    // 判断射线打中的物体
    // 是否属于允许触发目标
    // ======================================================

    private bool IsTargetTransform(
        Transform hitTransform
    )
    {
        if (hitTransform == null)
            return false;


        if (
            gazeTargets == null ||
            gazeTargets.Length == 0
        )
        {
            return false;
        }


        foreach (
            Transform target
            in gazeTargets
        )
        {
            if (target == null)
                continue;


            // 直接打到目标
            if (hitTransform == target)
            {
                return true;
            }


            // 打到目标的子物体
            if (
                hitTransform.IsChildOf(
                    target
                )
            )
            {
                return true;
            }
        }


        return false;
    }


    // ======================================================
    // 恢复彩色
    // ======================================================

    private void RevealColor()
    {
        if (hasRevealedColor)
            return;


        hasRevealedColor = true;


        // 恢复原始材质
        RestoreOriginalMaterials();


        if (showDebugLog)
        {
            Debug.Log(
                "[MansuitGazeTarget] 注视完成 → Mansuit恢复彩色"
            );
        }


        // ==================================================
        // ★ 新增：
        // Mansuit恢复彩色以后
        // 才允许电脑玩家按 E 帮助
        // ==================================================

        if (
            desktopMode &&
            desktopHelpShortcut != null
        )
        {
            desktopHelpShortcut.ShowHelpHint();


            if (showDebugLog)
            {
                Debug.Log(
                    "[MansuitGazeTarget] Mansuit恢复彩色 → 显示 [E] Help"
                );
            }
        }
    }


    // ======================================================
    // 收集 Renderer
    // ======================================================

    private void CollectAllColorRenderers()
    {
        List<Renderer> rendererList =
            new List<Renderer>();


        if (colorRoots != null)
        {
            foreach (
                Transform root
                in colorRoots
            )
            {
                if (root == null)
                    continue;


                Renderer[] renderers =
                    root.GetComponentsInChildren<
                        Renderer
                    >(true);


                foreach (
                    Renderer renderer
                    in renderers
                )
                {
                    if (
                        renderer != null &&
                        !rendererList.Contains(
                            renderer
                        )
                    )
                    {
                        rendererList.Add(
                            renderer
                        );
                    }
                }
            }
        }


        allColorRenderers =
            rendererList.ToArray();


        if (showDebugLog)
        {
            Debug.Log(
                "[MansuitGazeTarget] 收集 Renderer 数量: "
                + allColorRenderers.Length
            );
        }
    }


    // ======================================================
    // 保存原始材质
    // ======================================================

    private void SaveOriginalMaterials()
    {
        if (allColorRenderers == null)
            return;


        originalMaterials =
            new Material[
                allColorRenderers.Length
            ][];


        for (
            int i = 0;
            i < allColorRenderers.Length;
            i++
        )
        {
            Renderer renderer =
                allColorRenderers[i];


            if (renderer == null)
                continue;


            Material[] materials =
                renderer.materials;


            originalMaterials[i] =
                new Material[
                    materials.Length
                ];


            for (
                int j = 0;
                j < materials.Length;
                j++
            )
            {
                originalMaterials[i][j] =
                    materials[j];
            }
        }
    }


    // ======================================================
    // 应用黑白灰
    // ======================================================

    private void ApplyGrayscale()
    {
        if (allColorRenderers == null)
            return;


        foreach (
            Renderer renderer
            in allColorRenderers
        )
        {
            if (renderer == null)
                continue;


            Material targetMaterial =
                GetGrayMaterialForRenderer(
                    renderer
                );


            if (targetMaterial == null)
                continue;


            Material[] currentMaterials =
                renderer.materials;


            Material[] grayMaterials =
                new Material[
                    currentMaterials.Length
                ];


            for (
                int i = 0;
                i < grayMaterials.Length;
                i++
            )
            {
                grayMaterials[i] =
                    targetMaterial;
            }


            renderer.materials =
                grayMaterials;
        }
    }


    // ======================================================
    // 判断 Renderer 使用哪个灰度材质
    // ======================================================

    private Material GetGrayMaterialForRenderer(
        Renderer renderer
    )
    {
        // 深灰优先
        if (
            IsRendererInArray(
                renderer,
                darkRenderers
            )
        )
        {
            if (grayDark != null)
                return grayDark;
        }


        // 中灰
        if (
            IsRendererInArray(
                renderer,
                midRenderers
            )
        )
        {
            if (grayMid != null)
                return grayMid;
        }


        // 其他全部浅灰/白
        return grayLight;
    }


    // ======================================================
    // Renderer 是否在数组里
    // ======================================================

    private bool IsRendererInArray(
        Renderer targetRenderer,
        Renderer[] rendererArray
    )
    {
        if (
            targetRenderer == null ||
            rendererArray == null
        )
        {
            return false;
        }


        foreach (
            Renderer renderer
            in rendererArray
        )
        {
            if (
                renderer ==
                targetRenderer
            )
            {
                return true;
            }
        }


        return false;
    }


    // ======================================================
    // 恢复原始彩色材质
    // ======================================================

    private void RestoreOriginalMaterials()
    {
        if (
            allColorRenderers == null ||
            originalMaterials == null
        )
        {
            return;
        }


        int count =
            Mathf.Min(
                allColorRenderers.Length,
                originalMaterials.Length
            );


        for (
            int i = 0;
            i < count;
            i++
        )
        {
            Renderer renderer =
                allColorRenderers[i];


            if (renderer == null)
                continue;


            if (
                originalMaterials[i] ==
                null
            )
            {
                continue;
            }


            renderer.materials =
                originalMaterials[i];
        }
    }


    // ======================================================
    // 外部手动重置为灰色
    // ======================================================

    public void ResetToGray()
    {
        if (!initialized)
            return;


        hasRevealedColor = false;

        gazeTimer = 0f;

        lookAwayTimer = 0f;


        ApplyGrayscale();


        if (showDebugLog)
        {
            Debug.Log(
                "[MansuitGazeTarget] 已重新变为黑白灰"
            );
        }
    }


    // ======================================================
    // 查询是否已经恢复彩色
    // ======================================================

    public bool HasRevealedColor()
    {
        return hasRevealedColor;
    }
}