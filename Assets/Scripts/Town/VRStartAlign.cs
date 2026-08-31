using System.Collections;
using UnityEngine;

public class VRStartAlign : MonoBehaviour
{
    [Header("要移动的玩家总父物体")]
    public Transform introPlayerRoot;

    [Header("XR里的Main Camera")]
    public Transform vrCamera;

    [Header("床上眼睛目标位置")]
    public Transform introStartView;

    [Header("等待头显初始化")]
    public float delay = 0.5f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);

        AlignPlayer();
    }

    public void AlignPlayer()
    {
        if (introPlayerRoot == null ||
            vrCamera == null ||
            introStartView == null)
        {
            Debug.LogWarning("VRStartAlign：有引用没有拖入！");
            return;
        }

        // ---------- 1. 先对齐旋转 ----------
        // 计算：需要把玩家根节点旋转多少，
        // 才能让当前VR Camera朝向 = IntroStartView朝向
        Quaternion rotationOffset =
            introStartView.rotation *
            Quaternion.Inverse(vrCamera.rotation);

        introPlayerRoot.rotation =
            rotationOffset * introPlayerRoot.rotation;

        // ---------- 2. 再对齐位置 ----------
        // 旋转后重新读取Camera位置
        Vector3 positionOffset =
            introStartView.position - vrCamera.position;

        introPlayerRoot.position += positionOffset;
    }
}