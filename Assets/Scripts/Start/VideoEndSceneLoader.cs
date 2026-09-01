using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class VideoEndSceneLoader : MonoBehaviour
{
    [Header("视频播放器")]
    public VideoPlayer videoPlayer;

    [Header("场景转换管理器")]
    public SceneTransitionManager sceneTransitionManager;

    [Header("视频结束后进入的场景 Build Index")]
    public int targetSceneIndex = 0;

    [Header("场景加载后延迟")]
    public float startDelay = 1.0f;

    private bool hasTriggered = false;

    private IEnumerator Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoEndSceneLoader：没有拖入 VideoPlayer！");
            yield break;
        }

        videoPlayer.loopPointReached += OnVideoFinished;

        // 先彻底停掉，避免从上一个场景切换后状态异常
        videoPlayer.Stop();

        // 等 XR 和场景初始化完成
        yield return new WaitForSeconds(startDelay);

        Debug.Log("IntroVideo：开始 Prepare");

        videoPlayer.Prepare();

        float timeout = 10f;
        float timer = 0f;

        while (!videoPlayer.isPrepared && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!videoPlayer.isPrepared)
        {
            Debug.LogError("IntroVideo：视频 Prepare 超时！");
            yield break;
        }

        Debug.Log("IntroVideo：Prepare 成功，开始播放");

        // 确保从第一帧开始
        videoPlayer.frame = 0;
        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (hasTriggered)
            return;

        hasTriggered = true;

        Debug.Log("IntroVideo：视频结束，开始切换场景");

        if (sceneTransitionManager != null)
        {
            sceneTransitionManager.GoToScene(targetSceneIndex);
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}