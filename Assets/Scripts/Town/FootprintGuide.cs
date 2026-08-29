using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FootprintGuide : MonoBehaviour
{
    [Header("玩家")]
    public Transform player;

    [Header("脚印终点")]
    public Transform target;

    [Header("脚印 Prefab")]
    public GameObject footprintPrefab;

    [Header("Driver")]
    public Animator driverAnimator;
    public string drivingStateName = "Driving";

    // ======================================================
    // 脚踏板呼吸灯
    // ======================================================

    [Header("脚踏板蓝色呼吸灯")]
    [Tooltip("拖入 StepBoardGlowPulse")]
    public StepBoardGlowPulse stepBoardGlow;


    // ======================================================
    // 脚印设置
    // ======================================================

    [Header("脚印设置")]
    public float spacing = 0.6f;
    public float groundOffset = 0.015f;


    // ======================================================
    // NavMesh
    // ======================================================

    [Header("NavMesh")]
    public float navMeshSampleDistance = 2f;


    // ======================================================
    // 模型旋转修正
    // ======================================================

    [Header("模型旋转修正")]
    public Vector3 rotationOffset =
        new Vector3(0f, 180f, 0f);


    // ======================================================
    // 内部状态
    // ======================================================

    private List<GameObject> spawnedFootprints =
        new List<GameObject>();

    private bool hasGenerated = false;


    // ======================================================
    // Update
    // ======================================================

    void Update()
    {
        // 已经生成过一次
        // 不再重复
        if (hasGenerated)
            return;


        if (driverAnimator == null)
            return;


        AnimatorStateInfo state =
            driverAnimator.GetCurrentAnimatorStateInfo(0);


        // ==================================================
        // Driver进入Driving
        //
        // 同一时刻：
        // 1. 生成脚印
        // 2. 脚踏板开始蓝色呼吸
        // ==================================================

        if (state.shortNameHash ==
            Animator.StringToHash(
                drivingStateName
            ))
        {
            // 生成脚印
            GenerateFootprints();


            // 脚踏板开始发光
            if (stepBoardGlow != null)
            {
                stepBoardGlow.StartGlow();
            }


            hasGenerated = true;


            Debug.Log(
                "Driver进入Driving → 脚印出现 + 脚踏板开始蓝色呼吸"
            );
        }
    }


    // ======================================================
    // 生成脚印
    // ======================================================

    public void GenerateFootprints()
    {
        ClearFootprints();


        if (player == null ||
            target == null ||
            footprintPrefab == null)
        {
            Debug.LogWarning(
                "FootprintGuide：引用没有设置完整！"
            );

            return;
        }


        NavMeshHit startHit;
        NavMeshHit endHit;


        // ==================================================
        // 找玩家附近 NavMesh
        // ==================================================

        bool foundStart =
            NavMesh.SamplePosition(
                player.position,
                out startHit,
                navMeshSampleDistance,
                NavMesh.AllAreas
            );


        // ==================================================
        // 找脚踏板附近 NavMesh
        // ==================================================

        bool foundEnd =
            NavMesh.SamplePosition(
                target.position,
                out endHit,
                navMeshSampleDistance,
                NavMesh.AllAreas
            );


        if (!foundStart)
        {
            Debug.LogWarning(
                "玩家附近找不到NavMesh！"
            );

            return;
        }


        if (!foundEnd)
        {
            Debug.LogWarning(
                "FootprintTarget附近找不到NavMesh！"
            );

            return;
        }


        // ==================================================
        // 计算路径
        // ==================================================

        NavMeshPath path =
            new NavMeshPath();


        bool success =
            NavMesh.CalculatePath(
                startHit.position,
                endHit.position,
                NavMesh.AllAreas,
                path
            );


        if (!success ||
            path.status !=
            NavMeshPathStatus.PathComplete ||
            path.corners.Length < 2)
        {
            Debug.LogWarning(
                "无法计算完整脚印路径！"
            );

            return;
        }


        // ==================================================
        // 沿 NavMesh 路径生成脚印
        // ==================================================

        for (int i = 0;
             i < path.corners.Length - 1;
             i++)
        {
            Vector3 segmentStart =
                path.corners[i];

            Vector3 segmentEnd =
                path.corners[i + 1];


            Vector3 direction =
                segmentEnd -
                segmentStart;


            float segmentLength =
                direction.magnitude;


            if (segmentLength < 0.01f)
                continue;


            direction.Normalize();


            int count =
                Mathf.FloorToInt(
                    segmentLength /
                    spacing
                );


            for (int j = 1;
                 j <= count;
                 j++)
            {
                Vector3 position =
                    segmentStart +
                    direction *
                    spacing *
                    j;


                NavMeshHit floorHit;


                if (NavMesh.SamplePosition(
                    position,
                    out floorHit,
                    0.5f,
                    NavMesh.AllAreas))
                {
                    position =
                        floorHit.position +
                        Vector3.up *
                        groundOffset;


                    Quaternion rotation =
                        Quaternion.LookRotation(
                            direction,
                            Vector3.up
                        );


                    rotation *=
                        Quaternion.Euler(
                            rotationOffset
                        );


                    GameObject footprint =
                        Instantiate(
                            footprintPrefab,
                            position,
                            rotation
                        );


                    spawnedFootprints.Add(
                        footprint
                    );
                }
            }
        }


        // ==================================================
        // 最后一个脚印
        // 直接放在脚踏板上
        // ==================================================

        if (target != null)
        {
            Vector3 finalDirection =
                target.position -
                path.corners[
                    path.corners.Length - 2
                ];


            finalDirection.y = 0f;


            if (finalDirection.sqrMagnitude >
                0.001f)
            {
                finalDirection.Normalize();
            }
            else
            {
                finalDirection =
                    target.forward;
            }


            Quaternion finalRotation =
                Quaternion.LookRotation(
                    finalDirection,
                    Vector3.up
                );


            finalRotation *=
                Quaternion.Euler(
                    rotationOffset
                );


            GameObject lastFootprint =
                Instantiate(
                    footprintPrefab,
                    target.position,
                    finalRotation
                );


            spawnedFootprints.Add(
                lastFootprint
            );
        }


        Debug.Log(
            "NavMesh脚印生成完成，共：" +
            spawnedFootprints.Count
        );
    }


    // ======================================================
    // 清除全部脚印
    // ======================================================

    public void ClearFootprints()
    {
        foreach (
            GameObject footprint
            in spawnedFootprints)
        {
            if (footprint != null)
            {
                Destroy(
                    footprint
                );
            }
        }


        spawnedFootprints.Clear();
    }


    // ======================================================
    // 玩家已经上车
    //
    // 脚印消失
    //
    // 脚踏板关闭由
    // VRBoardingDetector → StopGlow()
    // 负责
    // ======================================================

    public void PlayerBoarded()
    {
        ClearFootprints();

        Debug.Log(
            "玩家上车 → 脚印消失"
        );
    }


    // ======================================================
    // 如果重新测试这一段
    // ======================================================

    public void ResetGuide()
    {
        ClearFootprints();

        hasGenerated = false;
    }
}