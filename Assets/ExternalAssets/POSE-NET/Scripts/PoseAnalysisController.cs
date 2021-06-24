using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Language;

namespace PoseNetScripts
{
    public class PoseAnalysisController : MonoBehaviour
    {
        public bool isFindPose = false;
        public bool isMultiPose = false;

        public bool isCenterPlaced = false;
        public bool isAllPartsInView = false;

        public bool isAllPartsInInside = false;

        public bool isFrontPoseOk = false;

        public bool isSidePoseOk = false;

        public float thScore = 0.05f;

        public int _texH = 0;
        public int _texW = 0;

        public float _heightRatio = 1f;

        private float FRONT_ARM_ANGLE = 110f;
        private float MIN_FRONT_DISTANCE = 12f;

//        private float MIN_INSIDE = 0.1f;
//        private float MAX_INSIDE = 0.95f;

//        private float NEAR_TOP = 0.3f;
//        private float NEAR_BOTTOM = 0.7f;

        private float ANKLE_TO_FOOT_RATIO = 0.05f;
        private float NOSE_TO_FOOT_RATIO = 0.1f;

        private float TH_POSITION = 0.05f;

        private bool isFrontPoseInFrame = false;
        private bool isSidePoseInFrame = false;

        public void UpdateState(PoseNet.Pose[] poses)
        {
            ResetValues();
            var numPoses = poses.Length;
            if (numPoses == 0)
            {
                return;
            }

            isFindPose = true;

            isCenterPlaced = CheckCenterPlaced(poses);
            if (!isCenterPlaced) return;

            isAllPartsInView = CheckAllPartsInView(poses);
            if (!isAllPartsInView) return;

            // isAllPartsInInside = CheckAllPartsInInside(poses);
            // if (!isAllPartsInInside) return;

            // isFrontPoseOk = CheckFrontAPose(poses);
            // isSidePoseOk = CheckSidePose(poses);
        }

        // success: 0
        // pose not found: 1
        // all parts not in view: 2
        // all parts not in inside: 3
        // not center: 4
        // front pose: 5xx
        // side pose: 6xx

        public int ValidateFrontPose(PoseNet.Pose[] poses)
        {
            ResetValues();
            var numPoses = poses.Length;
            if (numPoses == 0)
            {
                return 100;
            }

            isFindPose = true;

            // Place person in frame first
            if (!isFrontPoseInFrame) 
            {
                //全身が入っていない
                isAllPartsInView = CheckAllPartsInView(poses);
                if (!isAllPartsInView) return 200;

                //　近すぎ or 遠すぎる
                var positionCheck = CheckAllPartsInInside(poses);
                if (positionCheck != 0) return positionCheck;
            }

            // Once person is in frame, do pose check
             isFrontPoseInFrame = true;

            isCenterPlaced = CheckCenterPlaced(poses);
            if (!isCenterPlaced) return 400;

            var poseCheck = CheckFrontAPose(poses);
            return poseCheck;
        }

        public int ValidateSidePose(PoseNet.Pose[] poses)
        {
            ResetValues();
            var numPoses = poses.Length;
            if (numPoses == 0)
            {
                return 100;
            }

            isFindPose = true;

            // Place person in frame first
            if (!isSidePoseInFrame) 
            {
                isAllPartsInView = CheckAllPartsInViewSide(poses);
                if (!isAllPartsInView) return 200;

                var positionCheck = CheckAllPartsInInside(poses);
                if (positionCheck != 0) return positionCheck;
            }

            // Once person is in frame, do pose check
            isSidePoseInFrame = true;                

            isCenterPlaced = CheckCenterPlacedSide(poses);
            if (!isCenterPlaced) return 400;

            var poseCheck = CheckSidePose(poses);
            return poseCheck;
        }

        private void ResetValues()
        {
            isFindPose = false;
            isMultiPose = false;
            isAllPartsInView = false;
            isAllPartsInInside = false;
            isCenterPlaced = false;
            isFrontPoseOk = false;
            isSidePoseOk = false;
        }

        private bool CheckCenterPlaced(PoseNet.Pose[] poses)
        {
            bool result = true;
            var pose = poses[0];
            var keypoints = pose.keypoints;
            var nose = FindKeypoint(keypoints, "nose");

            // Debug.Log($"nose Position:::{nose.part} {nose.position.x} {nose.position.y}");
            int _Width, _Height;

            //frame.ResizePro(Width, Height, false, false);
            if (_texW > _texH)
            {
                _Width = 257;
                _Height = Mathf.CeilToInt(257f * ((float)_texH / (float)_texW));
            }
            else
            {
                _Width = Mathf.CeilToInt(257f * ((float)_texW / (float)_texH));
                _Height = 257;
            }

            float _noseX = nose.position.x / (float)_Width;
            float _noseY = nose.position.y / (float)_Height;
            if (_noseX > 0.6 || _noseX < 0.4)
            {
                // Debug.Log($"Not CenterPosition:::{nose.part} {_noseX}");
                result = false;
            }
            return result;

        }

        private bool CheckCenterPlacedSide(PoseNet.Pose[] poses)
        {
            bool result = true;
            var pose = poses[0];
            var keypoints = pose.keypoints;
            var nose = FindKeypoint(keypoints, "nose");

            // Debug.Log($"nose Position:::{nose.part} {nose.position.x} {nose.position.y}");
            int _Width, _Height;

            //frame.ResizePro(Width, Height, false, false);
            if (_texW > _texH)
            {
                _Width = 257;
                _Height = Mathf.CeilToInt(257f * ((float)_texH / (float)_texW));
            }
            else
            {
                _Width = Mathf.CeilToInt(257f * ((float)_texW / (float)_texH));
                _Height = 257;
            }

            float _noseX = nose.position.x / (float)_Width;
            float _noseY = nose.position.y / (float)_Height;
            if (_noseX > 0.65 || _noseX < 0.35)
            {
                // Debug.Log($"Not CenterPosition:::{nose.part} {_noseX}");
                result = false;
            }
            return result;

        }


        /// <summary>
        /// 部位のパーツが画面内に全て入っているかの確認
        /// </summary>
        /// <param name="poses"></param>
        /// <returns></returns>
        private bool CheckAllPartsInView(PoseNet.Pose[] poses)
        {
            //鼻、両足首の確認
            bool result = true;
            var targets = new ArrayList { "nose", "leftAnkle", "rightAnkle" };
            foreach (var pose in poses)
            {
                // Debug.Log($"Pose score {pose.score}");
                var keypoints = pose.keypoints;
                foreach (var keypoint in keypoints)
                {
                    if (targets.Contains(keypoint.part))
                    {
                        if (keypoint.score < thScore)
                        {
                            //Debug.Log($"Low Score:::{keypoint.part} {keypoint.score}");
                            result = false;
                        }
                    }
                    else
                    {

                    }

                    // Debug.Log($"{keypoint.part} {keypoint.score}");
                }
            }
            return result;
        }

        private bool CheckAllPartsInViewSide(PoseNet.Pose[] poses)
        {
            //鼻、右足を確認
            bool result = true;
            var targets = new ArrayList { "nose", "rightAnkle" };
            foreach (var pose in poses)
            {
                // Debug.Log($"Pose score {pose.score}");
                var keypoints = pose.keypoints;
                foreach (var keypoint in keypoints)
                {
                    if (targets.Contains(keypoint.part))
                    {
                        if (keypoint.score < thScore)
                        {
                            //Debug.Log($"Low Score:::{keypoint.part} {keypoint.score}");
                            result = false;
                        }
                    }
                    else
                    {

                    }

                    // Debug.Log($"{keypoint.part} {keypoint.score}");
                }
            }
            return result;
        }

        // 0 ok
        // 301 far
        // 302 near
        private int CheckAllPartsInInside(PoseNet.Pose[] poses)
        {
            //bool result = true;
            var targets = new ArrayList { "nose", "leftAnkle", "rightAnkle" };

            var pose = poses[0];
            var keypoints = pose.keypoints;
            var nose = FindKeypoint(keypoints, "nose");
            var leftAnkle = FindKeypoint(keypoints, "leftAnkle");
            var rightAnkle = FindKeypoint(keypoints, "rightAnkle");

            int _Width, _Height;

            //frame.ResizePro(Width, Height, false, false);
            if (_texW > _texH)
            {
                _Width = 257;
                _Height = Mathf.CeilToInt(257f * ((float)_texH / (float)_texW));
            }
            else
            {
                _Width = Mathf.CeilToInt(257f * ((float)_texW / (float)_texH));
                _Height = 257;
            }

            float noseY = nose.position.y / (float)_Height;
            var leftAnkleY = leftAnkle.position.y / (float)_Height;
            var rightAnkleY = rightAnkle.position.y / (float)_Height;

            var ratio = _heightRatio / 2f;
            var top = 0.5f - ratio + NOSE_TO_FOOT_RATIO;
            var bottom = 0.5f + ratio - ANKLE_TO_FOOT_RATIO;
            var requireRatio = bottom - top;

            var userBottom = (leftAnkleY + rightAnkleY) / 2f;
            var userTop = noseY;

            var userBodyRatio = userBottom - userTop;

            if (userBodyRatio - requireRatio < -TH_POSITION)
            {
#if !ENV_PRODUCTION
                Debug.Log($"TOO Far:::{userBodyRatio} {requireRatio}");
#endif
                return 301;
            }

            if (userBodyRatio - requireRatio > TH_POSITION)
            {
#if !ENV_PRODUCTION
                Debug.Log($"TOO Near:::{userBodyRatio} {requireRatio}");
#endif
                return 302;
            }

            return 0;
        }

        // 0: is OK
        // 501: 両手手を広げましょう
        // 502: 両手を閉じましょう
        // 503: 左手を広げましょう
        // 504: 左手を閉じましょう
        // 505: 右手を広げましょう
        // 506: 右手を閉じましょう
        // 507: 足を広げましょう
        // 508: 足を閉じましょう
        private int CheckFrontAPose(PoseNet.Pose[] poses)
        {
            var pose = poses[0];
            var keypoints = pose.keypoints;

            var leftShoulder = FindKeypoint(keypoints, "leftShoulder");
            var rightShoulder = FindKeypoint(keypoints, "rightShoulder");
            var leftWrist = FindKeypoint(keypoints, "leftWrist");
            var rightWrist = FindKeypoint(keypoints, "rightWrist");
            var leftElbow = FindKeypoint(keypoints, "leftElbow");
            var rightElbow = FindKeypoint(keypoints, "rightElbow");
            var leftHip = FindKeypoint(keypoints, "leftHip");
            var rightHip = FindKeypoint(keypoints, "rightHip");

            var angleLeft = CalcAngle(GetVector(leftWrist), GetVector(leftShoulder), GetVector(rightShoulder));
            var angleRight = CalcAngle(GetVector(rightWrist), GetVector(rightShoulder), GetVector(leftShoulder));

            var distanceLeft = Math.Abs(leftWrist.position.x - leftHip.position.x);
            var distanceRight = Math.Abs(rightWrist.position.x - rightHip.position.x);


            // Debug.Log($"front a pose::: {leftArmAngle}, {leftElbowAngle} :: {rightArmAngle}, {rightElbowAngle}");

            var leftAnkle = FindKeypoint(keypoints, "leftAnkle");
            var rightAnkle = FindKeypoint(keypoints, "rightAnkle");
            var ankleDistance = Math.Abs(leftAnkle.position.x - rightAnkle.position.x);

            // Debug.Log($"front a pose::: {leftWrist.position.x}, {rightWrist.position.x}");
            //frame.ResizePro(Width, Height, false, false);

            var shoulderDistance = Math.Abs(leftShoulder.position.x - rightShoulder.position.x);

            var angleLeftStatus = 0;

            if (angleLeft < FRONT_ARM_ANGLE)
            {
                angleLeftStatus = -1;
#if !ENV_PRODUCTION
                Debug.Log($"front pose error :::左腕閉じすぎ {angleLeft}");
#endif
            }

            if (angleLeft > FRONT_ARM_ANGLE + 20f)
            {
                angleLeftStatus = 1;
#if !ENV_PRODUCTION
                Debug.Log($"front pose error :::左腕広げすぎ {angleLeft}");
#endif
            }

            if (distanceLeft < MIN_FRONT_DISTANCE)
            {
                angleLeftStatus = -1;
#if !ENV_PRODUCTION
                Debug.Log($"front pose error :::左腰に近い {distanceLeft}");
#endif
            }

            var angleRightStatus = 0;

            if (angleRight < FRONT_ARM_ANGLE)
            {
                angleRightStatus = -1;
#if !ENV_PRODUCTION
                Debug.Log($"front pose error ::: 右腕閉じすぎ {angleRight}");
#endif
            }

            if (angleRight > FRONT_ARM_ANGLE + 20f)
            {
                angleRightStatus = 1;
#if !ENV_PRODUCTION
                Debug.Log($"front pose error ::: 右腕広げすぎ {angleRight}");
#endif
            }

            if (distanceRight < MIN_FRONT_DISTANCE)
            {
                angleRightStatus = -1;
#if !ENV_PRODUCTION
                Debug.Log($"front pose error :::右腰に近い {distanceRight}");
#endif

            }


            // 
            if (angleLeftStatus == -1 && angleRightStatus == -1)
            {
#if !ENV_PRODUCTION
                Debug.Log($"front pose error :::両腕閉じすぎ {angleLeft}");
#endif
                return 501;

            }

            if (angleLeftStatus == 1 && angleRightStatus == 1)
            {
#if !ENV_PRODUCTION
                Debug.Log($"front pose error :::両腕広げすぎ {angleLeft}");
#endif
                return 502;
            }

            if (angleLeft == -1)
            {
                return 503;
            }

            if (angleLeft == 1)
            {
                return 504;
            }

            if (angleRight == -1)
            {
                return 505;
            }

            if (angleRight == 1)
            {
                return 506;
            }

            if (ankleDistance - shoulderDistance > 20f)
            {
#if !ENV_PRODUCTION
                Debug.Log($"front pose error ::: 足広げすぎ {ankleDistance} {shoulderDistance}");
#endif
                return 507;
            }

            if (ankleDistance - shoulderDistance < -20f)
            {
#if !ENV_PRODUCTION
                Debug.Log($"front pose error ::: 足閉じすぎ {ankleDistance} {shoulderDistance}");
#endif
                return 508;
            }

            return 0;
        }

        // 0: is OK
        // 601: 肩が開いている
        // 602: 足が開いている
        // 603: 手が開いてる
        // 604: 向いている方向が逆かもしれません
        // 605: 顔が横向きでない
        private int CheckSidePose(PoseNet.Pose[] poses)
        {
            var pose = poses[0];
            var keypoints = pose.keypoints;

            var leftShoulder = FindKeypoint(keypoints, "leftShoulder");
            var rightShoulder = FindKeypoint(keypoints, "rightShoulder");
            var leftWrist = FindKeypoint(keypoints, "leftWrist");
            var rightWrist = FindKeypoint(keypoints, "rightWrist");
            var nose = FindKeypoint(keypoints, "nose");
            var leftAnkle = FindKeypoint(keypoints, "leftAnkle");
            var rightAnkle = FindKeypoint(keypoints, "rightAnkle");
            var leftEye = FindKeypoint(keypoints, "leftEye");
            var rightEye = FindKeypoint(keypoints, "rightEye");
            var rightElbow = FindKeypoint(keypoints, "rightElbow");

            var distanceShoulder = Math.Abs(leftShoulder.position.x - rightShoulder.position.x);
            var distanceAnkle = Math.Abs(leftAnkle.position.x - rightAnkle.position.x);
//            var distanceWrist = Math.Abs(leftWrist.position.x - rightWrist.position.x);
            var armStraight = Math.Abs(rightElbow.position.x - rightWrist.position.x);
            var distanceEye = Math.Abs(leftEye.position.x - rightEye.position.x);

           if (distanceEye > 5.0f)
            {
                Debug.Log($"side pose::: distanceEye error {distanceEye}");
                return 605;
            }
/*
            if (distanceShoulder > 15.0f)
            {
                Debug.Log($"side pose::: distanceShoulder error {distanceShoulder}");
                return 601;
            }
*/
            if (distanceAnkle > 10.0f)
            {
                Debug.Log($"side pose::: distanceAnkle error {distanceAnkle}");
                return 602;
            }
/*
            if (distanceWrist > 10.0f)
            {
                Debug.Log($"side pose::: distanceWrist error {distanceWrist}");
                return 603;
            }
*/
            if (armStraight > 15.0f)
            {
                Debug.Log($"side pose::: armStraight error {armStraight}");
                return 603;
            }

#if UNITY_IOS
            if (nose.position.x > rightShoulder.position.x)
            {
                return 604;
            }
#elif UNITY_ANDROID && !UNITY_EDITOR
            if (nose.position.x < rightShoulder.position.x && Mathf.Abs(nose.position.x - rightShoulder.position.x) < 15f)
            {
                return 604;
            }
#else
            if (nose.position.x > rightShoulder.position.x && Mathf.Abs(nose.position.x - rightShoulder.position.x) < 15f)
            {
                return 604;
                // Debug.Log($"side pose::: nose position error {nose.position.x} { rightShoulder.position.x}");
            }
#endif
            return 0;
        }


        public float GetAim(Vector2 p1, Vector2 p2)
        {
            float dx = p2.x - p1.x;
            float dy = p2.y - p1.y;
            float rad = Mathf.Atan2(dy, dx);
            return rad * Mathf.Rad2Deg;
        }

        private PoseNet.Keypoint FindKeypoint(PoseNet.Keypoint[] keypoints, string part)
        {
            return Array.Find(keypoints, p => p.part == part);
        }

        private Vector2 GetVector(PoseNet.Keypoint key)
        {
            return new Vector2(key.position.x, key.position.y);
        }

        private float CalcAngle(Vector2 a, Vector2 b, Vector2 c)
        {
            return Vector2.Angle(a - b, c - b);
        }

        public void AnnouceError(int errorCode)
        {
            EasyTTSUtil.StopSpeech();
            switch (errorCode)
            {
                // 撮影条件クリア
/*
                case 0:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("そのポーズのまま静止してください", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Please stand still in that pose.", 1, 0.5f, 1);
                    }
                    break;
*/
                // Pose が見つからない
                case 100:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("ポーズが検出できません。カメラの前に立ってポーズをとってください", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("The pose cannot be detected. Stand in front of the camera and pose accordingly.", 1, 0.5f, 1);
                    }
                    break;
                // all parts not in view: カメラに全身が収まっていません
                case 200:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("全身がおさまるように、撮影位置の調整、カメラの位置を調整しましょう", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Adjust the shooting position and camera position so that the whole body fits inside the viewfinder", 1, 0.5f, 1);
                    }
                    break;
                // all parts not in inside: 3
                // far
                case 301:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("もう少しカメラに近づいてください、ガイドと同じような大きさでうつるように調整しましょう", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Move closer to the camera a little more and adjust your position so your body aligns with the guide.", 1, 0.5f, 1);
                    }
                    break;
                // near
                case 302:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("もう少しカメラから離れてください、ガイドと同じような大きさでうつるように調整しましょう", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Move away from the camera a little further and adjust your position so your body aligns with the guide.", 1, 0.5f, 1);
                    }
                    break;
                // not center: 4
                case 400:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("カメラの中心にくるようにからだの位置を調整してください", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Adjust the position of your body so that your body fits inside the viewfinder", 1, 0.5f, 1);
                    }
                    break;

                // 501: 両腕を広げましょう
                case 501:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("両腕をもう少し広げてください", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Raise your arms a little more", 1, 0.5f, 1);
                    }
                    break;
                // 502: 両腕を閉じましょう
                case 502:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("両腕をもう少し閉じてください", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Please lower your arms a little more", 1, 0.5f, 1);
                    }
                    break;
                // 503: 左腕を広げましょう
                case 503:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("左腕をもう少し広げてください", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Raise your left arm a little more", 1, 0.5f, 1);
                    }
                    break;
                // 504: 左腕を閉じましょう
                case 504:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("左腕をもう少し閉じてください", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Please lower your left arm a little more", 1, 0.5f, 1);
                    }
                    break;
                // 505: 右腕を広げましょう
                case 505:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("右腕をもう少し広げてください", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Raise your right arm a little more", 1, 0.5f, 1);
                    }
                    break;
                // 506: 右腕を閉じましょう
                case 506:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("右腕をもう少し閉じてください", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Please lower your right arm a little more", 1, 0.5f, 1);
                    }
                    break;
                // 507: 足を広げすぎ
                case 507:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("足を肩幅くらいに閉じてください", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Spread your legs about shoulder width apart", 1, 0.5f, 1);
                    }
                    break;
                // 508: 足を閉じすぎ
                case 508:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("足を肩幅くらいに開いてください", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Close your legs about shoulder width apart", 1, 0.5f, 1);
                    }
                    break;

                // 601: 肩が開いている
                case 601:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("左を向いて気をつけのポーズを取ってください", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Shoulder may be facing front. Please look to the left, put your arms at your side, feet together, and stand upright.", 1, 0.5f, 1);
                    }
                    break;
                // 602: 足が開いている
                case 602:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("足を揃えて、左を向いて気をつけのポーズを取ってください", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Feet may not be aligned. Please look to the left, put your arms at your side, feet together, and stand upright.", 1, 0.5f, 1);
                    }
                    break;
                // 603: 手が開いてる
                case 603:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("腕が降りていません。左を向いて気をつけのポーズを取ってください", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Arms appear not down. Please look to the left, put your arms at your side, feet together, and stand upright.", 1, 0.5f, 1);
                    }
                    break;
                // 604: 向いている方向が逆かもしれません
                case 604:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("カメラに対して左向きであることを確認してください", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Make sure you are facing left with respect to the camera", 1, 0.5f, 1);
                    }
                    break;
                // 605: Head not facing side
                case 605:
                    if (DetectLanguage.IsJapanese())
                    {
                        EasyTTSUtil.SpeechAdd("体と同じ方向に、顔も向けてください。", 1, 0.5f, 1);
                    }
                    else
                    {
                        EasyTTSUtil.SpeechAdd("Please move your face to the left.", 1, 0.5f, 1);
                    }
                    break;

            }
        }
    }
}