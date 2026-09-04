using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldmanGazeTarget : MonoBehaviour
{
    // ======================================================
    // 控制模式
    // ======================================================

    [Header("控制模式")]
    [Tooltip("勾选 = 电脑鼠标模式；不勾选 = VR真实眼动模式")]
    public bool desktopMode = true;


    // ======================================================
    // VR真实眼动
    // ======================================================

    [Header("VR真实眼动射线")]
    public CombinedEyeGazeRay eyeGazeRay;


    // ======================================================
    // Desktop鼠标视线
    // ======================================================

    [Header("电脑鼠标视线")]

    [Tooltip("拖入 XR Origin > Camera Offset > Main Camera")]
    public Camera desktopCamera;

    [Tooltip("拖入 DesktopGazeCanvas 上的 DesktopMouseGaze")]
    public DesktopMouseGaze desktopMouseGaze;

    [Tooltip("电脑视线最远检测距离")]
    public float desktopRayDistance = 100f;


    // ======================================================
    // 允许触发目标
    // ======================================================

    [Header("允许触发的目标")]
    public Transform[] gazeTargets;


    // ======================================================
    // 恢复彩色
    // ======================================================

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
    // 箱子任务
    // ======================================================

    [Header("箱子任务")]
    [Tooltip("拖入场景里的 BoxTaskManager")]
    public BoxTaskManager boxTaskManager;


    // ======================================================
    // Keyboard帮助
    // ======================================================

    [Header("Keyboard 帮助")]
    [Tooltip("拖入 XR Origin 上的 OldmanDesktopHelpShortcut")]
    public OldmanDesktopHelpShortcut desktopHelpShortcut;


    // ======================================================
    // 注视设置
    // ======================================================

    [Header("注视设置")]
    public float requiredGazeTime = 1.0f;

    public float gazeBreakTolerance = 0.5f;


    // ======================================================
    // 调试
    // ======================================================

    [Header("调试")]
    public bool showDebugLog = false;

    [Tooltip("Scene窗口显示电脑模式射线")]
    public bool showDesktopRay = true;


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
        CollectAllColorRenderers();

        if (allColorRenderers == null ||
            allColorRenderers.Length == 0)
        {
            Debug.LogError(
                "[OldmanGazeTarget] 没找到需要处理的 Renderer。"
            );

            return;
        }

        SaveOriginalMaterials();

        ApplyGrayscale();

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

        if (hasRevealedColor)
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

            if (gazeTimer >= requiredGazeTime)
            {
                RevealColor();
            }
        }
        else
        {
            lookAwayTimer += Time.deltaTime;

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
        if (desktopMode)
        {
            return IsDesktopLookingAtOldman();
        }

        return IsVRLookingAtOldman();
    }


    // ======================================================
    // Desktop鼠标视线
    // ======================================================

    private bool IsDesktopLookingAtOldman()
    {
        if (desktopCamera == null)
            return false;

        if (desktopMouseGaze == null)
            return false;

        Vector2 screenPosition =
            desktopMouseGaze.GetGazeScreenPosition();

        Ray ray =
            desktopCamera.ScreenPointToRay(
                screenPosition
            );

        if (showDesktopRay)
        {
            Debug.DrawRay(
                ray.origin,
                ray.direction * desktopRayDistance,
                Color.green
            );
        }

        RaycastHit hit;

        if (!Physics.Raycast(
                ray,
                out hit,
                desktopRayDistance))
        {
            return false;
        }

        if (hit.collider == null)
            return false;

        Transform hitTransform =
            hit.collider.transform;

        if (showDebugLog)
        {
            Debug.Log(
                "[Oldman Desktop Gaze] 当前看到：" +
                hitTransform.name
            );
        }

        return IsTargetTransform(
            hitTransform
        );
    }


    // ======================================================
    // VR真实眼动
    // ======================================================

    private bool IsVRLookingAtOldman()
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

        return IsTargetTransform(
            hitTransform
        );
    }


    // ======================================================
    // 判断命中的物体是不是 gazeTargets
    // ======================================================

    private bool IsTargetTransform(
        Transform hitTransform)
    {
        if (hitTransform == null)
            return false;

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

            if (hitTransform == target)
                return true;

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


        // ==================================================
        // Keyboard模式：
        // 老人恢复彩色以后开放 E 帮助
        // ==================================================

        if (desktopMode &&
            desktopHelpShortcut != null)
        {
            desktopHelpShortcut.ShowHelpHint();

            if (showDebugLog)
            {
                Debug.Log(
                    "[OldmanGazeTarget] Keyboard E 帮助已解锁。"
                );
            }
        }


        // 开始原来的求助对话
        StartFirstDialogue();
    }


    // ======================================================
    // 第一次求助
    // ======================================================

    private void StartFirstDialogue()
    {
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

        // 播放语音
        if (oldmanAudioSource != null)
        {
            if (helpVoiceClip != null)
            {
                oldmanAudioSource.clip =
                    helpVoiceClip;
            }

            if (oldmanAudioSource.clip != null)
            {
                oldmanAudioSource.Play();

                while (oldmanAudioSource.isPlaying)
                {
                    yield return null;
                }
            }
            else
            {
                yield return
                    new WaitForSeconds(3f);
            }
        }
        else
        {
            yield return
                new WaitForSeconds(3f);
        }

        // 语音结束关闭字幕
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }

        // 老人说完话以后开始箱子任务
        if (boxTaskManager != null)
        {
            boxTaskManager.BeginBoxTask();

            if (showDebugLog)
            {
                Debug.Log(
                    "[OldmanGazeTarget] 老人说完话，两个箱子任务开始。"
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "[OldmanGazeTarget] BoxTaskManager 没有拖入。"
            );
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

        // Keyboard E 也一起重新锁住
        if (desktopHelpShortcut != null)
        {
            desktopHelpShortcut.ResetDesktopHelp();
        }

        ApplyGrayscale();

        Debug.Log(
            "[OldmanGazeTarget] 老人已重新变成黑白灰。"
        );
    }
}