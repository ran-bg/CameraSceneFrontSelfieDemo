using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSelfieSuggestMsgController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] CanvasGroup canvas = null;

    [SerializeField] CanvasGroup noDetection = null;
    [SerializeField] CanvasGroup near = null;
    [SerializeField] CanvasGroup far = null;
    [SerializeField] CanvasGroup center = null;
    [SerializeField] CanvasGroup frontPose = null;
    [SerializeField] CanvasGroup frontFoot = null;
    [SerializeField] CanvasGroup frontArmClose = null;
    [SerializeField] CanvasGroup frontArmRaise = null;

    [SerializeField] CanvasGroup sidePose = null;

    [SerializeField] CanvasGroup keepPose = null;
    void HideMsgs()
    {
        noDetection.alpha = 0;
        near.alpha = 0;
        far.alpha = 0;
        center.alpha = 0;
        frontPose.alpha = 0;
        frontFoot.alpha = 0;
        frontArmClose.alpha = 0;
        frontArmRaise.alpha = 0;
        sidePose.alpha = 0;
        keepPose.alpha = 0;
    }

    public void ShowMsgByErrorCase(int errorCode)
    {
        HideMsgs();
        if (errorCode == 0)
        {
            keepPose.alpha = 0;
        }

        if (errorCode >= 100 && errorCode < 300)
        {
            noDetection.alpha = 1f;
        }
        // far
        if (errorCode == 301)
        {
            far.alpha = 1;
        }
        // near
        if (errorCode == 302)
        {
            near.alpha = 1;
        }
        if (errorCode == 400)
        {
            center.alpha = 1;
        }

        if (errorCode >= 500 && errorCode < 600)
        {
            if (errorCode == 501 || errorCode == 503 || errorCode == 505)
            {
                frontArmRaise.alpha = 1;
            }

            if (errorCode == 502 || errorCode == 504 || errorCode == 506)
            {
                frontArmClose.alpha = 1;
            }

            if (errorCode == 507 || errorCode == 508)
            {
                frontFoot.alpha = 1;
            }
        }

        if (errorCode >= 600)
        {
            sidePose.alpha = 1;
        }
    }


    void Start()
    {
        HideCanvas();
    }

    public void ShowCanvas()
    {
        //canvas.alpha = 1.0f;
        //canvas.blocksRaycasts = true;
        //canvas.interactable = true;

        //とりあえず消したままにする
        canvas.alpha = 0;
        canvas.blocksRaycasts = false;
        canvas.interactable = false;
    }

    public void HideCanvas()
    {
        canvas.alpha = 0;
        canvas.blocksRaycasts = false;
        canvas.interactable = false;
    }
}
