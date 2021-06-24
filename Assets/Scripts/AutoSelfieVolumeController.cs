using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Language;
using System;
using DG.Tweening;

namespace AutoSelfie
{
    public class AutoSelfieVolumeController : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvas = null;
        [SerializeField] Button backButton = null;
        [SerializeField] Button completeCheckButton = null;

        public Action OnBack;
        public Action OnComplete;



        // Start is called before the first frame update
        void Start()
        {
            backButton.onClick.AddListener(OnClickBackButton);
            completeCheckButton.onClick.AddListener(OnClickCompleteCheckButton);
            
        }

        void OnClickBackButton()
        {
            OnBack.Invoke();
        }

        void OnClickCompleteCheckButton()
        {
            OnComplete.Invoke();
            
        }

        

        public void StartFrontTutorial1()
        {
            Debug.Log("StartTutorial1");

            EasyTTSUtil.StopSpeech();

            DOVirtual.DelayedCall(0.0f, () =>
            {
                if (DetectLanguage.IsJapanese())
                {
                    EasyTTSUtil.SpeechAdd("採寸中は音声でご案内します", 1, 0.5f, 1);
                }
                else
                {
                    EasyTTSUtil.SpeechAdd("Voice guidance will be activated during measurement", 1, 0.5f, 1);
                }
            });


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