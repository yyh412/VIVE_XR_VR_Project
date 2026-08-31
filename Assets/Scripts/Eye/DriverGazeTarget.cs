using UnityEngine;

public class DriverGazeTarget : MonoBehaviour
{
    [Header("真实眼动射线")]
    public CombinedEyeGazeRay eyeGazeRay;

    [Header("Driver 根物体")]
    [Tooltip("拖入整个 driver3 根物体")]
    public Transform targetRoot;

    [Header("纯白材质")]
    [Tooltip("拖入 NPC_White 材质")]
    public Material whiteMaterial;

    [Header("注视设置")]
    [Tooltip("累计注视多少秒后恢复彩色")]
    public float requiredGazeTime = 1.0f;

    [Tooltip("允许短暂移开视线的最长时间")]
    public float gazeBreakTolerance = 0.5f;

    [Header("调试")]
    public bool showDebugLog = false;

    private Renderer[] targetRenderers;
    private Material[][] originalMaterials;

    private float gazeTimer = 0f;
    private float lookAwayTimer = 0f;

    private bool hasRevealedColor = false;
    private bool initialized = false;

    private void Start()
    {
        if (targetRoot == null)
            targetRoot = transform;

        targetRenderers =
            targetRoot.GetComponentsInChildren<Renderer>(true);

        if (targetRenderers == null ||
            targetRenderers.Length == 0)
        {
            Debug.LogError(
                "[DriverGazeTarget] 没找到 Driver 的 Renderer。"
            );
            return;
        }

        if (whiteMaterial == null)
        {
            Debug.LogError(
                "[DriverGazeTarget] 没有设置 NPC_White 材质。"
            );
            return;
        }

        SaveOriginalMaterials();
        ApplyWhiteMaterial();

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

        bool lookingAtDriver = IsLookingAtDriver();

        if (lookingAtDriver)
        {
            lookAwayTimer = 0f;
            gazeTimer += Time.deltaTime;

            if (showDebugLog)
            {
                Debug.Log(
                    "[DriverGazeTarget] 注视：" +
                    gazeTimer.ToString("F2") +
                    " / " +
                    requiredGazeTime.ToString("F2")
                );
            }

            if (gazeTimer >= requiredGazeTime)
                RevealColor();
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

    private bool IsLookingAtDriver()
    {
        if (!eyeGazeRay.HasHit)
            return false;

        Collider hitCollider =
            eyeGazeRay.CurrentHit.collider;

        if (hitCollider == null)
            return false;

        Transform hitTransform =
            hitCollider.transform;

        if (hitTransform == targetRoot)
            return true;

        if (hitTransform.IsChildOf(targetRoot))
            return true;

        return false;
    }

    private void SaveOriginalMaterials()
    {
        originalMaterials =
            new Material[targetRenderers.Length][];

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Material[] materials =
                targetRenderers[i].materials;

            originalMaterials[i] =
                new Material[materials.Length];

            for (int j = 0; j < materials.Length; j++)
                originalMaterials[i][j] = materials[j];
        }
    }

    private void ApplyWhiteMaterial()
    {
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Material[] currentMaterials =
                targetRenderers[i].materials;

            Material[] whiteMaterials =
                new Material[currentMaterials.Length];

            for (int j = 0; j < whiteMaterials.Length; j++)
                whiteMaterials[j] = whiteMaterial;

            targetRenderers[i].materials =
                whiteMaterials;
        }
    }

    private void RevealColor()
    {
        if (hasRevealedColor)
            return;

        hasRevealedColor = true;

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            targetRenderers[i].materials =
                originalMaterials[i];

        }

        Debug.Log(
            "[DriverGazeTarget] 注视完成，Driver 恢复彩色。"
        );
    }

    public void ResetToWhite()
    {
        if (!initialized)
            return;

        gazeTimer = 0f;
        lookAwayTimer = 0f;
        hasRevealedColor = false;

        ApplyWhiteMaterial();
    }
}