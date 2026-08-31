using UnityEngine;
using UnityEngine.UI;

using VIVE.OpenXR;
using VIVE.OpenXR.EyeTracker;

public class ProjectEyeTrackerDisplay : MonoBehaviour
{
    [Header("眼动可视化")]
    public Transform leftGazeTransform;
    public Transform rightGazeTransform;

    [Header("XR Tracking Origin")]
    [Tooltip("拖入正式场景最外层的 XR Origin / XR Rig")]
    public Transform trackingOrigin;

    [Header("调试文字（可选）")]
    public Text debugText;

    [Header("调试设置")]
    public bool showDebugText = true;


    private void Awake()
    {
        // 如果 Inspector 没有手动指定 Text
        // 就尝试从当前物体上获取
        if (debugText == null)
        {
            debugText = GetComponent<Text>();
        }
    }


    private void Update()
    {
        // =====================================================
        // 1. 获取 VIVE 眼动数据
        // =====================================================

        XR_HTC_eye_tracker.Interop.GetEyeGazeData(
            out XrSingleEyeGazeDataHTC[] gazes
        );


        // =====================================================
        // 2. 检查是否成功拿到数据
        // =====================================================

        if (gazes == null)
        {
            ShowMessage("Eye gaze data unavailable.");
            return;
        }


        int leftIndex =
            (int)XrEyePositionHTC.XR_EYE_POSITION_LEFT_HTC;

        int rightIndex =
            (int)XrEyePositionHTC.XR_EYE_POSITION_RIGHT_HTC;


        if (leftIndex < 0 ||
            rightIndex < 0 ||
            leftIndex >= gazes.Length ||
            rightIndex >= gazes.Length)
        {
            ShowMessage("Eye gaze data array invalid.");
            return;
        }


        // =====================================================
        // 3. 获取左右眼数据
        // =====================================================

        XrSingleEyeGazeDataHTC leftGaze =
            gazes[leftIndex];

        XrSingleEyeGazeDataHTC rightGaze =
            gazes[rightIndex];


        // =====================================================
        // 4. 更新左眼
        // =====================================================

        if (leftGazeTransform != null)
        {
            if (leftGaze.isValid != 0)
            {
                Vector3 localPosition =
                    leftGaze.gazePose.position.ToUnityVector();

                Quaternion localRotation =
                    leftGaze.gazePose.orientation.ToUnityQuaternion();


                // 如果有 XR Origin
                // 把 Tracking Space 坐标转换成 World Space
                if (trackingOrigin != null)
                {
                    leftGazeTransform.position =
                        trackingOrigin.TransformPoint(localPosition);

                    leftGazeTransform.rotation =
                        trackingOrigin.rotation * localRotation;
                }
                else
                {
                    // 没拖 Tracking Origin 时的备用方式
                    leftGazeTransform.position = localPosition;
                    leftGazeTransform.rotation = localRotation;
                }


                if (!leftGazeTransform.gameObject.activeSelf)
                {
                    leftGazeTransform.gameObject.SetActive(true);
                }
            }
            else
            {
                if (leftGazeTransform.gameObject.activeSelf)
                {
                    leftGazeTransform.gameObject.SetActive(false);
                }
            }
        }


        // =====================================================
        // 5. 更新右眼
        // =====================================================

        if (rightGazeTransform != null)
        {
            if (rightGaze.isValid != 0)
            {
                Vector3 localPosition =
                    rightGaze.gazePose.position.ToUnityVector();

                Quaternion localRotation =
                    rightGaze.gazePose.orientation.ToUnityQuaternion();


                if (trackingOrigin != null)
                {
                    rightGazeTransform.position =
                        trackingOrigin.TransformPoint(localPosition);

                    rightGazeTransform.rotation =
                        trackingOrigin.rotation * localRotation;
                }
                else
                {
                    rightGazeTransform.position = localPosition;
                    rightGazeTransform.rotation = localRotation;
                }


                if (!rightGazeTransform.gameObject.activeSelf)
                {
                    rightGazeTransform.gameObject.SetActive(true);
                }
            }
            else
            {
                if (rightGazeTransform.gameObject.activeSelf)
                {
                    rightGazeTransform.gameObject.SetActive(false);
                }
            }
        }


        // =====================================================
        // 6. 显示调试数据
        // =====================================================

        if (showDebugText && debugText != null)
        {
            Vector3 leftLocalPos =
                leftGaze.gazePose.position.ToUnityVector();

            Quaternion leftLocalRot =
                leftGaze.gazePose.orientation.ToUnityQuaternion();

            Vector3 rightLocalPos =
                rightGaze.gazePose.position.ToUnityVector();

            Quaternion rightLocalRot =
                rightGaze.gazePose.orientation.ToUnityQuaternion();


            Vector3 leftWorldPos;
            Quaternion leftWorldRot;

            Vector3 rightWorldPos;
            Quaternion rightWorldRot;


            if (trackingOrigin != null)
            {
                leftWorldPos =
                    trackingOrigin.TransformPoint(leftLocalPos);

                leftWorldRot =
                    trackingOrigin.rotation * leftLocalRot;

                rightWorldPos =
                    trackingOrigin.TransformPoint(rightLocalPos);

                rightWorldRot =
                    trackingOrigin.rotation * rightLocalRot;
            }
            else
            {
                leftWorldPos = leftLocalPos;
                leftWorldRot = leftLocalRot;

                rightWorldPos = rightLocalPos;
                rightWorldRot = rightLocalRot;
            }


            debugText.text =
                "[Eye Tracker]\n\n" +

                "LEFT EYE\n" +
                "Valid: " + leftGaze.isValid + "\n" +

                "Local Position:\n" +
                leftLocalPos.x.ToString("F4") + ", " +
                leftLocalPos.y.ToString("F4") + ", " +
                leftLocalPos.z.ToString("F4") + "\n" +

                "World Position:\n" +
                leftWorldPos.x.ToString("F4") + ", " +
                leftWorldPos.y.ToString("F4") + ", " +
                leftWorldPos.z.ToString("F4") + "\n" +

                "Rotation:\n" +
                leftWorldRot.x.ToString("F4") + ", " +
                leftWorldRot.y.ToString("F4") + ", " +
                leftWorldRot.z.ToString("F4") + ", " +
                leftWorldRot.w.ToString("F4") + "\n\n" +


                "RIGHT EYE\n" +
                "Valid: " + rightGaze.isValid + "\n" +

                "Local Position:\n" +
                rightLocalPos.x.ToString("F4") + ", " +
                rightLocalPos.y.ToString("F4") + ", " +
                rightLocalPos.z.ToString("F4") + "\n" +

                "World Position:\n" +
                rightWorldPos.x.ToString("F4") + ", " +
                rightWorldPos.y.ToString("F4") + ", " +
                rightWorldPos.z.ToString("F4") + "\n" +

                "Rotation:\n" +
                rightWorldRot.x.ToString("F4") + ", " +
                rightWorldRot.y.ToString("F4") + ", " +
                rightWorldRot.z.ToString("F4") + ", " +
                rightWorldRot.w.ToString("F4");
        }
    }


    // =========================================================
    // 调试信息
    // =========================================================

    private void ShowMessage(string message)
    {
        if (showDebugText && debugText != null)
        {
            debugText.text =
                "[Eye Tracker]\n\n" +
                message;
        }
    }
}