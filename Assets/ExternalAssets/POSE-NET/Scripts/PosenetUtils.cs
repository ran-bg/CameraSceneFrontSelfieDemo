using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PosenetUtils : MonoBehaviour
{
    public static void DebugPosesValues(PoseNet.Pose[] poses)
    {
        foreach (var pose in poses)
        {
#if !ENV_PRODUCTION
            Debug.Log($"Pose score {pose.score}");
#endif
            var keypoints = pose.keypoints;

            foreach (var keypoint in keypoints)
            {
#if !ENV_PRODUCTION
                Debug.Log($"{keypoint.part} {keypoint.score}");
#endif
            }
        }
    }
}
