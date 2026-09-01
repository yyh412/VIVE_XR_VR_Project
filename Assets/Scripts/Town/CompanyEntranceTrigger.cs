using UnityEngine;

public class CompanyEntranceTrigger : MonoBehaviour
{
    [Header("场景切换管理器")]
    public SceneTransitionManager transitionManager;

    [Header("Office 场景 Build Index")]
    public int officeSceneIndex = 0;

    [Header("玩家 XR Origin")]
    public Transform xrOrigin;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        if (xrOrigin == null)
        {
            Debug.LogError("CompanyEntranceTrigger：没有拖入 XR Origin！");
            return;
        }

        // 只要进入 Trigger 的物体属于 XR Origin 这一整套，就算玩家进入
        if (other.transform == xrOrigin ||
            other.transform.IsChildOf(xrOrigin))
        {
            hasTriggered = true;

            Debug.Log("CompanyEntranceTrigger：玩家进入公司入口");

            if (transitionManager != null)
            {
                transitionManager.GoToScene(officeSceneIndex);
            }
            else
            {
                Debug.LogError("CompanyEntranceTrigger：没有 Transition Manager！");
            }
        }
    }
}