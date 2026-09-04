using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ControlModeManager : MonoBehaviour
{
    public static ControlModeManager Instance;

    // ======================================================
    // 当前模式
    // true  = Keyboard + Mouse
    // false = VR
    // ======================================================

    [Header("当前控制模式")]
    public bool desktopMode = true;


    // ======================================================
    // Menu按钮文字
    // ======================================================

    [Header("Menu模式按钮文字")]
    [Tooltip("拖入按钮里面的 TMP Text")]
    public TMP_Text modeButtonText;


    // ======================================================
    // PlayerPrefs Key
    // ======================================================

    private const string ModeKey = "ControlMode";


    // ======================================================
    // Awake
    // ======================================================

    private void Awake()
    {
        // 防止切场景后重复出现多个Manager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);


        // ==================================================
        // 读取上一次选择
        //
        // 1 = Keyboard
        // 0 = VR
        //
        // 第一次运行默认 Keyboard
        // ==================================================

        desktopMode =
            PlayerPrefs.GetInt(
                ModeKey,
                1
            ) == 1;


        UpdateButtonText();


        // 每次切Scene以后重新应用模式
        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -=
                OnSceneLoaded;
        }
    }


    // ======================================================
    // Scene加载完成
    // ======================================================

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        // 等Scene加载以后应用当前模式
        ApplyModeToScene();

        UpdateButtonText();
    }


    // ======================================================
    // Menu按钮调用
    // ======================================================

    public void ToggleControlMode()
    {
        desktopMode =
            !desktopMode;


        // 保存选择
        PlayerPrefs.SetInt(
            ModeKey,
            desktopMode ? 1 : 0
        );

        PlayerPrefs.Save();


        ApplyModeToScene();

        UpdateButtonText();


        Debug.Log(
            desktopMode
                ? "控制模式 → Keyboard + Mouse"
                : "控制模式 → VR"
        );
    }


    // ======================================================
    // 更新按钮文字
    // ======================================================

    private void UpdateButtonText()
    {
        if (modeButtonText == null)
            return;


        if (desktopMode)
        {
            modeButtonText.text =
                "Mode: Keyboard";
        }
        else
        {
            modeButtonText.text =
                "Mode: VR";
        }
    }


    // ======================================================
    // 给当前Scene里的脚本统一设置模式
    // ======================================================

    public void ApplyModeToScene()
    {
        // ==================================================
        // Desktop移动
        // ==================================================

        DesktopXRController[] desktopControllers =
            FindObjectsOfType<DesktopXRController>(true);

        foreach (
            DesktopXRController controller
            in desktopControllers)
        {
            controller.desktopMode =
                desktopMode;
        }


        // ==================================================
        // Mansuit E
        // ==================================================

        DesktopHelpShortcut[] mansuitHelpScripts =
            FindObjectsOfType<DesktopHelpShortcut>(true);

        foreach (
            DesktopHelpShortcut help
            in mansuitHelpScripts)
        {
            help.desktopMode =
                desktopMode;
        }


        // ==================================================
        // Driver E
        // ==================================================

        DriverDesktopHelpShortcut[] driverHelpScripts =
            FindObjectsOfType<DriverDesktopHelpShortcut>(true);

        foreach (
            DriverDesktopHelpShortcut help
            in driverHelpScripts)
        {
            help.desktopMode =
                desktopMode;
        }


        // ==================================================
        // Driver眼动 / 鼠标视线
        // ==================================================

        DriverGazeTarget[] driverGazeTargets =
            FindObjectsOfType<DriverGazeTarget>(true);

        foreach (
            DriverGazeTarget gaze
            in driverGazeTargets)
        {
            gaze.desktopMode =
                desktopMode;
        }


        // ==================================================
        // Mansuit眼动 / 鼠标视线
        // ==================================================

        MansuitGazeTarget[] mansuitGazeTargets =
            FindObjectsOfType<MansuitGazeTarget>(true);

        foreach (
            MansuitGazeTarget gaze
            in mansuitGazeTargets)
        {
            gaze.desktopMode =
                desktopMode;
        }


        // ==================================================
        // Oldman眼动 / 鼠标视线
        // ==================================================

        OldmanGazeTarget[] oldmanGazeTargets =
            FindObjectsOfType<OldmanGazeTarget>(true);

        foreach (
            OldmanGazeTarget gaze
            in oldmanGazeTargets)
        {
            gaze.desktopMode =
                desktopMode;
        }


        // ==================================================
        // Desktop鼠标视线Canvas
        //
        // Keyboard模式显示
        // VR模式隐藏
        // ==================================================

        GameObject gazeCanvas =
            GameObject.Find(
                "DesktopGazeCanvas"
            );

        if (gazeCanvas != null)
        {
            gazeCanvas.SetActive(
                desktopMode
            );
        }


        Debug.Log(
            desktopMode
                ? "[ControlMode] 当前Scene = Keyboard"
                : "[ControlMode] 当前Scene = VR"
        );
    }
}