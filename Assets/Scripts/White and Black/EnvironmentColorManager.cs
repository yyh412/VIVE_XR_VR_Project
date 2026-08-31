using System.Collections.Generic;
using UnityEngine;

public class EnvironmentColorManager : MonoBehaviour
{
    [Header("环境根物体")]
    [Tooltip("拖入 town")]
    public Transform townRoot;

    [Header("环境初始材质")]
    public Material whiteMaterial;
    public Material grayMaterial;
    public Material darkMaterial;

    [Header("已经标记为灰色的物体")]
    public List<Renderer> grayRenderers =
        new List<Renderer>();

    [Header("已经标记为黑色/深灰的物体")]
    public List<Renderer> darkRenderers =
        new List<Renderer>();

    [Header("调试")]
    public bool showDebugLog = false;


    private Renderer[] allEnvironmentRenderers;

    private Dictionary<Renderer, Material[]> originalMaterials =
        new Dictionary<Renderer, Material[]>();


    private void Start()
    {
        if (townRoot == null)
        {
            Debug.LogError(
                "[EnvironmentColorManager] 没有设置 Town Root。"
            );
            return;
        }

        CollectEnvironmentRenderers();

        SaveOriginalMaterials();

        ApplyInitialColors();

        if (showDebugLog)
        {
            Debug.Log(
                "[EnvironmentColorManager] town 初始化完成，共 " +
                allEnvironmentRenderers.Length +
                " 个 Renderer。"
            );
        }
    }


    // =====================================================
    // 收集 town 下所有 Renderer
    // =====================================================

    private void CollectEnvironmentRenderers()
    {
        allEnvironmentRenderers =
            townRoot.GetComponentsInChildren<Renderer>(true);
    }


    // =====================================================
    // 保存原始彩色材质
    // =====================================================

    private void SaveOriginalMaterials()
    {
        originalMaterials.Clear();

        foreach (Renderer r in allEnvironmentRenderers)
        {
            if (r == null)
                continue;

            Material[] current =
                r.materials;

            Material[] saved =
                new Material[current.Length];

            for (int i = 0; i < current.Length; i++)
            {
                saved[i] = current[i];
            }

            originalMaterials[r] = saved;
        }
    }


    // =====================================================
    // 游戏开始：全部白 → 指定灰 → 指定黑
    // =====================================================

    private void ApplyInitialColors()
    {
        // 全部环境先变白
        foreach (Renderer r in allEnvironmentRenderers)
        {
            if (r == null)
                continue;

            ApplyMaterial(
                r,
                whiteMaterial
            );
        }


        // 灰色
        foreach (Renderer r in grayRenderers)
        {
            if (r == null)
                continue;

            ApplyMaterial(
                r,
                grayMaterial
            );
        }


        // 深灰 / 黑色
        foreach (Renderer r in darkRenderers)
        {
            if (r == null)
                continue;

            ApplyMaterial(
                r,
                darkMaterial
            );
        }
    }


    // =====================================================
    // 给 Renderer 所有材质槽统一换材质
    // =====================================================

    private void ApplyMaterial(
        Renderer target,
        Material material)
    {
        if (target == null ||
            material == null)
            return;

        Material[] oldMaterials =
            target.materials;

        Material[] newMaterials =
            new Material[oldMaterials.Length];

        for (int i = 0;
             i < newMaterials.Length;
             i++)
        {
            newMaterials[i] =
                material;
        }

        target.materials =
            newMaterials;
    }


    // =====================================================
    // Cube 后面会调用：
    // 恢复一个 Renderer 的原始彩色
    // =====================================================

    public void RestoreRenderer(
        Renderer target)
    {
        if (target == null)
            return;

        if (!originalMaterials.ContainsKey(target))
            return;

        target.materials =
            originalMaterials[target];

        if (showDebugLog)
        {
            Debug.Log(
                "[EnvironmentColorManager] 恢复：" +
                target.name
            );
        }
    }


    // =====================================================
    // 恢复一个 GameObject 和所有子物体
    // =====================================================

    public void RestoreObject(
        GameObject targetObject)
    {
        if (targetObject == null)
            return;

        Renderer[] renderers =
            targetObject.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer r in renderers)
        {
            RestoreRenderer(r);
        }
    }


    // =====================================================
    // 调试：整个 town 恢复彩色
    // =====================================================

    public void RestoreAllEnvironment()
    {
        foreach (
            KeyValuePair<Renderer, Material[]> pair
            in originalMaterials)
        {
            if (pair.Key == null)
                continue;

            pair.Key.materials =
                pair.Value;
        }

        Debug.Log(
            "[EnvironmentColorManager] town 全部恢复彩色。"
        );
    }


    // =====================================================
    // 调试：重新变成初始黑白灰
    // =====================================================

    public void ResetEnvironmentColors()
    {
        ApplyInitialColors();

        Debug.Log(
            "[EnvironmentColorManager] town 已恢复初始黑白灰。"
        );
    }
}