using UnityEngine;
using UnityEngine.SceneManagement;

public class RefreshSceneLighting : MonoBehaviour
{
    void Start()
    {
        DynamicGI.UpdateEnvironment();

        Debug.Log("Scene loaded: " + SceneManager.GetActiveScene().name);
        Debug.Log("Skybox: " + RenderSettings.skybox);
        Debug.Log("Ambient Mode: " + RenderSettings.ambientMode);
        Debug.Log("Ambient Intensity: " + RenderSettings.ambientIntensity);
    }
}