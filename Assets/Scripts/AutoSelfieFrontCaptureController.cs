using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AutoSelfie
{

    public class AutoSelfieFrontCaptureController : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvas = default;

        // Start is called before the first frame update
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