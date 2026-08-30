using UnityEngine;

public class BriefcaseGlowPulse : MonoBehaviour
{
    [Header("呼吸速度")]
    public float pulseSpeed = 1.2f;

    [Header("最暗透明度")]
    [Range(0f, 1f)]
    public float minAlpha = 0.08f;

    [Header("最亮透明度")]
    [Range(0f, 1f)]
    public float maxAlpha = 0.35f;

    [Header("Emission 强度")]
    public float minEmission = 0.5f;
    public float maxEmission = 3f;

    private Renderer glowRenderer;
    private Material glowMaterial;

    private Color baseColor;
    private Color emissionBaseColor;

    private void Awake()
    {
        glowRenderer = GetComponent<Renderer>();

        if (glowRenderer == null)
        {
            Debug.LogWarning(
                gameObject.name + " 没有 Renderer"
            );
            return;
        }

        glowMaterial = glowRenderer.material;

        baseColor = glowMaterial.color;

        if (glowMaterial.HasProperty("_EmissionColor"))
        {
            emissionBaseColor =
                glowMaterial.GetColor("_EmissionColor");

            // 如果原本没有设置发光颜色，
            // 默认使用蓝色
            if (
                emissionBaseColor.r <= 0.01f &&
                emissionBaseColor.g <= 0.01f &&
                emissionBaseColor.b <= 0.01f
            )
            {
                emissionBaseColor =
                    new Color(
                        0.1f,
                        0.7f,
                        1f,
                        1f
                    );
            }

            glowMaterial.EnableKeyword(
                "_EMISSION"
            );
        }
    }

    private void Update()
    {
        if (glowMaterial == null)
            return;

        float t =
            (Mathf.Sin(
                Time.time * pulseSpeed
            ) + 1f) / 2f;

        // =========================
        // 透明度呼吸
        // =========================

        float alpha =
            Mathf.Lerp(
                minAlpha,
                maxAlpha,
                t
            );

        Color currentColor = baseColor;

        currentColor.a = alpha;

        glowMaterial.color = currentColor;

        // =========================
        // Emission 呼吸
        // =========================

        if (
            glowMaterial.HasProperty(
                "_EmissionColor"
            )
        )
        {
            float emissionStrength =
                Mathf.Lerp(
                    minEmission,
                    maxEmission,
                    t
                );

            Color finalEmission =
                emissionBaseColor *
                emissionStrength;

            glowMaterial.SetColor(
                "_EmissionColor",
                finalEmission
            );
        }
    }
}