using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("退出游戏");

#if UNITY_EDITOR
        // 在 Unity 编辑器测试时停止 Play
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Build 成正式游戏后退出程序
        Application.Quit();
#endif
    }
}