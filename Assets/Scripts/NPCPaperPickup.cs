using UnityEngine;

public class NPCPaperPickup : MonoBehaviour
{
    [Header("Character")]
    public Animator characterAnimator;

    [Header("Paper")]
    public Rigidbody pickupPaper;
    public Transform leftPaperGrabPoint;

    [Header("Animation")]
    public string pickingUpStateName = "Picking Up Paper";

    // 你的 Blender 中左手最低大约在39帧
    // Unity这个Clip从21帧开始，所以大约是第18帧
    // 18 / 247 ≈ 0.073
    // 先用0.075测试
    [Range(0f, 1f)]
    public float grabAtNormalizedTime = 0.075f;

    [Header("Paper Position In Hand")]
    public Vector3 paperLocalPosition = Vector3.zero;
    public Vector3 paperLocalEulerAngles = Vector3.zero;

    private bool grabbed = false;


    private void Update()
    {
        if (grabbed)
            return;

        if (characterAnimator == null)
            return;

        if (pickupPaper == null)
            return;

        if (leftPaperGrabPoint == null)
            return;


        AnimatorStateInfo state =
            characterAnimator.GetCurrentAnimatorStateInfo(0);


        // 只有正在播放 Picking Up Paper 才检测
        if (!state.IsName(pickingUpStateName))
            return;


        // 动画到手碰纸的位置
        if (state.normalizedTime >= grabAtNormalizedTime)
        {
            GrabPaper();
        }
    }


    private void GrabPaper()
    {
        if (grabbed)
            return;

        grabbed = true;


        // ==========================================
        // 关闭纸的物理
        // ==========================================

        pickupPaper.velocity = Vector3.zero;
        pickupPaper.angularVelocity = Vector3.zero;

        pickupPaper.isKinematic = true;
        pickupPaper.useGravity = false;


        // ==========================================
        // 关闭 Collider
        // 避免拿起来以后撞人物
        // ==========================================

        Collider paperCollider =
            pickupPaper.GetComponent<Collider>();

        if (paperCollider != null)
        {
            paperCollider.enabled = false;
        }


        // ==========================================
        // 绑定到左手
        // ==========================================

        pickupPaper.transform.SetParent(
            leftPaperGrabPoint,
            false
        );


        pickupPaper.transform.localPosition =
            paperLocalPosition;


        pickupPaper.transform.localRotation =
            Quaternion.Euler(
                paperLocalEulerAngles
            );


        Debug.Log(
            "NPC picked up paper"
        );
    }
}