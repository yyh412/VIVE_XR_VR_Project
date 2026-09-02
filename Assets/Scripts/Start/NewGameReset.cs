using UnityEngine;

public class NewGameReset : MonoBehaviour
{
    public void ResetGameTimer()
    {
        GameCountdown.ResetCountdown();

        Debug.Log("新游戏：倒计时已重置");
    }
}