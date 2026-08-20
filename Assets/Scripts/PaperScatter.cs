using System.Collections;
using UnityEngine;

public class PaperScatter : MonoBehaviour
{
    [Header("Papers")]
    public Rigidbody[] papers;


    // ==========================================================
    // Scatter Direction
    // ==========================================================

    [Header("Scatter Direction")]

    // 水平散开的最小速度
    public float minHorizontalSpeed = 0.3f;

    // 水平散开的最大速度
    public float maxHorizontalSpeed = 0.7f;

    // 向上飞的基础速度
    public float upwardForce = 1.0f;

    // 向后散开的概率范围
    // 数值越小，越偏向前方散开
    public float backwardAmount = 0.25f;


    // ==========================================================
    // Rotation
    // ==========================================================

    [Header("Rotation")]

    // 初始旋转速度
    public float torqueForce = 0.25f;


    // ==========================================================
    // Air Time
    // ==========================================================

    [Header("Air Time")]

    // 最短飘动时间
    public float minAirTime = 1.0f;

    // 最长飘动时间
    public float maxAirTime = 2.5f;


    // ==========================================================
    // Flutter
    // ==========================================================

    [Header("Flutter / Paper Floating")]

    // 左右飘动强度
    public float flutterForce = 0.05f;

    // 很轻的向上托力
    public float liftForce = 0.015f;

    // 飘动速度
    public float flutterSpeed = 2.0f;

    // 空中轻微翻转
    public float flutterTorque = 0.03f;


    // ==========================================================
    // Falling
    // ==========================================================

    [Header("Falling")]

    // 下落时使用的重力倍率
    [Range(0.1f, 1f)]
    public float gravityMultiplier = 0.35f;


    // ==========================================================
    // Collision
    // ==========================================================

    [Header("Paper Collision")]

    // 刚飞出来时纸与纸之间先不碰撞
    public float paperCollisionDelay = 0.3f;


    // ==========================================================
    // Safety Limits
    // ==========================================================

    [Header("Safety Limits")]

    // 最大水平速度
    public float maxHorizontalVelocity = 1.0f;

    // 最大向上速度
    public float maxUpwardVelocity = 1.4f;

    // 最大整体速度
    public float maxTotalVelocity = 1.6f;


    // ==========================================================
    // Internal
    // ==========================================================

    private bool scattered = false;



    // ==========================================================
    // 开始散落
    // ==========================================================

    public void Scatter()
    {
        if (scattered)
            return;


        scattered = true;


        // ==========================================
        // 先关闭纸与纸之间碰撞
        // ==========================================

        IgnorePaperCollisions(true);


        // ==========================================
        // 所有纸开始飞出
        // ==========================================

        foreach (Rigidbody rb in papers)
        {
            if (rb == null)
                continue;


            // ==========================================
            // 从公文包 / 父物体中脱离
            // ==========================================

            rb.transform.SetParent(
                null,
                true
            );


            // ==========================================
            // 开启物理
            // ==========================================

            rb.isKinematic = false;

            // 飘动阶段暂时不用默认重力
            rb.useGravity = false;


            // ==========================================
            // 清除残留速度
            // ==========================================

            rb.velocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;



            // ==========================================
            // 让每张纸朝不同方向散开
            // ==========================================

            // 左右随机
            float randomSide =
                Random.Range(
                    -1f,
                    1f
                );


            // 前后随机
            // 大部分偏向前方
            float randomForward =
                Random.Range(
                    -backwardAmount,
                    1f
                );


            Vector3 horizontalDirection =
                transform.right
                * randomSide
                +
                transform.forward
                * randomForward;


            // 防止刚好得到接近零的方向
            if (
                horizontalDirection.sqrMagnitude
                < 0.01f
            )
            {
                horizontalDirection =
                    transform.forward;
            }


            horizontalDirection.Normalize();



            // ==========================================
            // 每张纸水平速度略微不同
            // ==========================================

            float horizontalSpeed =
                Random.Range(
                    minHorizontalSpeed,
                    maxHorizontalSpeed
                );



            // ==========================================
            // 每张纸向上的速度略微不同
            // ==========================================

            float randomUp =
                Random.Range(
                    upwardForce * 0.65f,
                    upwardForce
                );



            // ==========================================
            // 最终速度
            // ==========================================

            Vector3 velocity =
                horizontalDirection
                * horizontalSpeed
                +
                Vector3.up
                * randomUp;



            // ==========================================
            // 安全限制
            // 防止突然飞得过高 / 过远
            // ==========================================

            Vector3 horizontalVelocity =
                new Vector3(
                    velocity.x,
                    0f,
                    velocity.z
                );


            if (
                horizontalVelocity.magnitude
                > maxHorizontalVelocity
            )
            {
                horizontalVelocity =
                    horizontalVelocity.normalized
                    * maxHorizontalVelocity;
            }


            velocity.x =
                horizontalVelocity.x;

            velocity.z =
                horizontalVelocity.z;


            velocity.y =
                Mathf.Clamp(
                    velocity.y,
                    0f,
                    maxUpwardVelocity
                );


            if (
                velocity.magnitude
                > maxTotalVelocity
            )
            {
                velocity =
                    velocity.normalized
                    * maxTotalVelocity;
            }



            // ==========================================
            // 直接改变速度
            // 不受 Rigidbody Mass 影响
            // ==========================================

            rb.AddForce(
                velocity,
                ForceMode.VelocityChange
            );



            // ==========================================
            // 初始随机旋转
            // ==========================================

            Vector3 torque =
                new Vector3(
                    Random.Range(
                        -torqueForce,
                        torqueForce
                    ),

                    Random.Range(
                        -torqueForce,
                        torqueForce
                    ),

                    Random.Range(
                        -torqueForce,
                        torqueForce
                    )
                );


            rb.AddTorque(
                torque,
                ForceMode.VelocityChange
            );



            // ==================================================
            // ★ 新增
            //
            // 检查这是不是 NPC 要捡的剧情纸
            // ==================================================

            StoryPaperLanding storyPaper =
                rb.GetComponent<StoryPaperLanding>();


            if (storyPaper != null)
            {
                // ==============================================
                // 特殊剧情纸
                //
                // 它前面已经和普通纸一样：
                // 1. 从包里脱离
                // 2. 得到随机初速度
                // 3. 得到随机旋转
                //
                // 接下来不再进入普通 FloatAndFall
                // 而由 StoryPaperLanding 接管
                // ==============================================

                storyPaper.StartControlledLanding();


                Debug.Log(
                    "Story Paper Controlled Landing Started: "
                    + rb.name
                );
            }
            else
            {
                // ==============================================
                // 普通纸
                // 保持原来的随机飘落逻辑
                // ==============================================

                float airTime =
                    Random.Range(
                        minAirTime,
                        maxAirTime
                    );


                StartCoroutine(
                    FloatAndFall(
                        rb,
                        airTime
                    )
                );
            }
        }



        // ==========================================
        // 过一小段时间恢复纸与纸碰撞
        // ==========================================

        StartCoroutine(
            RestorePaperCollisions()
        );


        Debug.Log(
            "Papers Scattered"
        );
    }



    // ==========================================================
    // 普通纸：
    // 空中飘动，然后慢慢落下
    // ==========================================================

    private IEnumerator FloatAndFall(
        Rigidbody rb,
        float airTime
    )
    {
        float elapsed =
            0f;


        // 每张纸随机不同的飘动节奏
        float randomPhase =
            Random.Range(
                0f,
                Mathf.PI * 2f
            );



        // ==========================================
        // 第一阶段：
        // 在空中轻轻飘
        // ==========================================

        while (
            elapsed
            < airTime
        )
        {
            if (rb == null)
                yield break;


            elapsed +=
                Time.deltaTime;



            float wave =
                Mathf.Sin(
                    elapsed
                    * flutterSpeed
                    + randomPhase
                );


            float secondWave =
                Mathf.Cos(
                    elapsed
                    * flutterSpeed
                    * 0.7f
                    + randomPhase
                );



            // ==========================================
            // 左右轻轻飘
            // ==========================================

            Vector3 flutter =
                transform.right
                * wave
                * flutterForce;



            // ==========================================
            // 前后非常轻微变化
            // ==========================================

            flutter +=
                transform.forward
                * secondWave
                * flutterForce
                * 0.25f;



            // ==========================================
            // 很弱的向上托力
            // ==========================================

            flutter +=
                Vector3.up
                * liftForce;



            rb.AddForce(
                flutter,
                ForceMode.Acceleration
            );



            // ==========================================
            // 空中缓慢翻转
            // ==========================================

            Vector3 airTorque =
                new Vector3(
                    secondWave,
                    wave,
                    -wave
                )
                * flutterTorque;


            rb.AddTorque(
                airTorque,
                ForceMode.Acceleration
            );



            // ==========================================
            // 每帧限制速度
            // 防止碰撞以后突然喷出去
            // ==========================================

            LimitVelocity(
                rb
            );


            yield return null;
        }



        // ==========================================
        // 第二阶段：
        // 开始慢慢下落
        // ==========================================

        while (
            rb != null
        )
        {
            elapsed +=
                Time.deltaTime;



            // ==========================================
            // 使用弱重力
            // ==========================================

            rb.AddForce(
                Physics.gravity
                * gravityMultiplier,
                ForceMode.Acceleration
            );



            // ==========================================
            // 下落时仍然保持一点左右飘动
            // ==========================================

            float wave =
                Mathf.Sin(
                    elapsed
                    * flutterSpeed
                    + randomPhase
                );


            rb.AddForce(
                transform.right
                * wave
                * flutterForce
                * 0.3f,
                ForceMode.Acceleration
            );



            // ==========================================
            // 持续限制速度
            // ==========================================

            LimitVelocity(
                rb
            );


            yield return null;
        }
    }



    // ==========================================================
    // 限制纸张速度
    // ==========================================================

    private void LimitVelocity(
        Rigidbody rb
    )
    {
        if (rb == null)
            return;


        Vector3 velocity =
            rb.velocity;



        // ==========================================
        // 限制水平速度
        // ==========================================

        Vector3 horizontalVelocity =
            new Vector3(
                velocity.x,
                0f,
                velocity.z
            );


        if (
            horizontalVelocity.magnitude
            > maxHorizontalVelocity
        )
        {
            horizontalVelocity =
                horizontalVelocity.normalized
                * maxHorizontalVelocity;


            velocity.x =
                horizontalVelocity.x;


            velocity.z =
                horizontalVelocity.z;
        }



        // ==========================================
        // 限制向上速度
        // ==========================================

        if (
            velocity.y
            > maxUpwardVelocity
        )
        {
            velocity.y =
                maxUpwardVelocity;
        }



        // ==========================================
        // 限制整体速度
        // ==========================================

        if (
            velocity.magnitude
            > maxTotalVelocity
        )
        {
            velocity =
                velocity.normalized
                * maxTotalVelocity;
        }


        rb.velocity =
            velocity;
    }



    // ==========================================================
    // 临时忽略 / 恢复纸与纸之间碰撞
    // ==========================================================

    private void IgnorePaperCollisions(
        bool ignore
    )
    {
        if (papers == null)
            return;


        for (
            int i = 0;
            i < papers.Length;
            i++
        )
        {
            if (papers[i] == null)
                continue;


            Collider colliderA =
                papers[i]
                    .GetComponent<Collider>();


            if (colliderA == null)
                continue;



            for (
                int j = i + 1;
                j < papers.Length;
                j++
            )
            {
                if (papers[j] == null)
                    continue;


                Collider colliderB =
                    papers[j]
                        .GetComponent<Collider>();


                if (colliderB == null)
                    continue;



                Physics.IgnoreCollision(
                    colliderA,
                    colliderB,
                    ignore
                );
            }
        }
    }



    // ==========================================================
    // 恢复纸与纸之间碰撞
    // ==========================================================

    private IEnumerator RestorePaperCollisions()
    {
        yield return new WaitForSeconds(
            paperCollisionDelay
        );


        IgnorePaperCollisions(
            false
        );


        Debug.Log(
            "Paper Collisions Restored"
        );
    }
}