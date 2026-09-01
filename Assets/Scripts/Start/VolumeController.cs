using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [Header("音量滑块")]
    public Slider volumeSlider;

    private const string VolumeKey = "MasterVolume";

    void Start()
    {
        // 读取之前保存的音量
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 0.8f);

        // 设置全局音量
        AudioListener.volume = savedVolume;

        // 设置滑块位置
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void SetVolume(float value)
    {
        // 修改整个游戏的音量
        AudioListener.volume = value;

        // 保存音量
        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(SetVolume);
        }
    }
}