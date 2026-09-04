using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class OldmanEscortController : MonoBehaviour
{
    // =====================================================
    // 基础引用
    // =====================================================

    [Header("老人")]
    [Tooltip("拖入 Oldman")]
    public Transform oldmanRoot;

    [Header("老人 NavMeshAgent")]
    [Tooltip("拖入 Oldman 上面的 NavMeshAgent")]
    public NavMeshAgent agent;

    [Header("玩家 VR 相机")]
    [Tooltip("拖入 XR Origin 下面的 Main Camera")]
    public Transform playerHead;

    [Header("电梯目标点")]
    [Tooltip("拖入 ElevatorTarget")]
    public Transform elevatorTarget;


    // =====================================================
    // 对话框
    // =====================================================

    [Header("老人对话框")]
    [Tooltip("拖入 OldmanSpeechBubble")]
    public GameObject speechBubble;

    [Header("字幕文字")]
    [Tooltip("拖入 OldmanSpeechBubble 下面的 DialogueText")]
    public TMP_Text dialogueText;

    [Header("老人 Audio Source")]
    [Tooltip("拖入老人使用的 AudioSource")]
    public AudioSource oldmanAudioSource;


    // =====================================================
    // 箱子完成后的第二句话
    // =====================================================

    [Header("箱子完成后的语音")]
    [Tooltip("Thank you! Are you here for the interview? Follow me.")]
    public AudioClip escortVoiceClip;

    [Header("箱子完成后的字幕")]
    [TextArea(2, 4)]
    public string escortText =
        "Thank you! Are you here for the interview? Follow me.";


    // =====================================================
    // 路上等待玩家
    // =====================================================

    [Header("Follow me 语音")]
    [Tooltip("只说 Follow me.")]
    public AudioClip followMeVoiceClip;

    [Header("Follow me 字幕")]
    [TextArea(1, 2)]
    public string followMeText =
        "Follow me.";


    // =====================================================
    // 电梯最后一句
    // =====================================================

    [Header("电梯最后一句语音")]
    [Tooltip("Press the elevator button and go straight to the third floor. Good luck!")]
    public AudioClip elevatorInstructionVoiceClip;

    [Header("电梯最后一句字幕")]
    [TextArea(2, 4)]
    public string elevatorInstructionText =
        "Press the elevator button and go straight to the third floor. Good luck!";

    [Header("最后一句触发距离")]
    [Tooltip("玩家距离老人多少米以内，老人说最后一句")]
    public float finalTalkDistance = 2f;


    // =====================================================
    // 老人移动
    // =====================================================

    [Header("老人移动")]
    [Tooltip("老人移动速度")]
    public float moveSpeed = 1.2f;

    [Tooltip("距离 ElevatorTarget 多近算到达")]
    public float elevatorStoppingDistance = 0.5f;


    // =====================================================
    // 玩家跟随检测
    // =====================================================

    [Header("玩家跟随检测")]
    [Tooltip("玩家距离老人超过这个距离，老人停下")]
    public float waitDistance = 5f;

    [Tooltip("玩家走到这个距离以内，老人继续走")]
    public float resumeDistance = 1.5f;

    [Tooltip("老人停下来以后，每隔多少秒重复一次 Follow me")]
    public float followMeRepeatInterval = 10f;


    // =====================================================
    // 环境恢复
    // =====================================================

    [Header("环境恢复彩色")]
    [Tooltip("暂时可以留空")]
    public GameObject environmentColorController;


    // =====================================================
    // 电脑测试
    // =====================================================

    [Header("电脑测试")]
    [Tooltip("勾选以后，按 T 直接让老人走向电梯")]
    public bool allowKeyboardMoveTest = false;

    [Tooltip("勾选以后，按 Y 模拟两个箱子完成")]
    public bool allowCompleteTaskTest = false;


    // =====================================================
    // Debug
    // =====================================================

    [Header("Debug")]
    public bool showDebugLog = true;


    // =====================================================
    // 内部状态
    // =====================================================

    private bool escortStarted = false;

    private bool isMovingToElevator = false;

    private bool isWaitingForPlayer = false;

    private bool hasReachedElevator = false;

    private bool finalDialogueStarted = false;

    private Coroutine followMeCoroutine;

    private Coroutine finalDialogueCoroutine;


    // =====================================================
    // Start
    // =====================================================

    private void Start()
    {
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }


        if (agent != null)
        {
            agent.speed = moveSpeed;

            agent.stoppingDistance =
                elevatorStoppingDistance;

            agent.isStopped = true;
        }
    }


    // =====================================================
    // Update
    // =====================================================

    private void Update()
    {
        // -------------------------------------------------
        // 电脑测试
        // -------------------------------------------------

        if (allowKeyboardMoveTest &&
            Input.GetKeyDown(KeyCode.T))
        {
            DebugLog(
                "T 测试：直接前往电梯。"
            );

            MoveToElevator();
        }


        if (allowCompleteTaskTest &&
            Input.GetKeyDown(KeyCode.Y))
        {
            DebugLog(
                "Y 测试：模拟箱子全部完成。"
            );

            StartEscortSequence();
        }


        // -------------------------------------------------
        // 正在前往电梯
        // -------------------------------------------------

        if (isMovingToElevator &&
            !hasReachedElevator)
        {
            CheckPlayerDistance();
        }


        // -------------------------------------------------
        // 检查是否到达电梯
        // -------------------------------------------------

        CheckElevatorArrival();


        // -------------------------------------------------
        // 已经到电梯以后
        // -------------------------------------------------

        if (hasReachedElevator)
        {
            HandleElevatorWaiting();
        }
    }


    // =====================================================
    // 两个箱子全部完成后调用
    // =====================================================

    public void StartEscortSequence()
    {
        if (escortStarted)
            return;


        escortStarted = true;


        DebugLog(
            "两个箱子完成，开始老人引导流程。"
        );


        StartCoroutine(
            EscortRoutine()
        );
    }


    // =====================================================
    // 箱子完成后的第二段对话
    // =====================================================

    private IEnumerator EscortRoutine()
    {
        // 设置文字
        if (dialogueText != null)
        {
            dialogueText.text =
                escortText;
        }


        // 显示文本框
        if (speechBubble != null)
        {
            speechBubble.SetActive(true);
        }


        // 播放语音
        if (oldmanAudioSource != null &&
            escortVoiceClip != null)
        {
            oldmanAudioSource.Stop();

            oldmanAudioSource.clip =
                escortVoiceClip;

            oldmanAudioSource.Play();


            DebugLog(
                "老人说：Thank you... Follow me."
            );


            while (
                oldmanAudioSource.isPlaying
            )
            {
                yield return null;
            }
        }
        else
        {
            // 没放语音时显示 3 秒
            yield return
                new WaitForSeconds(3f);
        }


        // 第二段说完关闭文本框
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }


        // 环境恢复彩色
        RevealEnvironmentColor();


        yield return
            new WaitForSeconds(0.3f);


        // 开始去电梯
        MoveToElevator();
    }


    // =====================================================
    // 老人开始前往电梯
    // =====================================================

    public void MoveToElevator()
    {
        if (agent == null)
        {
            Debug.LogWarning(
                "[OldmanEscortController] Agent 没有拖入。"
            );

            return;
        }


        if (elevatorTarget == null)
        {
            Debug.LogWarning(
                "[OldmanEscortController] ElevatorTarget 没有拖入。"
            );

            return;
        }


        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning(
                "[OldmanEscortController] Oldman 不在 NavMesh 上。"
            );

            return;
        }


        // 重置状态
        hasReachedElevator = false;

        finalDialogueStarted = false;

        isWaitingForPlayer = false;

        isMovingToElevator = true;


        agent.speed =
            moveSpeed;

        agent.stoppingDistance =
            elevatorStoppingDistance;

        agent.isStopped =
            false;


        bool success =
            agent.SetDestination(
                elevatorTarget.position
            );


        if (success)
        {
            DebugLog(
                "老人开始前往电梯。"
            );
        }
        else
        {
            Debug.LogWarning(
                "[OldmanEscortController] 无法生成前往电梯的路径。"
            );
        }
    }


    // =====================================================
    // 路上检测玩家距离
    // =====================================================

    private void CheckPlayerDistance()
    {
        if (playerHead == null ||
            oldmanRoot == null)
        {
            return;
        }


        float distance =
            GetHorizontalDistance(
                oldmanRoot.position,
                playerHead.position
            );


        // 玩家没有掉队
        if (!isWaitingForPlayer)
        {
            if (distance >
                waitDistance)
            {
                StartWaitingForPlayer();
            }
        }

        // 玩家已经掉队
        else
        {
            if (distance <=
                resumeDistance)
            {
                ResumeEscort();
            }
        }
    }


    // =====================================================
    // 玩家掉队，老人停下
    // =====================================================

    private void StartWaitingForPlayer()
    {
        if (isWaitingForPlayer)
            return;


        isWaitingForPlayer =
            true;


        // 停止走路
        if (agent != null &&
            agent.isOnNavMesh)
        {
            agent.isStopped =
                true;
        }


        // 显示 Follow me
        ShowFollowMeBubble();


        // 播放 Follow me
        PlayFollowMeVoice();


        // 开始重复提醒
        if (followMeCoroutine != null)
        {
            StopCoroutine(
                followMeCoroutine
            );
        }


        followMeCoroutine =
            StartCoroutine(
                FollowMeRepeatRoutine()
            );


        DebugLog(
            "玩家距离超过 " +
            waitDistance +
            " 米，老人停下来等待。"
        );


        // 老人不转身
        // 只让 SpeechBubble 自己面对玩家
    }


    // =====================================================
    // 每隔一段时间重复 Follow me
    // =====================================================

    private IEnumerator FollowMeRepeatRoutine()
    {
        while (
            isWaitingForPlayer &&
            !hasReachedElevator
        )
        {
            yield return
                new WaitForSeconds(
                    followMeRepeatInterval
                );


            if (!isWaitingForPlayer ||
                hasReachedElevator)
            {
                break;
            }


            PlayFollowMeVoice();
        }


        followMeCoroutine =
            null;
    }


    // =====================================================
    // 显示 Follow me 文本框
    // =====================================================

    private void ShowFollowMeBubble()
    {
        if (dialogueText != null)
        {
            dialogueText.text =
                followMeText;
        }


        if (speechBubble != null)
        {
            speechBubble.SetActive(true);
        }
    }


    // =====================================================
    // 播放 Follow me
    // =====================================================

    private void PlayFollowMeVoice()
    {
        if (oldmanAudioSource == null ||
            followMeVoiceClip == null)
        {
            return;
        }


        oldmanAudioSource.Stop();

        oldmanAudioSource.clip =
            followMeVoiceClip;

        oldmanAudioSource.Play();


        DebugLog(
            "老人说：Follow me."
        );
    }


    // =====================================================
    // 玩家追上以后继续走
    // =====================================================

    private void ResumeEscort()
    {
        if (!isWaitingForPlayer)
            return;


        isWaitingForPlayer =
            false;


        // 停止 Follow me 循环
        if (followMeCoroutine != null)
        {
            StopCoroutine(
                followMeCoroutine
            );

            followMeCoroutine =
                null;
        }


        // 停止 Follow me 声音
        if (oldmanAudioSource != null &&
            followMeVoiceClip != null &&
            oldmanAudioSource.clip ==
            followMeVoiceClip)
        {
            oldmanAudioSource.Stop();
        }


        // 关闭 Follow me 文本框
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }


        // 继续前往电梯
        if (agent != null &&
            agent.isOnNavMesh &&
            elevatorTarget != null)
        {
            agent.isStopped =
                false;

            agent.SetDestination(
                elevatorTarget.position
            );
        }


        DebugLog(
            "玩家已经追上，老人继续走。"
        );


        // 老人不转身
    }


    // =====================================================
    // 检查是否到达电梯
    // =====================================================

    private void CheckElevatorArrival()
    {
        if (!isMovingToElevator)
            return;


        if (isWaitingForPlayer)
            return;


        if (agent == null)
            return;


        if (!agent.isOnNavMesh)
            return;


        if (agent.pathPending)
            return;


        if (agent.remainingDistance >
            agent.stoppingDistance)
        {
            return;
        }


        if (agent.hasPath &&
            agent.velocity.sqrMagnitude >
            0.01f)
        {
            return;
        }


        // =================================================
        // 老人到达电梯
        // =================================================

        isMovingToElevator =
            false;

        hasReachedElevator =
            true;

        isWaitingForPlayer =
            false;


        agent.isStopped =
            true;


        // 停止 Follow me 协程
        if (followMeCoroutine != null)
        {
            StopCoroutine(
                followMeCoroutine
            );

            followMeCoroutine =
                null;
        }


        // 停止 Follow me 声音
        if (oldmanAudioSource != null &&
            followMeVoiceClip != null &&
            oldmanAudioSource.clip ==
            followMeVoiceClip)
        {
            oldmanAudioSource.Stop();
        }


        // 先关闭之前的 Follow me
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }


        DebugLog(
            "老人已经到达 ElevatorTarget，保持当前位置和朝向，等待玩家靠近。"
        );


        // 不转 Oldman
        // 不转 Wheelchair
        // 不转 Neck
    }


    // =====================================================
    // 到电梯以后等待玩家进入指定距离
    // =====================================================

    private void HandleElevatorWaiting()
    {
        // 最后一句已经说过
        if (finalDialogueStarted)
            return;


        if (playerHead == null ||
            oldmanRoot == null)
        {
            return;
        }


        float distance =
            GetHorizontalDistance(
                oldmanRoot.position,
                playerHead.position
            );


        // 玩家进入指定范围
        if (distance <=
            finalTalkDistance)
        {
            StartFinalDialogue();
        }
    }


    // =====================================================
    // 开始最后一句
    // =====================================================

    private void StartFinalDialogue()
    {
        if (finalDialogueStarted)
            return;


        // 马上设 true
        // 保证只说一次
        finalDialogueStarted =
            true;


        if (finalDialogueCoroutine != null)
        {
            StopCoroutine(
                finalDialogueCoroutine
            );
        }


        finalDialogueCoroutine =
            StartCoroutine(
                FinalDialogueRoutine()
            );
    }


    // =====================================================
    // 最后一段对话
    // =====================================================

    private IEnumerator FinalDialogueRoutine()
    {
        // 最后一句文字
        if (dialogueText != null)
        {
            dialogueText.text =
                elevatorInstructionText;
        }


        // 打开文本框
        if (speechBubble != null)
        {
            speechBubble.SetActive(true);
        }


        DebugLog(
            "玩家进入 " +
            finalTalkDistance +
            " 米范围，老人说最后一句。"
        );


        // 播放最后一句
        if (oldmanAudioSource != null &&
            elevatorInstructionVoiceClip != null)
        {
            oldmanAudioSource.Stop();

            oldmanAudioSource.clip =
                elevatorInstructionVoiceClip;

            oldmanAudioSource.Play();


            while (
                oldmanAudioSource.isPlaying
            )
            {
                yield return null;
            }
        }


        // =================================================
        // 不关闭文本框
        //
        // Press the elevator button...
        // 这句话会一直显示
        // =================================================


        DebugLog(
            "最后一句播放完成，文本框保持显示。"
        );


        finalDialogueCoroutine =
            null;
    }


    // =====================================================
    // 环境恢复彩色
    // =====================================================

    private void RevealEnvironmentColor()
    {
        if (environmentColorController == null)
        {
            DebugLog(
                "Environment Color Controller 暂时为空，跳过环境恢复。"
            );

            return;
        }


        environmentColorController.SendMessage(
            "RevealColor",
            SendMessageOptions.DontRequireReceiver
        );


        DebugLog(
            "环境恢复彩色。"
        );
    }


    // =====================================================
    // 计算水平距离
    // =====================================================

    private float GetHorizontalDistance(
        Vector3 a,
        Vector3 b
    )
    {
        a.y = 0f;

        b.y = 0f;


        return Vector3.Distance(
            a,
            b
        );
    }


    // =====================================================
    // Debug
    // =====================================================

    private void DebugLog(
        string message
    )
    {
        if (!showDebugLog)
            return;


        Debug.Log(
            "[OldmanEscortController] " +
            message
        );
    }
}