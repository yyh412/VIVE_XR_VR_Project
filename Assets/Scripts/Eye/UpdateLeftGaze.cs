using UnityEngine;
using VIVE.OpenXR;
using VIVE.OpenXR.EyeTracker;

public class UpdateLeftGaze : MonoBehaviour
{
    void Update()
    {
        bool success = XR_HTC_eye_tracker.Interop.GetEyeGazeData(
            out XrSingleEyeGazeDataHTC[] gazes
        );

        // 没拿到眼动数据时，直接停止
        if (!success || gazes == null || gazes.Length == 0)
            return;

        int leftIndex =
            (int)XrEyePositionHTC.XR_EYE_POSITION_LEFT_HTC;

        if (leftIndex >= gazes.Length)
            return;

        var leftGaze = gazes[leftIndex];

        if (!leftGaze.isValid)
            return;

        transform.position =
            leftGaze.gazePose.position.ToUnityVector();

        transform.rotation =
            leftGaze.gazePose.orientation.ToUnityQuaternion();
    }
}