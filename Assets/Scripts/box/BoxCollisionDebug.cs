using UnityEngine;

public class BoxCollisionDebug : MonoBehaviour
{
    private Rigidbody rb;

    private Vector3 lastPosition;

    [Header("检测阈值")]
    public float moveThreshold = 0.01f;
    public float speedThreshold = 0.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        lastPosition = transform.position;

        if (rb == null)
        {
            Debug.LogError(
                "[BOX DEBUG ERROR] " +
                gameObject.name +
                " 没有 Rigidbody！"
            );

            return;
        }

        Debug.LogError(
            "[BOX START] " +
            gameObject.name +
            " | Position=" + transform.position +
            " | Velocity=" + rb.velocity +
            " | AngularVelocity=" + rb.angularVelocity +
            " | IsKinematic=" + rb.isKinematic +
            " | UseGravity=" + rb.useGravity
        );
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        // ==============================
        // 检测箱子是否发生明显移动
        // ==============================

        float movedDistance =
            Vector3.Distance(lastPosition, transform.position);

        if (movedDistance > moveThreshold)
        {
            Debug.LogWarning(
                "[BOX MOVED] " +
                gameObject.name +
                " | Distance=" + movedDistance.ToString("F4") +
                " | From=" + lastPosition +
                " | To=" + transform.position +
                " | Velocity=" + rb.velocity
            );
        }


        // ==============================
        // 检测速度是否过大
        // ==============================

        if (rb.velocity.magnitude > speedThreshold)
        {
            Debug.LogWarning(
                "[BOX HIGH SPEED] " +
                gameObject.name +
                " | Speed=" + rb.velocity.magnitude.ToString("F3") +
                " | Velocity=" + rb.velocity +
                " | Position=" + transform.position
            );
        }


        // ==============================
        // 检测角速度
        // ==============================

        if (rb.angularVelocity.magnitude > speedThreshold)
        {
            Debug.LogWarning(
                "[BOX HIGH ANGULAR SPEED] " +
                gameObject.name +
                " | AngularSpeed=" +
                rb.angularVelocity.magnitude.ToString("F3") +
                " | AngularVelocity=" +
                rb.angularVelocity
            );
        }


        lastPosition = transform.position;
    }


    // =====================================================
    // 第一次发生碰撞
    // =====================================================

    void OnCollisionEnter(Collision collision)
    {
        Debug.LogError(
            "[BOX COLLISION ENTER] " +
            gameObject.name +
            " 撞到了 → " +
            collision.gameObject.name +
            " | 完整路径: " +
            GetFullPath(collision.transform) +
            " | RelativeVelocity=" +
            collision.relativeVelocity +
            " | Impulse=" +
            collision.impulse
        );
    }


    // =====================================================
    // 持续碰撞
    // =====================================================

    void OnCollisionStay(Collision collision)
    {
        Debug.LogError(
            "[BOX TOUCHING] " +
            gameObject.name +
            " 正在碰撞 → " +
            collision.gameObject.name +
            " | 完整路径: " +
            GetFullPath(collision.transform) +
            " | 相对速度=" +
            collision.relativeVelocity.magnitude.ToString("F3") +
            " | Impulse=" +
            collision.impulse.magnitude.ToString("F3")
        );
    }


    // =====================================================
    // 离开碰撞
    // =====================================================

    void OnCollisionExit(Collision collision)
    {
        Debug.Log(
            "[BOX COLLISION EXIT] " +
            gameObject.name +
            " 离开了 → " +
            collision.gameObject.name +
            " | 完整路径: " +
            GetFullPath(collision.transform)
        );
    }


    // =====================================================
    // 进入 Trigger
    // =====================================================

    void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning(
            "[BOX TRIGGER ENTER] " +
            gameObject.name +
            " 进入 Trigger → " +
            other.gameObject.name +
            " | 完整路径: " +
            GetFullPath(other.transform)
        );
    }


    // =====================================================
    // 持续在 Trigger 中
    // =====================================================

    void OnTriggerStay(Collider other)
    {
        Debug.Log(
            "[BOX TRIGGER STAY] " +
            gameObject.name +
            " 正在 Trigger 内 → " +
            other.gameObject.name
        );
    }


    // =====================================================
    // 离开 Trigger
    // =====================================================

    void OnTriggerExit(Collider other)
    {
        Debug.Log(
            "[BOX TRIGGER EXIT] " +
            gameObject.name +
            " 离开 Trigger → " +
            other.gameObject.name
        );
    }


    // =====================================================
    // 获取 Hierarchy 完整路径
    // =====================================================

    private string GetFullPath(Transform target)
    {
        if (target == null)
            return "NULL";

        string path = target.name;

        while (target.parent != null)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }

        return path;
    }
}