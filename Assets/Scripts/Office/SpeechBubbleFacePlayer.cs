using UnityEngine;

public class SpeechBubbleFacePlayer : MonoBehaviour
{
    [Header("玩家 VR 相机")]
    [Tooltip("拖入 XR Origin 下面的 Main Camera")]
    public Transform playerHead;

    [Header("只左右旋转")]
    [Tooltip("建议勾选，避免文本框上下歪")]
    public bool onlyRotateY = true;

    [Header("转向速度")]
    public float turnSpeed = 8f;

    [Header("如果文字背对玩家")]
    [Tooltip("如果运行后文字是反的，就勾这个")]
    public bool flip180 = false;


    private void LateUpdate()
    {
        if (playerHead == null)
            return;


        Vector3 direction =
            playerHead.position -
            transform.position;


        // 只左右转
        if (onlyRotateY)
        {
            direction.y = 0f;
        }


        if (direction.sqrMagnitude <
            0.001f)
        {
            return;
        }


        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );


        // 如果 Canvas 正反方向反了
        if (flip180)
        {
            targetRotation *=
                Quaternion.Euler(
                    0f,
                    180f,
                    0f
                );
        }


        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed *
                Time.deltaTime
            );
    }
}