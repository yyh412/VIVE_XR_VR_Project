using UnityEngine;

public class CarHelpManager : MonoBehaviour
{
    public enum CarHelpStage
    {
        WaitingForHelp,        // 0：等玩家进入 HelpTrigger
        Talking,               // 1：Driver 正在说话
        WaitingForPushPoint,   // 2：说完话，等玩家去车后面
        ReadyToPush,           // 3：玩家到车后面，可以开始手部交互
        Pushing,               // 4：正在推车
        Finished               // 5：完成
    }

    [Header("当前流程阶段")]
    public CarHelpStage currentStage = CarHelpStage.WaitingForHelp;

    public bool IsStage(CarHelpStage stage)
    {
        return currentStage == stage;
    }

    public void SetStage(CarHelpStage newStage)
    {
        currentStage = newStage;

        Debug.Log("Car Help Stage → " + currentStage);
    }
}