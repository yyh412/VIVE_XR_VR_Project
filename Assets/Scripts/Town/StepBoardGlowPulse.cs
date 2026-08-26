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

    [Header("测试")]
    public bool testGlowOnStart = true;

    private Material glowMaterial;
    private bool isGlowing = false;

    void Start()
    {
        if (glowRenderer == null)
        {
            Debug.LogWarning("StepBoardGlowPulse：没有设置 Glow Renderer！");
            return;
        }

        // 创建独立材质实例，避免影响其他使用同一材质的物体
        glowMaterial = glowRenderer.material;

        // 开启 Emission
        glowMaterial.EnableKeyword("_EMISSION");

        // 默认先隐藏外壳
        glowRenderer.enabled = false;

        // 临时测试：游戏开始直接呼吸发光
        if (testGlowOnStart)
        {
            StartGlow();
        }
    }

    void Update()
    {
        if (!isGlowing)
            return;

        if (glowMaterial == null)
            return;

        // 生成 0~1 的循环值
        float pulse =
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

        // 根据呼吸变化计算亮度
        float intensity =
            Mathf.Lerp(
                minIntensity,
                maxIntensity,
                pulse
            );

        // 设置发光颜色
        Color emissionColor =
            glowColor * intensity;

        glowMaterial.SetColor(
            "_EmissionColor",
            emissionColor
        );
    }

    // =========================
    // 开始呼吸发光
    // =========================
    public void StartGlow()
    {
        if (glowRenderer == null)
            return;

        isGlowing = true;

        glowRenderer.enabled = true;

        Debug.Log("脚踏板开始蓝色呼吸发光");
    }

    // =========================
    // 停止呼吸发光
    // =========================
    public void StopGlow()
    {
        isGlowing = false;

        if (glowRenderer != null)
        {
            glowRenderer.enabled = false;
        }

        Debug.Log("脚踏板停止发光");
    }

    // =========================
    // 永久关闭
    // =========================
    public void StopGlowPermanently()
    {
        StopGlow();

        enabled = false;

        Debug.Log("脚踏板发光永久关闭");
    }
}