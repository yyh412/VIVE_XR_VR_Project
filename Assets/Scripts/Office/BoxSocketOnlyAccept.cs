using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BoxSocketOnlyAccept : MonoBehaviour
{
    [Header("这个 Socket 唯一允许的箱子")]
    [Tooltip("拖入对应箱子的 XR Grab Interactable")]
    public XRGrabInteractable allowedBox;

    private XRSocketInteractor socket;


    private void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();

        if (socket == null)
        {
            Debug.LogError(
                "[BoxSocketOnlyAccept] " +
                gameObject.name +
                " 上没有 XR Socket Interactor。"
            );
        }
    }


    private void OnEnable()
    {
        if (socket != null)
        {
            socket.selectEntered.AddListener(
                OnBoxEntered
            );
        }
    }


    private void OnDisable()
    {
        if (socket != null)
        {
            socket.selectEntered.RemoveListener(
                OnBoxEntered
            );
        }
    }


    private void OnBoxEntered(
        SelectEnterEventArgs args
    )
    {
        if (allowedBox == null)
        {
            Debug.LogWarning(
                "[BoxSocketOnlyAccept] " +
                gameObject.name +
                " 没有设置 Allowed Box。"
            );

            return;
        }


        IXRSelectInteractable entered =
            args.interactableObject;


        if (entered == null)
            return;


        // 正确箱子
        if (entered ==
            (IXRSelectInteractable)allowedBox)
        {
            Debug.Log(
                "[BoxSocketOnlyAccept] " +
                gameObject.name +
                " 接收到正确箱子：" +
                allowedBox.gameObject.name
            );

            return;
        }


        // =================================================
        // 错误箱子
        // =================================================

        Debug.Log(
            "[BoxSocketOnlyAccept] " +
            gameObject.name +
            " 拒绝错误箱子：" +
            entered.transform.gameObject.name
        );


        // 立即让 Socket 松开错误箱子
        if (socket.interactionManager != null)
        {
            socket.interactionManager.SelectExit(
                socket,
                entered
            );
        }
    }
}