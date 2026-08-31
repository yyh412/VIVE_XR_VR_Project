using System.Collections.Generic;
using UnityEngine;

public class SceneGrayscaleManager : MonoBehaviour
{
    [Header("要处理的场景根物体")]
    public Transform[] sceneRoots;

    [Header("三档灰色材质")]
    public Material grayDark;
    public Material grayMid;
    public Material grayLight;

    [Header("亮度分界")]
    [Range(0f, 1f)]
    public float darkThreshold = 0.33f;

    [Range(0f, 1f)]
    public float lightThreshold = 0.66f;

    [Header("调试")]
    public bool showDebugLog = true;

    private List<Renderer> renderers = new List<Renderer>();
    private Dictionary<Renderer, Material[]> originalMaterials =
        new Dictionary<Renderer, Material[]>();

    void Start()
    {
        CollectRenderers();
        SaveOriginalMaterials();
        ApplyGrayscale();
    }

    void CollectRenderers()
    {
        renderers.Clear();

        foreach (Transform root in sceneRoots)
        {
            if (root == null)
                continue;

            Renderer[] found =
                root.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer r in found)
            {
                if (!renderers.Contains(r))
                    renderers.Add(r);
            }
        }
    }

    void SaveOriginalMaterials()
    {
        originalMaterials.Clear();

        foreach (Renderer r in renderers)
        {
            Material[] mats = r.materials;

            Material[] saved =
                new Material[mats.Length];

            for (int i = 0; i < mats.Length; i++)
            {
                saved[i] = mats[i];
            }

            originalMaterials[r] = saved;
        }
    }

    void ApplyGrayscale()
    {
        foreach (Renderer r in renderers)
        {
            Material[] sourceMats =
                originalMaterials[r];

            Material[] newMats =
                new Material[sourceMats.Length];

            for (int i = 0; i < sourceMats.Length; i++)
            {
                float brightness =
                    GetMaterialBrightness(sourceMats[i]);

                if (brightness < darkThreshold)
                {
                    newMats[i] = grayDark;
                }
                else if (brightness < lightThreshold)
                {
                    newMats[i] = grayMid;
                }
                else
                {
                    newMats[i] = grayLight;
                }

                if (showDebugLog)
                {
                    Debug.Log(
                        r.name +
                        " / " +
                        sourceMats[i].name +
                        " brightness = " +
                        brightness.ToString("F2")
                    );
                }
            }

            r.materials = newMats;
        }
    }

    float GetMaterialBrightness(Material mat)
    {
        if (mat == null)
            return 0.5f;

        Texture texture = null;

        if (mat.HasProperty("_BaseMap"))
        {
            texture = mat.GetTexture("_BaseMap");
        }
        else if (mat.HasProperty("_MainTex"))
        {
            texture = mat.GetTexture("_MainTex");
        }

        Texture2D tex2D =
            texture as Texture2D;

        if (tex2D != null)
        {
            try
            {
                Color[] pixels = tex2D.GetPixels();

                if (pixels.Length > 0)
                {
                    float total = 0f;

                    int step =
                        Mathf.Max(1, pixels.Length / 1000);

                    int count = 0;

                    for (int i = 0; i < pixels.Length; i += step)
                    {
                        Color c = pixels[i];

                        float brightness =
                            0.2126f * c.r +
                            0.7152f * c.g +
                            0.0722f * c.b;

                        total += brightness;
                        count++;
                    }

                    if (count > 0)
                        return total / count;
                }
            }
            catch
            {
                // 贴图不可读时，退回到材质颜色
            }
        }

        if (mat.HasProperty("_BaseColor"))
        {
            Color c = mat.GetColor("_BaseColor");

            return
                0.2126f * c.r +
                0.7152f * c.g +
                0.0722f * c.b;
        }

        if (mat.HasProperty("_Color"))
        {
            Color c = mat.GetColor("_Color");

            return
                0.2126f * c.r +
                0.7152f * c.g +
                0.0722f * c.b;
        }

        return 0.5f;
    }

    public void RestoreOriginalColors()
    {
        foreach (var pair in originalMaterials)
        {
            if (pair.Key != null)
            {
                pair.Key.materials = pair.Value;
            }
        }
    }
}