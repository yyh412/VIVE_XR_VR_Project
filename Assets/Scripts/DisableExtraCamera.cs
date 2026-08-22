using UnityEngine;

public class DisableExtraCamera : MonoBehaviour
{
    [Header("切换到此场景后需要关闭的额外 Camera")]
    public Camera cameraToDisable;

    void Start()
    {
        if (cameraToDisable != null)
        {
            cameraToDisable.enabled = false;
            Debug.Log("Disabled extra camera: " + cameraToDisable.name);
        }
    }
}