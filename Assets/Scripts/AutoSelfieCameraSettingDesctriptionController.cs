using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Language;
using System;
using DG.Tweening;

public class AutoSelfieCameraSettingDesctriptionController : MonoBehaviour
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

    

    public void StartSpeach()
    {
        Debug.Log("StartTutorial1");

        EasyTTSUtil.StopSpeech();

        DOVirtual.DelayedCall(0.0f, () =>
        {
            if (DetectLanguage.IsJapanese())
            {
                EasyTTSUtil.SpeechAdd("カメラをテーブル上で水平、垂直に固定しましょう", 1, 0.5f, 1);
            }
            else
            {
                EasyTTSUtil.SpeechAdd("Put your device vertically on the table.", 1, 0.5f, 1);
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
