using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using Language;

using DG.Tweening;


namespace AutoSelfie
{


    public class AutoSelfieFrontCaptureExplainController : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvas = null;

        //[SerializeField] Image text1 = null;
        //[SerializeField] Image text2 = null;

        private float completeSec = 10f;
        //private float completeSec = 1f; //debug
        private float changeSec;

        public Action OnComplete;

        // Start is called before the first frame update
        void Start()
        {
            // StartSpeech();
            changeSec = completeSec / 2f;
            Init();
        }

        void Init()
        {
            /*
            text1.gameObject.SetActive(true);
            text2.gameObject.SetActive(false);
            */
        }

        public void StartFlow()
        {
            StartSpeech();
            DOVirtual.DelayedCall(changeSec, ShowText2);
            DOVirtual.DelayedCall(completeSec, CompleteExplain);
        }

        void ShowText2()
        {
            /*
            text1.gameObject.SetActive(false);
            text2.gameObject.SetActive(true);
            */
        }

        void CompleteExplain()
        {
            OnComplete.Invoke();
        }

        void StartSpeech()
        {
#if UNITY_ANDROID
            if (DetectLanguage.IsJapanese())
            {
                EasyTTSUtil.Initialize(EasyTTSUtil.Japan);
            }
            else
            {
                EasyTTSUtil.Initialize(EasyTTSUtil.UnitedStates);
            }
#endif
            EasyTTSUtil.StopSpeech();
            if (DetectLanguage.IsJapanese())
            {
                EasyTTSUtil.SpeechAdd("フロント画像の撮影を開始します。カメラの中央で同じポーズを撮ってください。自動で撮影が行われます", 1, 0.5f, 1);
            }
            else
            {
                EasyTTSUtil.SpeechAdd("Start taking the front image. Take the same pose in the center of the camera. Shooting is done automatically.", 1, 0.5f, 1);
            }

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