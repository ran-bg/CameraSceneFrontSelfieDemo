using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PoseNetScripts {
    public class GyroCheckController : MonoBehaviour
    {
        [SerializeField] RectTransform containerRectTransform = null;
        [SerializeField] CanvasGroup canvas = null;
        [SerializeField] Image gyroConditionImage = null;

        public bool isGoodGyroPosition = false;

        private float drawH = 0;
        // private double prev = 0;

        private void Start() {
            Input.gyro.enabled = true;
            drawH = containerRectTransform.rect.height;
        }

        // Update is called once per frame
        void Update()
        {
            var rotRH = Input.gyro.attitude;
            var rot = new Quaternion(-rotRH.x, -rotRH.z, -rotRH.y, rotRH.w) * Quaternion.Euler(90f, 0f, 0f);
            Debug.Log($"{rot.x}, {rot.y}, {rot.z}");
            var zNormalized = Normalize(rot.z, -1, 1, -drawH / 2f, drawH / 2f);
            Debug.Log(zNormalized);
            var prevPosition = gyroConditionImage.rectTransform.localPosition;
            gyroConditionImage.rectTransform.localPosition = new Vector3(prevPosition.x, (float) zNormalized, 0);
        
            if (-0.02 < rot.z && rot.z < 0.02) {
                isGoodGyroPosition = true;
            } else {
                isGoodGyroPosition = false;
            }
        }

        public void ShowCanvas() {
            canvas.alpha = 1.0f;
            canvas.blocksRaycasts = true;
            canvas.interactable = true;
        }

        public void HideCanvas() {
            canvas.alpha = 0;
            canvas.blocksRaycasts = false;
            canvas.interactable = false;
        }

        double Normalize(double val, double valmin, double valmax, double min, double max) 
        {
            return (((val - valmin) / (valmax - valmin)) * (max - min)) + min;
        }
    }
}