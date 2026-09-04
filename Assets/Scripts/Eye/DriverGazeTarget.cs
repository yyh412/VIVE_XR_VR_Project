using System.Collections.Generic;
using UnityEngine;

public class DriverGazeTarget : MonoBehaviour
{
    // =====================================================
    // 模式
    // =====================================================

    [Header("控制模式")]
    [Tooltip("勾选 = 电脑鼠标模式；不勾选 = VR真实眼动模式")]
    public bool desktopMode = true;


    // =====================================================
    // VR真实眼动
    // =====================================================

    [Header("VR真实眼动射线")]
    public CombinedEyeGazeRay eyeGazeRay;


    // =====================================================
    // Desktop鼠标视线
    // =====================================================

    [Header("电脑鼠标视线")]
    [Tooltip("拖入 XR Origin > Camera Offset > Main Camera")]
    public Camera desktopCamera;

    [Tooltip("拖入 DesktopGazeCanvas 上的 DesktopMouseGaze")]
    public DesktopMouseGaze desktopMouseGaze;

    [Tooltip("电脑视线最远检测距离")]
    public float desktopRayDistance = 100f;


    // =====================================================
    // Driver流程
    // =====================================================

    [Header("Driver 求助流程")]
    [Tooltip("拖入 HelpTrigger 上面的 DriverHelpTrigger")]
    public DriverHelpTrigger driverHelpTrigger;


    // =====================================================
    // Desktop Driver E
    // =====================================================

    [Header("电脑模式 Driver E 帮助")]
    [Tooltip("拖入 XR Origin 上的 DriverDesktopHelpShortcut")]
    public DriverDesktopHelpShortcut driverDesktopHelpShortcut;


    // =====================================================
    // 注视目标
    // =====================================================

    [Header("允许触发这个事件的目标")]
    [Tooltip("例如 driver3、car、mud")]
    public Transform[] gazeTargets;


    // =====================================================
    // 恢复彩色
    // =====================================================

    [Header("恢复彩色的根物体")]
    [Tooltip("看满1秒后，这些物体一起恢复彩色")]
    public Transform[] colorRoots;


    // =====================================================
    // 灰度材质
    // =====================================================

    [Header("三种灰度材质")]
    public Material grayDark;
    public Material grayMid;
    public Material grayLight;

    [Header("深灰部件")]
    public Renderer[] darkRenderers;

    [Header("中灰部件")]
    public Renderer[] midRenderers;

    [Header("浅灰部件")]
    public Renderer[] lightRenderers;


    // =====================================================
    // 注视设置
    // =====================================================

    [Header("注视设置")]
    public float requiredGazeTime = 1.0f;

    public float gazeBreakTolerance = 0.5f;


    // =====================================================
    // 调试
    // =====================================================

    [Header("调试")]
    public bool showDebugLog = false;

    [Tooltip("在Scene窗口显示电脑模式的视线")]
    public bool showDesktopRay = true;


    // =====================================================
    // 内部变量
    // =====================================================

    private Renderer[] allColorRenderers;
    private Material[][] originalMaterials;

    private float gazeTimer = 0f;
    private float lookAwayTimer = 0f;

    private bool hasRevealedColor = false;
    private bool initialized = false;


    // =====================================================
    // Start
    // =====================================================

    private void Start()
    {
        CollectAllColorRenderers();

        if (allColorRenderers == null ||
            allColorRenderers.Length == 0)
        {
            Debug.LogError(
                "[DriverGazeTarget] 没找到需要恢复颜色的 Renderer。"
            );

            return;
        }

        SaveOriginalMaterials();

        ApplyGrayscale();

        gazeTimer = 0f;
        lookAwayTimer = 0f;

        hasRevealedColor = false;
        initialized = true;
    }


    // =====================================================
    // Update
    // =====================================================

    private void Update()
    {
        if (!initialized)
            return;

        if (hasRevealedColor)
            return;


        bool lookingAtEvent =
            IsLookingAtEvent();


        if (lookingAtEvent)
        {
            lookAwayTimer = 0f;

            gazeTimer += Time.deltaTime;


            if (showDebugLog)
            {
                Debug.Log(
                    "[DriverGazeTarget] 注视事件：" +
                    gazeTimer.ToString("F2") +
                    " / " +
                    requiredGazeTime.ToString("F2")
                );
            }


            if (gazeTimer >= requiredGazeTime)
            {
                RevealColor();
            }
        }
        else
        {
            lookAwayTimer += Time.deltaTime;


            if (lookAwayTimer > gazeBreakTolerance)
            {
                gazeTimer = 0f;
                lookAwayTimer = 0f;
            }
        }
    }


    // =====================================================
    // 判断现在是不是在看 Driver / Car / Mud
    // =====================================================

    private bool IsLookingAtEvent()
    {
        // =================================================
        // Desktop模式
        // =================================================

        if (desktopMode)
        {
            return IsDesktopLookingAtEvent();
        }


        // =================================================
        // VR真实眼动模式
        // =================================================

        return IsVRLookingAtEvent();
    }


    // =====================================================
    // Desktop鼠标视线检测
    // =====================================================

    private bool IsDesktopLookingAtEvent()
    {
        if (desktopCamera == null)
            return false;

        if (desktopMouseGaze == null)
            return false;


        // 获取虚拟鼠标小点在屏幕上的位置
        Vector2 screenPosition =
            desktopMouseGaze.GetGazeScreenPosition();


        // 从Camera穿过这个屏幕位置发射射线
        Ray ray =
            desktopCamera.ScreenPointToRay(
                screenPosition
            );


        // Scene窗口显示射线
        if (showDesktopRay)
        {
            Debug.DrawRay(
                ray.origin,
                ray.direction * desktopRayDistance,
                Color.green
            );
        }


        RaycastHit hit;


        if (!Physics.Raycast(
                ray,
                out hit,
                desktopRayDistance))
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
                "[Desktop Gaze] 当前看到：" +
                hitTransform.name
            );
        }


        return IsTargetTransform(
            hitTransform
        );
    }


    // =====================================================
    // VR真实眼动检测
    // =====================================================

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


        return IsTargetTransform(
            hitTransform
        );
    }


    // =====================================================
    // 判断碰到的物体是不是 gazeTargets
    // =====================================================

    private bool IsTargetTransform(
        Transform hitTransform)
    {
        if (hitTransform == null)
            return false;

        if (gazeTargets == null)
            return false;


        for (int i = 0;
             i < gazeTargets.Length;
             i++)
        {
            Transform target =
                gazeTargets[i];


            if (target == null)
                continue;


            // 直接碰到目标
            if (hitTransform == target)
                return true;


            // 碰到目标下面的Collider
            if (hitTransform.IsChildOf(target))
                return true;
        }


        return false;
    }


    // =====================================================
    // 收集需要恢复彩色的 Renderer
    // =====================================================

    private void CollectAllColorRenderers()
    {
        List<Renderer> rendererList =
            new List<Renderer>();


        if (colorRoots == null)
        {
            allColorRenderers =
                rendererList.ToArray();

            return;
        }


        for (int i = 0;
             i < colorRoots.Length;
             i++)
        {
            Transform root =
                colorRoots[i];


            if (root == null)
                continue;


            Renderer[] foundRenderers =
                root.GetComponentsInChildren<Renderer>(true);


            for (int j = 0;
                 j < foundRenderers.Length;
                 j++)
            {
                Renderer r =
                    foundRenderers[j];


                if (r == null)
                    continue;


                if (!rendererList.Contains(r))
                {
                    rendererList.Add(r);
                }
            }
        }


        allColorRenderers =
            rendererList.ToArray();
    }


    // =====================================================
    // 保存原来的彩色材质
    // =====================================================

    private void SaveOriginalMaterials()
    {
        originalMaterials =
            new Material[allColorRenderers.Length][];


        for (int i = 0;
             i < allColorRenderers.Length;
             i++)
        {
            Material[] materials =
                allColorRenderers[i].materials;


            originalMaterials[i] =
                new Material[materials.Length];


            for (int j = 0;
                 j < materials.Length;
                 j++)
            {
                originalMaterials[i][j] =
                    materials[j];
            }
        }
    }


    // =====================================================
    // 初始变成黑白灰
    // =====================================================

    private void ApplyGrayscale()
    {
        ApplyGrayMaterial(
            darkRenderers,
            grayDark
        );

        ApplyGrayMaterial(
            midRenderers,
            grayMid
        );

        ApplyGrayMaterial(
            lightRenderers,
            grayLight
        );
    }


    private void ApplyGrayMaterial(
        Renderer[] renderers,
        Material grayMaterial)
    {
        if (renderers == null)
            return;

        if (grayMaterial == null)
            return;


        for (int i = 0;
             i < renderers.Length;
             i++)
        {
            Renderer r =
                renderers[i];


            if (r == null)
                continue;


            Material[] currentMaterials =
                r.materials;


            Material[] newMaterials =
                new Material[currentMaterials.Length];


            for (int j = 0;
                 j < newMaterials.Length;
                 j++)
            {
                newMaterials[j] =
                    grayMaterial;
            }


            r.materials =
                newMaterials;
        }
    }


    // =====================================================
    // 注视成功
    // =====================================================

    public void RevealColor()
    {
        if (!initialized)
            return;

        if (hasRevealedColor)
            return;


        hasRevealedColor = true;


        // ==================================================
        // Driver + Car + Mud恢复彩色
        // ==================================================

        for (int i = 0;
             i < allColorRenderers.Length;
             i++)
        {
            if (allColorRenderers[i] == null)
                continue;


            allColorRenderers[i].materials =
                originalMaterials[i];
        }


        Debug.Log(
            "[DriverGazeTarget] Driver + Car + Mud 已恢复彩色。"
        );


        // ==================================================
        // 原来的Driver求助流程
        // ==================================================

        if (driverHelpTrigger != null)
        {
            driverHelpTrigger.TriggerHelp();

            Debug.Log(
                "[DriverGazeTarget] 注视成功 → 启动 Driver Talking。"
            );
        }
        else
        {
            Debug.LogWarning(
                "[DriverGazeTarget] 没有设置 Driver Help Trigger。"
            );
        }


        // ==================================================
        // Desktop模式：
        // Driver恢复彩色以后，解锁Driver自己的E
        // ==================================================

        if (
            desktopMode &&
            driverDesktopHelpShortcut != null
        )
        {
            driverDesktopHelpShortcut.ShowHelpHint();

            Debug.Log(
                "[DriverGazeTarget] Driver恢复彩色 → 解锁Driver的E。"
            );
        }
    }


    // =====================================================
    // 调试：重新变灰
    // =====================================================

    public void ResetToGray()
    {
        if (!initialized)
            return;


        gazeTimer = 0f;
        lookAwayTimer = 0f;
        hasRevealedColor = false;


        ApplyGrayscale();


        Debug.Log(
            "[DriverGazeTarget] Driver + Car + Mud 已重新变成黑白灰。"
        );
    }
}