using UnityEngine;

public class OfficeArrowRoute : MonoBehaviour
{
    [Header("玩家头显")]
    public Transform playerCamera;

    [Header("按路线顺序拖入所有箭头")]
    public GameObject[] arrows;

    [Header("同时亮起几个箭头")]
    public int visibleCount = 3;

    [Header("走到多近算经过")]
    public float passDistance = 1.0f;

    private int currentIndex = 0;

    void Start()
    {
        RefreshArrows();
    }

    void Update()
    {
        if (playerCamera == null)
            return;

        if (arrows == null || arrows.Length == 0)
            return;

        if (currentIndex >= arrows.Length)
            return;

        GameObject currentArrow = arrows[currentIndex];

        if (currentArrow == null)
            return;

        Vector3 playerPos = playerCamera.position;
        Vector3 arrowPos = currentArrow.transform.position;

        // 只计算地面距离，不考虑玩家头部高度
        playerPos.y = 0f;
        arrowPos.y = 0f;

        float distance = Vector3.Distance(playerPos, arrowPos);

        if (distance <= passDistance)
        {
            currentIndex++;

            RefreshArrows();
        }
    }

    void RefreshArrows()
    {
        for (int i = 0; i < arrows.Length; i++)
        {
            if (arrows[i] == null)
                continue;

            bool shouldShow =
                i >= currentIndex &&
                i < currentIndex + visibleCount;

            arrows[i].SetActive(shouldShow);
        }
    }
}