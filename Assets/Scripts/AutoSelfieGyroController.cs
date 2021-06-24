using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Language;

namespace AutoSelfie
{
    public class AutoSelfieGyroController : MonoBehaviour
    {
        // Start is called before the first frame update[SerializeField] RectTransform containerRectTransform;
        [SerializeField] CanvasGroup canvas = null;

        //中心円の位置
        [SerializeField] Image gyroConditionImage = null;

        [SerializeField] RectTransform containerRectTransform = null;

        [SerializeField] Image isFittingImage = null;

        [SerializeField] private Button gyroBackButton = null;

        [SerializeField] private Button doneAdjustButton = null;

        [SerializeField] private CurrentGyroImageController _currentGyroImageController;


        public bool isGoodGyroPosition = false;

        private float drawH = 0;
        private float drawW = 0;

        //private double prev = 0;

        //DoneButtonを true:押した false:
        private bool doneAdjustFlag = false;
        //
        private bool isHidden = false;

        public Action OnBack;
        public Action<bool,bool> OnUpdateState;

        private void Start()
        {
            Input.gyro.enabled = true;
            drawW = containerRectTransform.rect.width;
            drawH = containerRectTransform.rect.height;

            gyroBackButton.onClick.AddListener(OnClickBackButton);
            doneAdjustButton.onClick.AddListener(OnClickDoneAdjustButton);
        }

        public void Speech()
        {
            EasyTTSUtil.StopSpeech();

            if (DetectLanguage.IsJapanese())
            {
                EasyTTSUtil.SpeechAdd("白い点がターゲットの中心になり、円が緑になるようにカメラの角度を調整しましょう");
            }
            else
            {
                EasyTTSUtil.SpeechAdd("Adjust the device angle so that the white dot is in the center of the target and the circle turns green.");
            }
        }

        // Update is called once per frame
        void Update()
        {
            /*
            var rotRH = Input.gyro.attitude;
            var rot = new Quaternion(-rotRH.x, -rotRH.z, -rotRH.y, rotRH.w) * Quaternion.Euler(90f, 0f, 0f);
#if UNITY_EDITOR
            rot.x = 0.0f;
            rot.z = 0.0f;
#endif

            var zNormalized = Normalize(rot.z, -1, 1, -drawH / 2f, drawH / 2f);
            var xNormalized = Normalize(rot.x, -1, 1, -drawW / 2f, drawW / 2f);


            var prevPosition = gyroConditionImage.rectTransform.localPosition;

            var x = (float)xNormalized*10;
            if( x < -200)
            {
                x = (float)-200;
            }
            else if(x > 200)
            {
                x = (float)200;
            }
            var z = (float)zNormalized * 10;
            if (z < -200)
            {
                z = (float)-200;
            }
            else if (z > 200)
            {
                z = (float)200;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            //Androidの時
            gyroConditionImage.rectTransform.localPosition = new Vector3(x,z, 0);
#else       
            //iPhoneの時
            gyroConditionImage.rectTransform.localPosition = new Vector3(z, x, 0);
#endif


            if (Mathf.Abs(rot.z) < 0.04 && Mathf.Abs(rot.x) < 0.04)
            {
                isGoodGyroPosition = true;
            }
            else
            {
                isGoodGyroPosition = false;
            }
            */

            isGoodGyroPosition = _currentGyroImageController.getGyroCheck();

            //OKボタンをおした後
            if (doneAdjustFlag )
            {
                
                if (!isGoodGyroPosition) {
                    // 水平が保たれなければ
                    HandleRequiredAdjust();
                }
                else
                {   // 水平になったら自動で撮影モードに戻る
                    if (isHidden == false)
                    {
                        OnClickDoneAdjustButton();
                    }
                }
            }

            

            // Debug.Log($"Gyro Condition :::  {xNormalized} {zNormalized} :: {isGoodGyroPosition}");
            UpdateIsFittingMark();
        }

        private void UpdateIsFittingMark()
        {
            if (isGoodGyroPosition)
            {
                //isFittingImage.sprite = ok;
                isFittingImage.color = ColorPalette.GetGyroOk();
                doneAdjustButton.interactable = true;
            }
            else
            {
                //isFittingImage.sprite = ng;
                isFittingImage.color = ColorPalette.GetGyroNg();
                doneAdjustButton.interactable = false;
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

        double Normalize(double val, double valmin, double valmax, double min, double max)
        {
            return (((val - valmin) / (valmax - valmin)) * (max - min)) + min;
        }

        private void OnClickDoneAdjustButton()
        {
            Debug.Log("OnClickDoneAdjustButton");
            doneAdjustFlag = true;
            doneAdjustButton.gameObject.SetActive(false);
            OnUpdateState?.Invoke(true,false);
            isHidden = true;
        }

        /// <summary>
        /// 水平が保たれなかったときの処理
        /// </summary>
        void HandleRequiredAdjust()
        {
            if (doneAdjustFlag)
            {
                return;
            }
            
            //doneAdjustFlag = false;
            OnUpdateState?.Invoke(false,true);
            isHidden = false;
        }

        /// <summary>
        /// Backボタン押下時の処理
        /// </summary>
        void OnClickBackButton()
        {
            Debug.Log("OnClickBackButton");
            HideCanvas();
            OnBack?.Invoke();
        }
    }
}
