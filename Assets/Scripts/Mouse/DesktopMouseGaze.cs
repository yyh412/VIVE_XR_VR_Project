using UnityEngine;

public class DesktopMouseGaze : MonoBehaviour
{
    [Header("Gaze Point UI")]
    public RectTransform gazePoint;

    [Header("Movement")]
    public float gazeSpeed = 8f;

    private Vector2 gazePosition;

    void Start()
    {
        // 初始放在屏幕中心
        gazePosition = new Vector2(
            Screen.width * 0.5f,
            Screen.height * 0.5f
        );

        UpdateGazePoint();
    }

    void Update()
    {
        // 读取鼠标移动量
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // 同一份鼠标移动量控制“眼睛位置”
        gazePosition.x += mouseX * gazeSpeed;
        gazePosition.y += mouseY * gazeSpeed;

        // 不允许跑出屏幕
        gazePosition.x = Mathf.Clamp(
            gazePosition.x,
            0f,
            Screen.width
        );

        gazePosition.y = Mathf.Clamp(
            gazePosition.y,
            0f,
            Screen.height
        );

        UpdateGazePoint();
    }

    void UpdateGazePoint()
    {
        if (gazePoint == null)
            return;

        gazePoint.position = gazePosition;
    }

    // 后面检测 NPC 时会用到这个位置
    public Vector2 GetGazeScreenPosition()
    {
        return gazePosition;
    }
}