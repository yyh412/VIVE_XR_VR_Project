using UnityEngine;
using UnityEngine.EventSystems;

public class DriverGazeTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("注视设置")]
    public float requiredGazeTime = 1f;
    public float lostGazeResetTime = 0.5f;
    public float maxDetectionDistance = 8f;

    [Header("玩家")]
    public Transform playerHead;

    [Header("调试")]
    public bool showDebugLog = true;

    private bool pointerOnNPC = false;
    private bool gazeCompleted = false;

    private float gazeTime = 0f;
    private float lostGazeTime = 0f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (gazeCompleted)
            return;

        pointerOnNPC = true;
        lostGazeTime = 0f;

        if (showDebugLog)
            Debug.Log("[Driver Gaze] 视线进入：" + gameObject.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (gazeCompleted)
            return;

        pointerOnNPC = false;
        lostGazeTime = 0f;

        if (showDebugLog)
            Debug.Log("[Driver Gaze] 视线离开：" + gameObject.name);
    }

    private void Update()
    {
        if (gazeCompleted || playerHead == null)
            return;

        float distance = Vector3.Distance(
            playerHead.position,
            transform.position
        );

        // 超过有效检测距离，不计算注视
        if (distance > maxDetectionDistance)
        {
            ResetGaze();
            return;
        }

        // 正在注视 Driver
        if (pointerOnNPC)
        {
            lostGazeTime = 0f;
            gazeTime += Time.deltaTime;

            if (gazeTime >= requiredGazeTime)
            {
                GazeCompleted();
            }
        }
        // 视线暂时离开
        else if (gazeTime > 0f)
        {
            lostGazeTime += Time.deltaTime;

            // 离开超过 0.5 秒才清零
            if (lostGazeTime >= lostGazeResetTime)
            {
                ResetGaze();
            }
        }
    }

    private void GazeCompleted()
    {
        gazeCompleted = true;
        gazeTime = requiredGazeTime;

        Debug.Log(
            "========== DRIVER GAZE SUCCESS ==========\n" +
            "真正注视到 Driver：" + gameObject.name + "\n" +
            "累计注视时间：" + requiredGazeTime + " 秒"
        );

        // 下一步：
        // Driver 黑白 -> 彩色
    }

    private void ResetGaze()
    {
        if (gazeTime > 0f && showDebugLog)
        {
            Debug.Log("[Driver Gaze] 注视中断超过 0.5 秒，重新计时");
        }

        gazeTime = 0f;
        lostGazeTime = 0f;
        pointerOnNPC = false;
    }
}