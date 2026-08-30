using UnityEngine;

public class PaperLightPulse : MonoBehaviour
{
    [Header("呼吸速度")]
    public float pulseSpeed = 1.5f;

    [Header("最低亮度")]
    public float minIntensity = 0.3f;

    [Header("最高亮度")]
    public float maxIntensity = 1.5f;

    private Light glowLight;

    private void Awake()
    {
        glowLight = GetComponent<Light>();
    }

    private void Update()
    {
        if (glowLight == null)
            return;

        float t =
            (Mathf.Sin(
                Time.time * pulseSpeed
            ) + 1f) / 2f;

        glowLight.intensity =
            Mathf.Lerp(
                minIntensity,
                maxIntensity,
                t
            );
    }
}