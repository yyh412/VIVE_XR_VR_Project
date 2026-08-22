using System.Collections;
using UnityEngine;

public class FixStartMenu : MonoBehaviour
{
    public Transform headCamera;

    public float distance = 2f;

    public float verticalOffset = 0f;

    IEnumerator Start()
    {
        // 等待一下，让 VR 头显完成定位
        yield return new WaitForSeconds(0.2f);

        if (headCamera == null)
        {
            Camera cam = Camera.main;

            if (cam != null)
                headCamera = cam.transform;
        }

        if (headCamera == null)
        {
            Debug.LogError("FixStartMenu: 找不到 Main Camera");
            yield break;
        }

        // 只取头显水平方向，避免玩家低头时菜单跑到地面
        Vector3 forward = headCamera.forward;
        forward.y = 0f;
        forward.Normalize();

        // 菜单放在玩家正前方
        transform.position =
            headCamera.position +
            forward * distance +
            Vector3.up * verticalOffset;

        // 菜单朝向玩家
        transform.rotation = Quaternion.LookRotation(forward);
    }
}