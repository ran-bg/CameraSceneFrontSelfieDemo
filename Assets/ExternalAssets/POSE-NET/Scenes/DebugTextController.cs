using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugTextController : MonoBehaviour
{
    [SerializeField] Text DebugDetectPose = default;
    [SerializeField] Text DebugLowScoreSegment = default;
    [SerializeField] Text DebugAllSegmentInFrame = default;

    public float lowScore = 0.4f;
    // Start is called before the first frame update
    // Update is called once per frame
    public void UpdateDebugTexts(PoseNet.Pose[] poses)
    {
        var numPoses = poses.Length;

        if (numPoses == 0)
        {
#if !ENV_PRODUCTION
            Debug.Log("no pose");
#endif
            ClearDebugTexts();
            return;
        }

#if !ENV_PRODUCTION
        Debug.Log($"Pose Found {numPoses}");
#endif

        if (numPoses >= 2)
        {
            DetectMultiPoses();
            return;
        }

        UpdateDebugDetectPoseText(numPoses);

    }

    void UpdateDebugDetectPoseText(int num)
    {
        DebugDetectPose.text = $"Detect Pose";
    }

    void ClearDebugTexts()
    {
        DebugDetectPose.text = "NO POSE ------";
        DebugLowScoreSegment.text = "";
        DebugAllSegmentInFrame.text = "";
    }

    void DetectMultiPoses()
    {
        DebugDetectPose.text = "Multi Pose Detected!!!";
        DebugLowScoreSegment.text = "";
        DebugAllSegmentInFrame.text = "";
    }
}
