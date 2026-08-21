using UnityEngine;

public class AutoNavigationZone : MonoBehaviour
{
    [Header("Navigation")]
    public VRNavigationPath navigationPath;

    [Header("Detection")]
    public bool showDebugLog = true;

    // 给边缘稍微留一点容错
    public float detectionPadding = 0.15f;

    private BoxCollider zoneCollider;
    private bool playerInside = false;


    private void Awake()
    {
        zoneCollider = GetComponent<BoxCollider>();

        if (zoneCollider == null)
        {
            Debug.LogError(
                gameObject.name +
                ": AutoNavigationZone needs a BoxCollider."
            );
        }
    }


    private void Update()
    {
        if (navigationPath == null)
            return;

        if (navigationPath.player == null)
            return;

        if (zoneCollider == null)
            return;


        Vector3 playerPosition =
            navigationPath.player.position;


        bool inside =
            IsPlayerInsideZone(playerPosition);


        // 玩家刚进入
        if (inside && !playerInside)
        {
            playerInside = true;

            navigationPath.EnterAutoNavigationZone(this);

            if (showDebugLog)
            {
                Debug.Log(
                    "ENTER AutoNavZone: "
                    + gameObject.name
                );
            }
        }


        // 玩家刚离开
        if (!inside && playerInside)
        {
            playerInside = false;

            navigationPath.ExitAutoNavigationZone(this);

            if (showDebugLog)
            {
                Debug.Log(
                    "EXIT AutoNavZone: "
                    + gameObject.name
                );
            }
        }
    }


    private bool IsPlayerInsideZone(Vector3 playerPosition)
    {
        // ClosestPoint 如果返回的点与输入点几乎相同，
        // 表示这个点在 Collider 内部。
        Vector3 closest =
            zoneCollider.ClosestPoint(playerPosition);

        float distance =
            Vector3.Distance(
                closest,
                playerPosition
            );

        return distance <= detectionPadding;
    }


    private void OnDisable()
    {
        if (
            playerInside &&
            navigationPath != null
        )
        {
            navigationPath.ExitAutoNavigationZone(this);
        }

        playerInside = false;
    }
}