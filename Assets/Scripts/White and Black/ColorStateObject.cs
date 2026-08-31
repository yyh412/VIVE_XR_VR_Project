using UnityEngine;

public class ColorStateObject : MonoBehaviour
{
    [Header("三种灰度材质")]
    public Material grayDark;
    public Material grayMid;
    public Material grayLight;

    [Header("手动分配部件")]
    [Tooltip("这些部件游戏开始后变成深灰/黑")]
    public Renderer[] darkRenderers;

    [Tooltip("这些部件游戏开始后变成中灰")]
    public Renderer[] midRenderers;

    [Tooltip("这些部件游戏开始后变成浅灰/白")]
    public Renderer[] lightRenderers;

    [Header("设置")]
    [Tooltip("游戏开始时是否自动变成黑白灰")]
    public bool startGrayscale = true;

    private Renderer[] allRenderers;
    private Material[][] originalMaterials;

    private bool initialized = false;
    private bool isColor = true;

    private void Awake()
    {
        // 找这个物体以及所有子物体的 Renderer
        allRenderers =
            GetComponentsInChildren<Renderer>(true);

        if (allRenderers == null ||
            allRenderers.Length == 0)
        {
            Debug.LogWarning(
                "[ColorStateObject] " +
                gameObject.name +
                " 没有找到 Renderer。"
            );

            return;
        }

        // 第一件事：
        // 保存当前的彩色材质
        SaveOriginalMaterials();

        initialized = true;

        // 然后才变成黑白灰
        if (startGrayscale)
        {
            SetGrayscale();
        }
    }

    // =========================================
    // 保存当前原始彩色材质
    // =========================================

    private void SaveOriginalMaterials()
    {
        originalMaterials =
            new Material[allRenderers.Length][];

        for (int i = 0; i < allRenderers.Length; i++)
        {
            Material[] current =
                allRenderers[i].sharedMaterials;

            originalMaterials[i] =
                new Material[current.Length];

            for (int j = 0; j < current.Length; j++)
            {
                originalMaterials[i][j] =
                    current[j];
            }
        }
    }

    // =========================================
    // 变成黑白灰
    // =========================================

    public void SetGrayscale()
    {
        if (!initialized)
            return;

        ApplyMaterial(
            darkRenderers,
            grayDark
        );

        ApplyMaterial(
            midRenderers,
            grayMid
        );

        ApplyMaterial(
            lightRenderers,
            grayLight
        );

        isColor = false;
    }

    // =========================================
    // 给一组 Renderer 换成指定灰度材质
    // =========================================

    private void ApplyMaterial(
        Renderer[] renderers,
        Material material)
    {
        if (renderers == null ||
            material == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];

            if (r == null)
                continue;

            Material[] current =
                r.sharedMaterials;

            Material[] replacement =
                new Material[current.Length];

            for (int j = 0; j < replacement.Length; j++)
            {
                replacement[j] = material;
            }

            r.sharedMaterials =
                replacement;
        }
    }

    // =========================================
    // 恢复原来的彩色
    // =========================================

    public void RestoreColor()
    {
        if (!initialized)
            return;

        for (int i = 0; i < allRenderers.Length; i++)
        {
            if (allRenderers[i] == null)
                continue;

            allRenderers[i].sharedMaterials =
                originalMaterials[i];
        }

        isColor = true;

        Debug.Log(
            "[ColorStateObject] " +
            gameObject.name +
            " 恢复彩色"
        );
    }

    // =========================================
    // 再次变回黑白灰
    // =========================================

    public void RestoreGrayscale()
    {
        if (!initialized)
            return;

        SetGrayscale();
    }

    public bool IsColor()
    {
        return isColor;
    }
}