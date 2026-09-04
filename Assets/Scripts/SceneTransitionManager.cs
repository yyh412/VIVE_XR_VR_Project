using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Fade Screen")]
    [Tooltip("需要淡出的场景就拖入 FadeScreen；不需要的场景可以留空")]
    public FadeScreen fadeScreen;


    // ======================================================
    // 按钮 / 其他脚本调用
    // ======================================================

    public void GoToScene(int sceneIndex)
    {
        StartCoroutine(
            GoToSceneRoutine(sceneIndex)
        );
    }


    // ======================================================
    // 切换场景
    // ======================================================

    private IEnumerator GoToSceneRoutine(int sceneIndex)
    {
        // ==================================================
        // 情况1：
        // 有 FadeScreen
        // 而且 FadeScreen 当前是激活状态
        // → 正常淡出
        // ==================================================

        if (
            fadeScreen != null &&
            fadeScreen.gameObject.activeInHierarchy
        )
        {
            Debug.Log(
                "[SceneTransition] FadeOut → 切换场景"
            );

            fadeScreen.FadeOut();

            yield return new WaitForSeconds(
                fadeScreen.fadeDuration
            );
        }

        // ==================================================
        // 情况2：
        // 没有 FadeScreen
        // 或 FadeScreen 当前没有激活
        // → 不淡出，直接切换
        // ==================================================

        else
        {
            Debug.Log(
                "[SceneTransition] 没有可用 FadeScreen → 直接切换场景"
            );
        }


        // ==================================================
        // 加载场景
        // ==================================================

        SceneManager.LoadScene(sceneIndex);
    }
}