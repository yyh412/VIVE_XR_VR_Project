using System.Collections;
using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    [Header("背景音乐")]
    [Tooltip("拖入 BackgroundMusic 上的 Audio Source")]
    public AudioSource musicSource;

    [Header("音量")]
    [Range(0f, 1f)]
    [Tooltip("平时背景音乐音量")]
    public float normalVolume = 0.4f;

    [Range(0f, 1f)]
    [Tooltip("NPC说话时背景音乐音量")]
    public float talkingVolume = 0.08f;

    [Header("淡入淡出")]
    [Tooltip("音量变化需要多少秒")]
    public float fadeDuration = 0.5f;

    private Coroutine fadeCoroutine;


    private void Start()
    {
        if (musicSource != null)
        {
            musicSource.volume = normalVolume;
        }
    }


    // ==================================================
    // NPC开始说话
    // ==================================================

    public void LowerMusic()
    {
        FadeTo(talkingVolume);
    }


    // ==================================================
    // NPC说话结束
    // ==================================================

    public void RestoreMusic()
    {
        FadeTo(normalVolume);
    }


    // ==================================================
    // 开始音量渐变
    // ==================================================

    private void FadeTo(float targetVolume)
    {
        if (musicSource == null)
            return;


        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }


        fadeCoroutine = StartCoroutine(
            FadeVolume(targetVolume)
        );
    }


    // ==================================================
    // 平滑改变音量
    // ==================================================

    private IEnumerator FadeVolume(float targetVolume)
    {
        float startVolume =
            musicSource.volume;

        float time = 0f;


        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    time / fadeDuration
                );


            musicSource.volume =
                Mathf.Lerp(
                    startVolume,
                    targetVolume,
                    t
                );


            yield return null;
        }


        musicSource.volume =
            targetVolume;

        fadeCoroutine =
            null;
    }
}