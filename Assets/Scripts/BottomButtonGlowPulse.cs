using UnityEngine;

public class BottomButtonGlowPulse : MonoBehaviour
{
    [Header("玩家")]
    public Transform playerRoot;

    [Header("发光外壳")]
    public Renderer glowRenderer;

    [Header("呼吸灯参数")]
    public Color glowColor = Color.cyan;
    public float minIntensity = 0.05f;
    public float maxIntensity = 1.5f;
    public float pulseSpeed = 1f;

    private Material glowMaterial;
    private bool playerNearby = false;

    void Start()
    {
        if (glowRenderer != null)
        {
            glowMaterial = glowRenderer.material;
            glowMaterial.EnableKeyword("_EMISSION");

            // 游戏开始时完全隐藏外壳
            glowRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (!playerNearby || glowMaterial == null)
            return;

        // 平滑：暗 → 亮 → 暗
        float pulse =
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

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

    private void OnTriggerEnter(Collider other)
    {
        if (playerRoot == null)
            return;

        if (other.transform == playerRoot ||
            other.transform.IsChildOf(playerRoot))
        {
            playerNearby = true;

            // 玩家靠近才显示外壳
            if (glowRenderer != null)
            {
                glowRenderer.enabled = true;
            }

            Debug.Log("玩家靠近，按钮外围开始呼吸发光");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerRoot == null)
            return;

        if (other.transform == playerRoot ||
            other.transform.IsChildOf(playerRoot))
        {
            playerNearby = false;

            // 玩家离开直接隐藏外壳
            if (glowRenderer != null)
            {
                glowRenderer.enabled = false;
            }

            Debug.Log("玩家离开，按钮外围隐藏");
        }
    }
    public void StopGlowPermanently()
{
    playerNearby = false;

    if (glowRenderer != null)
    {
        glowRenderer.enabled = false;
    }

    enabled = false;
}
}