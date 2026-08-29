using UnityEngine;

public class StepBoardGlowPulse : MonoBehaviour
{
    [Header("脚踏板发光外壳")]
    public Renderer glowRenderer;

    [Header("呼吸灯参数")]
    public Color glowColor = Color.cyan;
    public float minIntensity = 0.05f;
    public float maxIntensity = 1.5f;
    public float pulseSpeed = 1f;

    private Material glowMaterial;
    private bool isGlowing = false;

    void Start()
    {
        if (glowRenderer == null)
        {
            Debug.LogWarning(
                "StepBoardGlowPulse：没有设置 Glow Renderer！"
            );
            return;
        }

        // 独立材质
        glowMaterial =
            glowRenderer.material;

        glowMaterial.EnableKeyword(
            "_EMISSION"
        );

        // 游戏开始不亮
        glowRenderer.enabled = false;
        isGlowing = false;
    }

    void Update()
    {
        if (!isGlowing)
            return;

        if (glowMaterial == null)
            return;

        float pulse =
            (Mathf.Sin(
                Time.time * pulseSpeed
            ) + 1f) / 2f;

        float intensity =
            Mathf.Lerp(
                minIntensity,
                maxIntensity,
                pulse
            );

        glowMaterial.SetColor(
            "_EmissionColor",
            glowColor * intensity
        );
    }

    // ======================================================
    // Driver 上车以后调用
    // ======================================================

    public void StartGlow()
    {
        if (glowRenderer == null)
            return;

        isGlowing = true;

        glowRenderer.enabled = true;

        Debug.Log(
            "Driver已上车 → 脚踏板开始蓝色呼吸"
        );
    }

    // ======================================================
    // 玩家上车以后调用
    // ======================================================

    public void StopGlow()
    {
        isGlowing = false;

        if (glowRenderer != null)
        {
            glowRenderer.enabled = false;
        }

        if (glowMaterial != null)
        {
            glowMaterial.SetColor(
                "_EmissionColor",
                Color.black
            );
        }

        Debug.Log(
            "玩家已上车 → 脚踏板停止发光"
        );
    }
}