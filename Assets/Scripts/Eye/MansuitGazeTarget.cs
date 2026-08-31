using System.Collections.Generic;
using UnityEngine;

public class MansuitGazeTarget : MonoBehaviour
{
    [Header("真实眼动射线")]
    public CombinedEyeGazeRay eyeGazeRay;


    [Header("允许触发这个事件的目标")]
    [Tooltip("例如 Mansuit 本体，也可以以后加入公文包、纸张等")]
    public Transform[] gazeTargets;


    [Header("恢复彩色的根物体")]
    [Tooltip("看满1秒后，哪些物体一起恢复原来的彩色")]
    public Transform[] colorRoots;


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


    [Header("注视设置")]
    [Tooltip("持续注视多久后恢复彩色")]
    public float requiredGazeTime = 1.0f;

    [Tooltip("允许短暂移开视线的最长时间")]
    public float gazeBreakTolerance = 0.5f;


    [Header("调试")]
    public bool showDebugLog = false;


    private Renderer[] allColorRenderers;
    private Material[][] originalMaterials;

    private float gazeTimer = 0f;
    private float lookAwayTimer = 0f;

    private bool hasRevealedColor = false;
    private bool initialized = false;


    private void Start()
    {
        // 收集所有需要恢复颜色的 Renderer
        CollectAllColorRenderers();


        if (allColorRenderers == null ||
            allColorRenderers.Length == 0)
        {
            Debug.LogError(
                "[MansuitGazeTarget] 没找到需要恢复颜色的 Renderer。"
            );

            return;
        }


        // 先保存原来的彩色材质
        SaveOriginalMaterials();


        // 再应用黑白灰材质
        ApplyGrayscale();


        gazeTimer = 0f;
        lookAwayTimer = 0f;

        hasRevealedColor = false;
        initialized = true;
    }


    private void Update()
    {
        if (!initialized)
            return;

        if (hasRevealedColor)
            return;

        if (eyeGazeRay == null)
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
                    "[MansuitGazeTarget] 注视：" +
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
    // 判断眼睛有没有看指定目标
    // =====================================================

    private bool IsLookingAtEvent()
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


            // 直接打中根物体
            if (hitTransform == target)
                return true;


            // 打中目标的子物体
            if (hitTransform.IsChildOf(target))
                return true;
        }


        return false;
    }


    // =====================================================
    // 收集所有需要恢复彩色的 Renderer
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
    // 游戏开始时变成黑白灰
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


    // =====================================================
    // 给指定 Renderer 换灰度材质
    // =====================================================

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
    // 恢复彩色
    // =====================================================

    public void RevealColor()
    {
        if (!initialized)
            return;

        if (hasRevealedColor)
            return;


        hasRevealedColor = true;


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
            "[MansuitGazeTarget] Mansuit 已恢复彩色。"
        );
    }


    // =====================================================
    // 调试：重新变回黑白灰
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
            "[MansuitGazeTarget] 已重新变成黑白灰。"
        );
    }
}