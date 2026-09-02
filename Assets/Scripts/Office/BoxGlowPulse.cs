using UnityEngine;

public class BoxGlowPulse : MonoBehaviour
{
    [Header("需要闪烁的 Renderer")]
    public Renderer targetRenderer;

    [Header("呼吸速度")]
    public float pulseSpeed = 2f;

    [Header("亮度范围")]
    public float minIntensity = 0.3f;
    public float maxIntensity = 2.5f;

    [Header("蓝色")]
    public Color glowColor = new Color(0.2f, 0.8f, 1f);

    private Material runtimeMaterial;
    private bool isGlowing = false;

    void Awake()
    {
        if (targetRenderer != null)
        {
            // 创建独立材质实例，避免影响其它物体
            runtimeMaterial = targetRenderer.material;
        }

        StopGlow();
    }

    void Update()
    {
        if (!isGlowing || runtimeMaterial == null)
            return;

        // 0~1 循环
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        Color emissionColor = glowColor * intensity;

        runtimeMaterial.EnableKeyword("_EMISSION");
        runtimeMaterial.SetColor("_EmissionColor", emissionColor);
    }

    public void StartGlow()
    {
        isGlowing = true;

        if (targetRenderer != null)
            targetRenderer.gameObject.SetActive(true);
    }

    public void StopGlow()
    {
        isGlowing = false;

        if (runtimeMaterial != null)
        {
            runtimeMaterial.SetColor("_EmissionColor", Color.black);
        }

        if (targetRenderer != null)
            targetRenderer.gameObject.SetActive(false);
    }
}