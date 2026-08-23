using UnityEngine;
using System.Collections;

public class DoorLiftController : MonoBehaviour
{
    [Header("要升起的门")]
    public Transform door;

    [Header("向上移动的距离")]
    public float liftHeight = 5f;

    [Header("开门时间")]
    public float liftDuration = 2f;

    private bool opened = false;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private void Start()
    {
        if (door == null)
            return;

        closedPosition = door.localPosition;

        openPosition =
            closedPosition + Vector3.up * liftHeight;
    }

    public void OpenDoor()
    {
        if (opened || door == null)
            return;

        opened = true;

        StartCoroutine(LiftDoor());
    }

    private IEnumerator LiftDoor()
    {
        Vector3 startPosition = door.localPosition;

        float time = 0f;

        while (time < liftDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(
                time / liftDuration
            );

            // 平滑启动和平滑停止
            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            door.localPosition =
                Vector3.Lerp(
                    startPosition,
                    openPosition,
                    t
                );

            yield return null;
        }

        door.localPosition = openPosition;

        Debug.Log("门已经向上打开");
    }
}