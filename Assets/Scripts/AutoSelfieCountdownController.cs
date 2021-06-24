using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using Language;

public class AutoSelfieCountdownController : MonoBehaviour
{
    [SerializeField] CanvasGroup canvas = null;
    [SerializeField] Text countdownText = null;

    void Start()
    {
        HideCanvas();
    }

    // Start is called before the first frame update
    public void StartCountDown(Action callback)
    {
        EasyTTSUtil.StopSpeech();
        if (DetectLanguage.IsJapanese())
        {
            EasyTTSUtil.SpeechAdd("撮影します。", 1, 0.5f, 1);
        }
        else
        {
            EasyTTSUtil.SpeechAdd("Please stand still.", 1, 0.5f, 1);
        }
        // countdownText.text = "1";
        // countdownText.DOCounter(1, 0, 3f)
        countdownText.DOCounter(1, 0, 1f)
            .SetEase(Ease.Linear)
            .SetDelay(0.5f)
            .OnComplete(() =>
            {
                callback();
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
