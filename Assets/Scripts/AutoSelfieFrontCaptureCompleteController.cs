using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Language;

namespace AutoSelfie
{


    public class AutoSelfieFrontCaptureCompleteController : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvas = null;
        private float completeSec = 5f;
        public Action OnComplete;

        void ShutterSound()
        {
#if UNITY_IOS
            UniIosAudioService.PlaySystemSound(1108);
#elif UNITY_ANDROID
            var mediaActionSound = new AndroidJavaObject("android.media.MediaActionSound");
            mediaActionSound.Call("play", mediaActionSound.GetStatic<int>("SHUTTER_CLICK"));
#endif
        }

        public void StartFrow()
        {
            DOVirtual.DelayedCall(0f, ShutterSound);
            DOVirtual.DelayedCall(1.5f, StartSpeech);
            DOVirtual.DelayedCall(completeSec, OnEndFrow);
        }

        void StartSpeech()
        {
            EasyTTSUtil.StopSpeech();
            if (DetectLanguage.IsJapanese())
            {
                EasyTTSUtil.SpeechAdd("Great。次のポーズに移ります", 1, 0.5f, 1);
            }
            else
            {
                EasyTTSUtil.SpeechAdd("Awesome. Go to next pose.", 1, 0.5f, 1);
            }

        }

        public void OnEndFrow()
        {
            OnComplete.Invoke();
        }

        public void ShowCanvas()
        {
            canvas.alpha = 1.0f;
            canvas.blocksRaycasts = true;
            canvas.interactable = true;
        }

        public void HideCanvas()
        {
            canvas.alpha = 0;
            canvas.blocksRaycasts = false;
            canvas.interactable = false;
        }
    }
}