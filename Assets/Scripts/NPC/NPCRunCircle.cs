using UnityEngine;

public class NPCRunCircle : MonoBehaviour
{
    [Header("圆心")]
    public Transform circleCenter;

    [Header("跑圈半径")]
    public float radius = 6f;

    [Header("跑步速度")]
    public float speed = 3f;

    [Header("顺时针")]
    public bool clockwise = true;

    [Header("保持当前Y高度")]
    public bool keepCurrentHeight = true;

    private float angle;
    private float fixedY;

    void Start()
    {
        if (circleCenter == null)
        {
            Debug.LogError("NPCRunCircle：没有设置 Circle Center！");
            enabled = false;
            return;
        }

        fixedY = transform.position.y;

        // 根据人物当前所在位置计算起始角度
        Vector3 offset = transform.position - circleCenter.position;

        angle = Mathf.Atan2(offset.z, offset.x);
    }

    void Update()
    {
        if (circleCenter == null)
            return;

        float direction = clockwise ? -1f : 1f;

        // 角速度 = 线速度 / 半径
        angle += direction * (speed / Mathf.Max(radius, 0.01f)) * Time.deltaTime;

        Vector3 targetPosition = new Vector3(
            circleCenter.position.x + Mathf.Cos(angle) * radius,
            keepCurrentHeight ? fixedY : circleCenter.position.y,
            circleCenter.position.z + Mathf.Sin(angle) * radius
        );

        Vector3 moveDirection = targetPosition - transform.position;

        // 人物朝向跑步方向
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Vector3 flatDirection = new Vector3(
                moveDirection.x,
                0f,
                moveDirection.z
            );

            if (flatDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(flatDirection),
                    10f * Time.deltaTime
                );
            }
        }

        transform.position = targetPosition;
    }
}