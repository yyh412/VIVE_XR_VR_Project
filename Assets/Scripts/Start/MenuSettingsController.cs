using UnityEngine;

public class MenuSettingsController : MonoBehaviour
{
    [Header("主菜单")]
    public GameObject startButton;
    public GameObject settingsButton;
    public GameObject exitButton;

    [Header("设置页面")]
    public GameObject settingsPanel;

    // 点击 OPTIONS
    public void OpenSettings()
    {
        startButton.SetActive(false);
        settingsButton.SetActive(false);
        exitButton.SetActive(false);

        settingsPanel.SetActive(true);
    }

    // 点击 X
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);

        startButton.SetActive(true);
        settingsButton.SetActive(true);
        exitButton.SetActive(true);
    }
}