using UnityEngine;

public class NPCPaperPickup : MonoBehaviour
{
    [Header("Character")]
    public Animator characterAnimator;

    [Header("Paper")]
    public Rigidbody pickupPaper;

    [Tooltip("捡纸动画时使用的抓取点")]
    public Transform leftPaperGrabPoint;

    [Tooltip("Talking 时使用的抓取点")]
    public Transform talkPaperGrabPoint;

    [Header("Animation")]
    public string pickingUpStateName = "Picking Up Paper";
    public string talkingStateName = "Talking";

    [Range(0f, 1f)]
    public float grabAtNormalizedTime = 0.075f;

    [Header("Picking Up Paper Position")]
    public Vector3 paperLocalPosition = Vector3.zero;
    public Vector3 paperLocalEulerAngles = Vector3.zero;

    private bool grabbed = false;
    private bool movedToTalkingPoint = false;

    private void Update()
    {
        if (characterAnimator == null)
            return;

        if (pickupPaper == null)
            return;

        AnimatorStateInfo state =
            characterAnimator.GetCurrentAnimatorStateInfo(0);

        // ==========================================
        // 1. Picking Up Paper
        // ==========================================
        if (
            !grabbed &&
            state.IsName(pickingUpStateName) &&
            state.normalizedTime >= grabAtNormalizedTime
        )
        {
            GrabPaper();
        }

        // ==========================================
        // 2. Talking
        // 进入 Talking 后切换到新的抓取点
        // ==========================================
        if (
            grabbed &&
            !movedToTalkingPoint &&
            state.IsName(talkingStateName)
        )
        {
            MovePaperToTalkingPoint();
        }
    }

    private void GrabPaper()
    {
        if (leftPaperGrabPoint == null)
            return;

        grabbed = true;

        pickupPaper.velocity = Vector3.zero;
        pickupPaper.angularVelocity = Vector3.zero;

        pickupPaper.isKinematic = true;
        pickupPaper.useGravity = false;

        Collider paperCollider =
            pickupPaper.GetComponent<Collider>();

        if (paperCollider != null)
            paperCollider.enabled = false;

        // 绑定到原来的捡纸点
        pickupPaper.transform.SetParent(
            leftPaperGrabPoint,
            false
        );

        // 保留你原来已经调好的捡纸位置
        pickupPaper.transform.localPosition =
            paperLocalPosition;

        pickupPaper.transform.localRotation =
            Quaternion.Euler(
                paperLocalEulerAngles
            );

        Debug.Log("NPC picked up paper");
    }

    private void MovePaperToTalkingPoint()
    {
        if (talkPaperGrabPoint == null)
        {
            Debug.LogWarning(
                "Talk Paper Grab Point 没有设置"
            );

            return;
        }

        movedToTalkingPoint = true;

        // 切换到你刚刚已经调好的 RightPaperTalkPoint
        pickupPaper.transform.SetParent(
            talkPaperGrabPoint,
            false
        );

        // 因为 TalkPoint 本身已经调好了，
        // 所以纸直接归零即可
        pickupPaper.transform.localPosition =
            Vector3.zero;

        pickupPaper.transform.localRotation =
            Quaternion.identity;

        Debug.Log(
            "Paper moved to Talking point"
        );
    }
}