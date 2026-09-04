using UnityEngine;

public class NewGameReset : MonoBehaviour
{
    public void ResetGameTimer()
    {
        // 重置倒计时
        GameCountdown.ResetCountdown();

        // 重置三个帮助记录
        HelpRecord.ResetAll();

        Debug.Log("新游戏：倒计时和帮助记录已重置");
    }
}