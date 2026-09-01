using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldmanGazeTarget : MonoBehaviour
{
    [Header("真实眼动射线")]
    public CombinedEyeGazeRay eyeGazeRay;

    [Header("允许触发的目标")]
    public Transform[] gazeTargets;

    [Header("需要恢复彩色的根物体")]
    [Tooltip("例如直接拖 Oldman 根物体")]
    public Transform[] colorRoots;


    // ======================================================
    // 黑白灰材质
    // ======================================================

    [Header("默认白色材质")]
    [Tooltip("所有未指定为中灰/深灰的部件，开始时都会使用这个材质")]
    public Material whiteMaterial;

    [Header("中灰材质")]
    public Material grayMid;

    [Header("深灰材质")]
    public Material grayDark;

    [Header("手动指定：中灰部件")]
    public Renderer[] midRenderers;

    [Header("手动指定：深灰部件")]
    public Renderer[] darkRenderers;


    // ======================================================
    // 老人第一次求助
    // ======================================================

    [Header("老人第一次求助对话框")]
    [Tooltip("拖入 OldmanSpeechBubble")]
    public GameObject speechBubble;

    [Header("老人语音 Audio Source")]
    [Tooltip("拖入 Oldman 上的 Audio Source")]
    public AudioSource oldmanAudioSource;

    [Header("第一次求助语音")]
    [Tooltip("Excuse me, I can't get through. Could you help me move these boxes?")]
    public AudioClip helpVoiceClip;


    // ======================================================
    // 注视设置
    // ======================================================

    [Header("注视设置")]
    public float requiredGazeTime = 1.0f;

    public float gazeBreakTolerance = 0.5f;


    [Header("调试")]
    public bool showDebugLog = false;


    // ======================================================
    // 内部变量
    // ======================================================

    private Renderer[] allColorRenderers;

    private Material[][] originalMaterials;

    private float gazeTimer = 0f;

    private float lookAwayTimer = 0f;

    private bool hasRevealedColor = false;

    private bool initialized = false;

    private Coroutine dialogueCoroutine;


    // ======================================================
    // Start
    // ======================================================

    private void Start()
    {
        // 收集所有需要变色的 Renderer
        CollectAllColorRenderers();


        if (allColorRenderers == null ||
            allColorRenderers.Length == 0)
        {
            Debug.LogError(
                "[OldmanGazeTarget] 没找到需要处理的 Renderer。"
            );

            return;
        }


        // 保存原始彩色材质
        SaveOriginalMaterials();


        // 游戏开始先变成黑白灰
        ApplyGrayscale();


        // 对话框开始隐藏
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }


        gazeTimer = 0f;

        lookAwayTimer = 0f;

        hasRevealedColor = false;

        initialized = true;
    }


    // ======================================================
    // Update
    // ======================================================

    private void Update()
    {
        if (!initialized)
            return;


        // 已经恢复彩色以后，不再重复触发
        if (hasRevealedColor)
            return;


        if (eyeGazeRay == null)
            return;


        bool lookingAtOldman =
            IsLookingAtOldman();


        if (lookingAtOldman)
        {
            lookAwayTimer = 0f;

            gazeTimer += Time.deltaTime;


            if (showDebugLog)
            {
                Debug.Log(
                    "[OldmanGazeTarget] 注视老人：" +
                    gazeTimer.ToString("F2") +
                    " / " +
                    requiredGazeTime.ToString("F2")
                );
            }


            // 注视时间达到要求
            if (gazeTimer >= requiredGazeTime)
            {
                RevealColor();
            }
        }
        else
        {
            lookAwayTimer += Time.deltaTime;


            // 短暂移开视线不会马上清零
            if (lookAwayTimer >
                gazeBreakTolerance)
            {
                gazeTimer = 0f;

                lookAwayTimer = 0f;
            }
        }
    }


    // ======================================================
    // 判断是不是看着老人
    // ======================================================

    private bool IsLookingAtOldman()
    {
        if (eyeGazeRay == null)
            return false;


        if (!eyeGazeRay.HasHit)
            return false;


        Collider hitCollider =
            eyeGazeRay.CurrentHit.collider;


        if (hitCollider == null)
            return false;


        Transform hitTransform =
            hitCollider.transform;


        if (gazeTargets == null)
            return false;


        for (int i = 0;
             i < gazeTargets.Length;
             i++)
        {
            Transform target =
                gazeTargets[i];


            if (target == null)
                continue;


            // 直接看中目标
            if (hitTransform == target)
                return true;


            // 看中目标的子物体
            if (hitTransform.IsChildOf(target))
                return true;
        }


        return false;
    }


    // ======================================================
    // 收集所有 Renderer
    // ======================================================

    private void CollectAllColorRenderers()
    {
        List<Renderer> rendererList =
            new List<Renderer>();


        if (colorRoots == null)
        {
            allColorRenderers =
                rendererList.ToArray();

            return;
        }


        for (int i = 0;
             i < colorRoots.Length;
             i++)
        {
            Transform root =
                colorRoots[i];


            if (root == null)
                continue;


            Renderer[] foundRenderers =
                root.GetComponentsInChildren
                <Renderer>(true);


            for (int j = 0;
                 j < foundRenderers.Length;
                 j++)
            {
                Renderer r =
                    foundRenderers[j];


                if (r == null)
                    continue;


                if (!rendererList.Contains(r))
                {
                    rendererList.Add(r);
                }
            }
        }


        allColorRenderers =
            rendererList.ToArray();
    }


    // ======================================================
    // 保存原始彩色材质
    // ======================================================

    private void SaveOriginalMaterials()
    {
        originalMaterials =
            new Material
            [allColorRenderers.Length][];


        for (int i = 0;
             i < allColorRenderers.Length;
             i++)
        {
            Material[] materials =
                allColorRenderers[i].materials;


            originalMaterials[i] =
                new Material[materials.Length];


            for (int j = 0;
                 j < materials.Length;
                 j++)
            {
                originalMaterials[i][j] =
                    materials[j];
            }
        }
    }


    // ======================================================
    // 应用黑白灰
    // ======================================================

    private void ApplyGrayscale()
    {
        // 默认全部白色
        if (whiteMaterial != null)
        {
            for (int i = 0;
                 i < allColorRenderers.Length;
                 i++)
            {
                ApplyMaterialToRenderer(
                    allColorRenderers[i],
                    whiteMaterial
                );
            }
        }


        // 指定部件覆盖成中灰
        ApplyMaterialToRenderers(
            midRenderers,
            grayMid
        );


        // 指定部件覆盖成深灰
        ApplyMaterialToRenderers(
            darkRenderers,
            grayDark
        );
    }


    private void ApplyMaterialToRenderers(
        Renderer[] renderers,
        Material material)
    {
        if (renderers == null)
            return;


        if (material == null)
            return;


        for (int i = 0;
             i < renderers.Length;
             i++)
        {
            ApplyMaterialToRenderer(
                renderers[i],
                material
            );
        }
    }


    private void ApplyMaterialToRenderer(
        Renderer renderer,
        Material material)
    {
        if (renderer == null ||
            material == null)
            return;


        Material[] currentMaterials =
            renderer.materials;


        Material[] newMaterials =
            new Material
            [currentMaterials.Length];


        for (int i = 0;
             i < newMaterials.Length;
             i++)
        {
            newMaterials[i] =
                material;
        }


        renderer.materials =
            newMaterials;
    }


    // ======================================================
    // 恢复彩色
    // ======================================================

    public void RevealColor()
    {
        if (!initialized)
            return;


        if (hasRevealedColor)
            return;


        hasRevealedColor = true;


        // 恢复所有原始彩色材质
        for (int i = 0;
             i < allColorRenderers.Length;
             i++)
        {
            if (allColorRenderers[i] == null)
                continue;


            allColorRenderers[i].materials =
                originalMaterials[i];
        }


        Debug.Log(
            "[OldmanGazeTarget] 老人已恢复彩色。"
        );


        // 开始第一次求助
        StartFirstDialogue();
    }


    // ======================================================
    // 第一次求助
    // ======================================================

    private void StartFirstDialogue()
    {
        // 防止协程重复
        if (dialogueCoroutine != null)
        {
            StopCoroutine(
                dialogueCoroutine
            );
        }


        dialogueCoroutine =
            StartCoroutine(
                FirstDialogueRoutine()
            );
    }


    private IEnumerator FirstDialogueRoutine()
    {
        // 显示对话框
        if (speechBubble != null)
        {
            speechBubble.SetActive(true);
        }


        // 播放指定语音
        if (oldmanAudioSource != null)
        {
            // 如果 Inspector 单独拖了语音
            if (helpVoiceClip != null)
            {
                oldmanAudioSource.clip =
                    helpVoiceClip;
            }


            if (oldmanAudioSource.clip != null)
            {
                oldmanAudioSource.Play();


                // 等语音播放完成
                while (oldmanAudioSource.isPlaying)
                {
                    yield return null;
                }
            }
            else
            {
                // 没有语音时，
                // 字幕至少显示 3 秒
                yield return
                    new WaitForSeconds(3f);
            }
        }
        else
        {
            // 没 Audio Source，
            // 字幕显示 3 秒
            yield return
                new WaitForSeconds(3f);
        }


        // 语音结束关闭字幕
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }


        dialogueCoroutine = null;
    }


    // ======================================================
    // 重置测试
    // ======================================================

    public void ResetToGray()
    {
        if (!initialized)
            return;


        gazeTimer = 0f;

        lookAwayTimer = 0f;

        hasRevealedColor = false;


        if (dialogueCoroutine != null)
        {
            StopCoroutine(
                dialogueCoroutine
            );

            dialogueCoroutine = null;
        }


        if (oldmanAudioSource != null)
        {
            oldmanAudioSource.Stop();
        }


        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }


        ApplyGrayscale();


        Debug.Log(
            "[OldmanGazeTarget] 老人已重新变成黑白灰。"
        );
    }
}