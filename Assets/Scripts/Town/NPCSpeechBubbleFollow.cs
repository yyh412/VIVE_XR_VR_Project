using UnityEngine;

public class NPCSpeechBubbleFollow : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform head;

    [Header("玩家相机")]
    public Transform playerCamera;

    [Header("头顶偏移")]
    public Vector3 worldOffset = new Vector3(0f, 0.55f, 0f);

    [Header("朝向修正")]
    public Vector3 rotationOffset = new Vector3(0f, 180f, 0f);

    private void LateUpdate()
    {
        if (head == null)
            return;

        // 只跟随头的位置，不继承头骨骼旋转
        transform.position =
            head.position + worldOffset;

        // 始终面向玩家
        if (playerCamera != null)
        {
            Vector3 direction =
                playerCamera.position - transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation =
                    Quaternion.LookRotation(direction);

                transform.rotation =
                    lookRotation *
                    Quaternion.Euler(rotationOffset);
            }
        }
    }
}