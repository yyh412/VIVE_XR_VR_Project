using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Fade Screen")]
    public FadeScreen fadeScreen;

    // 按钮调用这个函数
    public void GoToScene(int sceneIndex)
    {
        StartCoroutine(GoToSceneRoutine(sceneIndex));
    }

    // 黑屏后切换场景
    private IEnumerator GoToSceneRoutine(int sceneIndex)
    {
        // 开始淡出：透明 → 黑色
        if (fadeScreen != null)
        {
            fadeScreen.FadeOut();

            // 等黑屏动画完成
            yield return new WaitForSeconds(fadeScreen.fadeDuration);
        }

        // 加载目标场景
        SceneManager.LoadScene(sceneIndex);
    }
}