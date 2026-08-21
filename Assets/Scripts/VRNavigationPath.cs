using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class VRNavigationPath : MonoBehaviour
{
    // =========================================================
    // References
    // =========================================================

    [Header("References")]

    public Transform player;

    public Transform destination;

    public GameObject arrowPrefab;


    // =========================================================
    // Fixed Navigation
    // =========================================================

    [Header("Fixed Navigation")]

    [Tooltip("手动摆放的固定箭头最多显示几个")]
    public int maxVisibleFixedArrows = 3;

    [Tooltip("玩家距离当前箭头多近时认为已经经过")]
    public float passedArrowDistance = 1.5f;

    [Tooltip("岔路切换防抖，新路线需要明显更近才切换")]
    public float branchSwitchAdvantage = 1.0f;


    // =========================================================
    // Auto Navigation
    // =========================================================

    [Header("Auto Navigation")]

    [Tooltip("自动导航只显示2个箭头")]
    public int maxAutoArrows = 2;

    [Tooltip("第一个自动箭头沿路线距离玩家多远")]
    public float autoArrowDistance1 = 4f;

    [Tooltip("第二个自动箭头沿路线距离玩家多远")]
    public float autoArrowDistance2 = 8f;

    [Tooltip("自动箭头离NavMesh稍微抬高")]
    public float autoArrowHeight = 0.035f;

    [Tooltip("自动箭头向路径前方看多远，以决定箭头朝向")]
    public float arrowDirectionLookAhead = 1.2f;


    // =========================================================
    // NavMesh Settings
    // =========================================================

    [Header("NavMesh Settings")]

    [Tooltip("寻找玩家附近NavMesh的范围")]
    public float playerNavMeshSearchRadius = 1.5f;

    [Tooltip("寻找终点附近NavMesh的范围")]
    public float destinationNavMeshSearchRadius = 2f;

    [Tooltip("玩家或终点吸附到NavMesh时，允许的最大垂直高度差")]
    public float maxVerticalSnapDistance = 1.2f;


    // =========================================================
    // Arrow Rotation
    // =========================================================

    [Header("Auto Arrow Rotation")]

    [Tooltip("修正箭头模型自身的轴方向")]
    public Vector3 autoArrowRotationOffset =
        new Vector3(90f, 0f, 0f);


    // =========================================================
    // Arrow Centering
    // =========================================================

    [Header("Auto Arrow Centering")]

    [Tooltip("是否尽量把自动箭头放到可走区域中间")]
    public bool centerArrowOnNavMesh = true;

    [Tooltip("向路径左右最多测试多少距离")]
    public float centerSearchWidth = 1.2f;

    [Tooltip("左右测试次数")]
    [Range(1, 8)]
    public int centerSearchSteps = 4;


    // =========================================================
    // Internal
    // =========================================================

    private readonly List<NavigationSegment> segments =
        new List<NavigationSegment>();

    private readonly List<GameObject> allFixedArrows =
        new List<GameObject>();

    private readonly List<GameObject> autoArrows =
        new List<GameObject>();

    private readonly HashSet<AutoNavigationZone> activeAutoZones =
        new HashSet<AutoNavigationZone>();


    private NavigationSegment currentSegment;

    private int currentArrowIndex = 0;


    private class NavigationSegment
    {
        public Transform root;

        public List<GameObject> arrows =
            new List<GameObject>();
    }


    // =========================================================
    // Start
    // =========================================================

    private void Start()
    {
        CollectSegments();

        CreateAutoArrows();

        HideAllFixedArrows();

        HideAutoArrows();


        if (player != null)
        {
            currentSegment =
                FindNearestSegment();

            currentArrowIndex =
                FindNearestArrowIndex(
                    currentSegment
                );
        }
    }


    // =========================================================
    // Update
    // =========================================================

    private void Update()
    {
        if (player == null)
            return;


        // -----------------------------------------------------
        // AutoNavZone
        // -----------------------------------------------------

        if (activeAutoZones.Count > 0)
        {
            HideAllFixedArrows();

            ShowAutoNavigationToDestination();

            return;
        }


        // -----------------------------------------------------
        // Fixed Navigation
        // -----------------------------------------------------

        HideAutoArrows();

        UpdateFixedNavigation();
    }


    // =========================================================
    // Collect Segments
    // =========================================================

    private void CollectSegments()
    {
        segments.Clear();

        allFixedArrows.Clear();


        foreach (Transform segmentRoot in transform)
        {
            if (!segmentRoot.name.StartsWith("Segment"))
                continue;


            NavigationSegment segment =
                new NavigationSegment();

            segment.root =
                segmentRoot;


            foreach (Transform child in segmentRoot)
            {
                string lowerName =
                    child.name.ToLower();


                if (lowerName.Contains("arrow"))
                {
                    segment.arrows.Add(
                        child.gameObject
                    );

                    allFixedArrows.Add(
                        child.gameObject
                    );
                }
            }


            if (segment.arrows.Count > 0)
            {
                segments.Add(segment);
            }
        }


        Debug.Log(
            "Navigation Segments: "
            + segments.Count
            + " | Fixed Arrows: "
            + allFixedArrows.Count
        );
    }


    // =========================================================
    // Fixed Navigation
    // =========================================================

    private void UpdateFixedNavigation()
    {
        HideAllFixedArrows();


        if (segments.Count == 0)
            return;


        NavigationSegment bestSegment =
            FindBestCurrentSegment();


        if (
            bestSegment != null &&
            bestSegment != currentSegment
        )
        {
            currentSegment =
                bestSegment;


            currentArrowIndex =
                FindNearestArrowIndex(
                    currentSegment
                );
        }


        if (currentSegment == null)
            return;


        UpdatePassedArrowIndex();

        ShowNextFixedArrows();
    }


    // =========================================================
    // Choose Segment
    // =========================================================

    private NavigationSegment FindBestCurrentSegment()
    {
        NavigationSegment bestSegment =
            currentSegment;


        float bestDistance =
            Mathf.Infinity;


        if (currentSegment != null)
        {
            bestDistance =
                GetSegmentDistance(
                    currentSegment
                );
        }


        foreach (
            NavigationSegment segment
            in segments
        )
        {
            float distance =
                GetSegmentDistance(
                    segment
                );


            if (float.IsInfinity(distance))
                continue;


            if (bestSegment == null)
            {
                bestSegment =
                    segment;

                bestDistance =
                    distance;

                continue;
            }


            if (
                segment != currentSegment &&
                distance + branchSwitchAdvantage
                < bestDistance
            )
            {
                bestSegment =
                    segment;

                bestDistance =
                    distance;
            }
        }


        return bestSegment;
    }


    // =========================================================
    // Segment Distance
    // =========================================================

    private float GetSegmentDistance(
        NavigationSegment segment
    )
    {
        if (
            segment == null ||
            segment.arrows.Count == 0
        )
        {
            return Mathf.Infinity;
        }


        float bestDistance =
            Mathf.Infinity;


        foreach (
            GameObject arrow
            in segment.arrows
        )
        {
            if (arrow == null)
                continue;


            float distance =
                GetNavMeshDistance(
                    player.position,
                    arrow.transform.position
                );


            if (distance < bestDistance)
            {
                bestDistance =
                    distance;
            }
        }


        return bestDistance;
    }


    // =========================================================
    // Find Nearest Arrow Index
    // =========================================================

    private int FindNearestArrowIndex(
        NavigationSegment segment
    )
    {
        if (
            segment == null ||
            segment.arrows.Count == 0
        )
        {
            return 0;
        }


        int bestIndex =
            0;


        float bestDistance =
            Mathf.Infinity;


        for (
            int i = 0;
            i < segment.arrows.Count;
            i++
        )
        {
            GameObject arrow =
                segment.arrows[i];


            if (arrow == null)
                continue;


            float distance =
                GetNavMeshDistance(
                    player.position,
                    arrow.transform.position
                );


            if (distance < bestDistance)
            {
                bestDistance =
                    distance;

                bestIndex =
                    i;
            }
        }


        return bestIndex;
    }


    // =========================================================
    // Passed Fixed Arrow
    // =========================================================

    private void UpdatePassedArrowIndex()
    {
        if (currentSegment == null)
            return;


        while (
            currentArrowIndex
            < currentSegment.arrows.Count
        )
        {
            GameObject arrow =
                currentSegment.arrows[
                    currentArrowIndex
                ];


            if (arrow == null)
            {
                currentArrowIndex++;

                continue;
            }


            float distance =
                Vector3.Distance(
                    player.position,
                    arrow.transform.position
                );


            if (
                distance
                <= passedArrowDistance
            )
            {
                currentArrowIndex++;

                continue;
            }


            break;
        }
    }


    // =========================================================
    // Show Fixed 3 Arrows
    // =========================================================

    private void ShowNextFixedArrows()
    {
        if (currentSegment == null)
            return;


        int shown =
            0;


        for (
            int i = currentArrowIndex;
            i < currentSegment.arrows.Count;
            i++
        )
        {
            GameObject arrow =
                currentSegment.arrows[i];


            if (arrow == null)
                continue;


            arrow.SetActive(true);

            shown++;


            if (
                shown
                >= maxVisibleFixedArrows
            )
            {
                break;
            }
        }
    }


    // =========================================================
    // Find Nearest Segment
    // =========================================================

    private NavigationSegment FindNearestSegment()
    {
        NavigationSegment bestSegment =
            null;


        float bestDistance =
            Mathf.Infinity;


        foreach (
            NavigationSegment segment
            in segments
        )
        {
            float distance =
                GetSegmentDistance(
                    segment
                );


            if (distance < bestDistance)
            {
                bestDistance =
                    distance;

                bestSegment =
                    segment;
            }
        }


        return bestSegment;
    }


    // =========================================================
    // AutoNavigationZone
    // =========================================================

    public void EnterAutoNavigationZone(
        AutoNavigationZone zone
    )
    {
        if (zone == null)
            return;


        activeAutoZones.Add(zone);
    }


    public void ExitAutoNavigationZone(
        AutoNavigationZone zone
    )
    {
        if (zone == null)
            return;


        activeAutoZones.Remove(zone);


        if (
            activeAutoZones.Count == 0 &&
            player != null
        )
        {
            currentSegment =
                FindNearestSegment();


            currentArrowIndex =
                FindNearestArrowIndex(
                    currentSegment
                );
        }
    }


    // =========================================================
    // Auto Navigation → Final Destination
    // =========================================================

    private void ShowAutoNavigationToDestination()
    {
        HideAutoArrows();


        if (
            player == null ||
            destination == null
        )
        {
            return;
        }


        NavMeshPath path =
            new NavMeshPath();


        if (
            !TryCalculateDestinationPath(
                player.position,
                destination.position,
                path
            )
        )
        {
            return;
        }


        if (
            path.status
            != NavMeshPathStatus.PathComplete
        )
        {
            return;
        }


        float[] distances =
        {
            autoArrowDistance1,
            autoArrowDistance2
        };


        int shown =
            0;


        for (
            int i = 0;
            i < distances.Length;
            i++
        )
        {
            if (
                shown
                >= autoArrows.Count
            )
            {
                break;
            }


            if (
                TryGetPointAlongPath(
                    path,
                    distances[i],
                    out Vector3 pathPoint,
                    out Vector3 pathDirection
                )
            )
            {
                Vector3 finalPosition =
                    GetCenteredNavMeshPosition(
                        pathPoint,
                        pathDirection
                    );


                PlaceAutoArrow(
                    autoArrows[shown],
                    finalPosition,
                    pathDirection
                );


                shown++;
            }
        }


        // -----------------------------------------------------
        // Very close to destination
        // -----------------------------------------------------

        if (
            shown == 0 &&
            autoArrows.Count > 0
        )
        {
            if (
                TrySampleNavMeshSameFloor(
                    destination.position,
                    destinationNavMeshSearchRadius,
                    out NavMeshHit endHit
                )
            )
            {
                Vector3 direction =
                    endHit.position
                    - player.position;


                PlaceAutoArrow(
                    autoArrows[0],
                    endHit.position,
                    direction
                );
            }
        }
    }


    // =========================================================
    // Player → Destination NavMesh Path
    // =========================================================

    private bool TryCalculateDestinationPath(
        Vector3 start,
        Vector3 end,
        NavMeshPath result
    )
    {
        if (
            !TrySampleNavMeshSameFloor(
                start,
                playerNavMeshSearchRadius,
                out NavMeshHit startHit
            )
        )
        {
            return false;
        }


        // 终点可能与玩家不同楼层，
        // 所以这里不要求 destination 和玩家同高度
        if (
            !NavMesh.SamplePosition(
                end,
                out NavMeshHit endHit,
                destinationNavMeshSearchRadius,
                NavMesh.AllAreas
            )
        )
        {
            return false;
        }


        bool success =
            NavMesh.CalculatePath(
                startHit.position,
                endHit.position,
                NavMesh.AllAreas,
                result
            );


        return
            success &&
            result.status
            == NavMeshPathStatus.PathComplete;
    }


    // =========================================================
    // Prevent player snapping to another floor
    // =========================================================

    private bool TrySampleNavMeshSameFloor(
        Vector3 worldPosition,
        float radius,
        out NavMeshHit bestHit
    )
    {
        bestHit =
            new NavMeshHit();


        if (
            !NavMesh.SamplePosition(
                worldPosition,
                out NavMeshHit hit,
                radius,
                NavMesh.AllAreas
            )
        )
        {
            return false;
        }


        float verticalDifference =
            Mathf.Abs(
                hit.position.y
                - worldPosition.y
            );


        if (
            verticalDifference
            > maxVerticalSnapDistance
        )
        {
            return false;
        }


        bestHit =
            hit;


        return true;
    }


    // =========================================================
    // General NavMesh Distance
    // =========================================================

    private float GetNavMeshDistance(
        Vector3 start,
        Vector3 end
    )
    {
        NavMeshPath path =
            new NavMeshPath();


        if (
            !TryCalculatePath(
                start,
                end,
                path
            )
        )
        {
            return Mathf.Infinity;
        }


        float total =
            0f;


        for (
            int i = 0;
            i < path.corners.Length - 1;
            i++
        )
        {
            total +=
                Vector3.Distance(
                    path.corners[i],
                    path.corners[i + 1]
                );
        }


        return total;
    }


    private bool TryCalculatePath(
        Vector3 start,
        Vector3 end,
        NavMeshPath result
    )
    {
        if (
            !TrySampleNavMeshSameFloor(
                start,
                playerNavMeshSearchRadius,
                out NavMeshHit startHit
            )
        )
        {
            return false;
        }


        if (
            !NavMesh.SamplePosition(
                end,
                out NavMeshHit endHit,
                1.5f,
                NavMesh.AllAreas
            )
        )
        {
            return false;
        }


        bool success =
            NavMesh.CalculatePath(
                startHit.position,
                endHit.position,
                NavMesh.AllAreas,
                result
            );


        return
            success &&
            result.status
            == NavMeshPathStatus.PathComplete;
    }


    // =========================================================
    // Get point + ROAD direction
    // =========================================================

    private bool TryGetPointAlongPath(
        NavMeshPath path,
        float targetDistance,
        out Vector3 point,
        out Vector3 direction
    )
    {
        point =
            Vector3.zero;

        direction =
            Vector3.forward;


        if (
            path == null ||
            path.corners == null ||
            path.corners.Length < 2
        )
        {
            return false;
        }


        float travelled =
            0f;


        for (
            int i = 0;
            i < path.corners.Length - 1;
            i++
        )
        {
            Vector3 start =
                path.corners[i];

            Vector3 end =
                path.corners[i + 1];


            float segmentLength =
                Vector3.Distance(
                    start,
                    end
                );


            if (
                segmentLength
                < 0.001f
            )
            {
                continue;
            }


            if (
                travelled + segmentLength
                >= targetDistance
            )
            {
                float remaining =
                    targetDistance
                    - travelled;


                Vector3 segmentDirection =
                    (end - start).normalized;


                point =
                    start
                    + segmentDirection
                    * remaining;


                // ---------------------------------------------
                // IMPORTANT:
                // 箭头不是直接指终点
                // 而是看当前道路接下来怎么走
                // ---------------------------------------------

                direction =
                    GetPathDirectionAhead(
                        path,
                        i,
                        point,
                        arrowDirectionLookAhead
                    );


                return true;
            }


            travelled +=
                segmentLength;
        }


        return false;
    }


    // =========================================================
    // Look ahead along road
    // =========================================================

    private Vector3 GetPathDirectionAhead(
        NavMeshPath path,
        int currentCornerIndex,
        Vector3 currentPoint,
        float lookAheadDistance
    )
    {
        if (
            path == null ||
            path.corners == null ||
            path.corners.Length < 2
        )
        {
            return Vector3.forward;
        }


        Vector3 from =
            currentPoint;


        float remainingLookAhead =
            Mathf.Max(
                0.1f,
                lookAheadDistance
            );


        for (
            int i = currentCornerIndex + 1;
            i < path.corners.Length;
            i++
        )
        {
            Vector3 next =
                path.corners[i];


            Vector3 delta =
                next - from;


            float length =
                delta.magnitude;


            if (
                length
                < 0.001f
            )
            {
                from =
                    next;

                continue;
            }


            if (
                length
                >= remainingLookAhead
            )
            {
                Vector3 lookPoint =
                    from
                    + delta.normalized
                    * remainingLookAhead;


                Vector3 roadDirection =
                    lookPoint
                    - currentPoint;


                roadDirection.y =
                    0f;


                if (
                    roadDirection.sqrMagnitude
                    > 0.001f
                )
                {
                    return
                        roadDirection.normalized;
                }
            }


            remainingLookAhead -=
                length;


            from =
                next;
        }


        // -----------------------------------------------------
        // Near end of route
        // -----------------------------------------------------

        Vector3 finalDirection =
            path.corners[
                path.corners.Length - 1
            ]
            - currentPoint;


        finalDirection.y =
            0f;


        if (
            finalDirection.sqrMagnitude
            > 0.001f
        )
        {
            return
                finalDirection.normalized;
        }


        return Vector3.forward;
    }


    // =========================================================
    // Center arrow inside walkable area
    // =========================================================

    private Vector3 GetCenteredNavMeshPosition(
        Vector3 pathPoint,
        Vector3 pathDirection
    )
    {
        if (!centerArrowOnNavMesh)
            return pathPoint;


        Vector3 flatDirection =
            pathDirection;

        flatDirection.y =
            0f;


        if (
            flatDirection.sqrMagnitude
            < 0.001f
        )
        {
            return pathPoint;
        }


        flatDirection.Normalize();


        Vector3 right =
            Vector3.Cross(
                Vector3.up,
                flatDirection
            ).normalized;


        Vector3 bestPosition =
            pathPoint;


        float bestEdgeDistance =
            GetDistanceFromNavMeshEdge(
                pathPoint
            );


        for (
            int i = 1;
            i <= centerSearchSteps;
            i++
        )
        {
            float offset =
                centerSearchWidth
                * ((float)i / centerSearchSteps);


            Vector3 leftCandidate =
                pathPoint
                - right * offset;


            Vector3 rightCandidate =
                pathPoint
                + right * offset;


            TestCenterCandidate(
                leftCandidate,
                pathPoint.y,
                ref bestPosition,
                ref bestEdgeDistance
            );


            TestCenterCandidate(
                rightCandidate,
                pathPoint.y,
                ref bestPosition,
                ref bestEdgeDistance
            );
        }


        return bestPosition;
    }


    private void TestCenterCandidate(
        Vector3 candidate,
        float expectedHeight,
        ref Vector3 bestPosition,
        ref float bestEdgeDistance
    )
    {
        if (
            !NavMesh.SamplePosition(
                candidate,
                out NavMeshHit sampleHit,
                0.35f,
                NavMesh.AllAreas
            )
        )
        {
            return;
        }


        if (
            Mathf.Abs(
                sampleHit.position.y
                - expectedHeight
            )
            > 0.5f
        )
        {
            return;
        }


        float edgeDistance =
            GetDistanceFromNavMeshEdge(
                sampleHit.position
            );


        if (
            edgeDistance
            > bestEdgeDistance
        )
        {
            bestEdgeDistance =
                edgeDistance;


            bestPosition =
                sampleHit.position;
        }
    }


    private float GetDistanceFromNavMeshEdge(
        Vector3 position
    )
    {
        if (
            NavMesh.FindClosestEdge(
                position,
                out NavMeshHit edgeHit,
                NavMesh.AllAreas
            )
        )
        {
            return
                Vector3.Distance(
                    position,
                    edgeHit.position
                );
        }


        return 0f;
    }


    // =========================================================
    // Place Auto Arrow
    // =========================================================

    private void PlaceAutoArrow(
        GameObject arrow,
        Vector3 navPosition,
        Vector3 direction
    )
    {
        if (arrow == null)
            return;


        arrow.SetActive(true);


        arrow.transform.position =
            navPosition
            + Vector3.up
            * autoArrowHeight;


        Vector3 flatDirection =
            direction;


        flatDirection.y =
            0f;


        if (
            flatDirection.sqrMagnitude
            < 0.001f
        )
        {
            return;
        }


        flatDirection.Normalize();


        Quaternion pathRotation =
            Quaternion.LookRotation(
                flatDirection,
                Vector3.up
            );


        arrow.transform.rotation =
            pathRotation
            * Quaternion.Euler(
                autoArrowRotationOffset
            );
    }


    // =========================================================
    // Create 2 Auto Arrows
    // =========================================================

    private void CreateAutoArrows()
    {
        autoArrows.Clear();


        if (arrowPrefab == null)
        {
            Debug.LogWarning(
                "VRNavigationPath: Arrow Prefab is missing."
            );

            return;
        }


        int count =
            Mathf.Max(
                1,
                maxAutoArrows
            );


        for (
            int i = 0;
            i < count;
            i++
        )
        {
            GameObject arrow =
                Instantiate(
                    arrowPrefab
                );


            arrow.name =
                "AutoNavigationArrow_"
                + (i + 1);


            arrow.SetActive(false);


            autoArrows.Add(
                arrow
            );
        }
    }


    // =========================================================
    // Hide
    // =========================================================

    private void HideAllFixedArrows()
    {
        foreach (
            GameObject arrow
            in allFixedArrows
        )
        {
            if (arrow != null)
            {
                arrow.SetActive(false);
            }
        }
    }


    private void HideAutoArrows()
    {
        foreach (
            GameObject arrow
            in autoArrows
        )
        {
            if (arrow != null)
            {
                arrow.SetActive(false);
            }
        }
    }
}