using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhoneCamera : MonoBehaviour
{
    private bool camAbailable;
    private WebCamTexture webcamTexture;

    private Texture defaultBackground;
    public RawImage m_Display;
    public AspectRatioFitter m_DisplayFitter;
    /// float dpiRatio;

    bool camAvailable;
    // Start is called before the first frame update

    // void Start()
    // {
    //     var resultSetupWebcam = SetupWebcam();
    //     if (!resultSetupWebcam)
    //     {
    //         Debug.LogError("DeviceCamera Error");
    //         return;
    //     }

    //     webcamTexture.Play();
    //     camAvailable = true;

    //     Debug.Log("Start");

    //     // TODO
    //     // requiredPosenet = false;
    // }

    private bool SetupWebcam()
    {
        var result = true;
        // dpiRatio = (float)Screen.width / Screen.dpi;
        /// dpiRatio = 1f;
        // m_DisplayConainerRectTransform.localScale = new Vector3(1f / dpiRatio, 1f / dpiRatio, 1f);

        WebCamDevice[] devices = WebCamTexture.devices;
        int deviceIndex = 0;
#if UNITY_EDITOR
        deviceIndex = 0;
#else
            if(devices.Length == 0) {
                return false;
            }
            deviceIndex = 1;
#endif
#if !ENV_PRODUCTION
        Debug.Log(deviceIndex);
#endif
        webcamTexture = new WebCamTexture(devices[deviceIndex].name, 1080, 1920, 60);
        // webcamTexture = new WebCamTexture(devices[deviceIndex].name, 1920, 1080, 60);
        m_Display.texture = webcamTexture;
        return result;
    }

    private void UpdateWebcam()
    {
        float ratio = (float)webcamTexture.width / (float)webcamTexture.height;
        m_DisplayFitter.aspectRatio = ratio;

        float scaleY = webcamTexture.videoVerticallyMirrored ? -1f : 1f;
        m_Display.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -webcamTexture.videoRotationAngle;
        m_Display.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
    }

    // Update is called once per frame
    // void Update()
    // {
    //     // Debug.Log(requiredPosenet);
    //     if (!camAvailable)
    //     {
    //         return;
    //     }
    //     UpdateWebcam();
    //     // UpdateWebcamTex();
    // }

    void Start()
    {
        defaultBackground = m_Display.texture;
        WebCamDevice[] devices = WebCamTexture.devices;

        // if (devices.Length == 0)
        // {
        //     Debug.Log("No Camera detected");
        //     camAbailable = false;
        //     return;
        // }

        // for (int i = 0; i < devices.Length; i++)
        // {
        //     if (devices[i].isFrontFacing)
        //     {
        //         webcamTexture = new WebCamTexture(devices[i].name, Screen.width, Screen.height, 60);
        //     }
        // }

        // if (webcamTexture == null)
        // {
        //     Debug.Log("Unable to find back camera");
        //     return;
        // }

        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing)
            {
                webcamTexture = new WebCamTexture(devices[i].name, Screen.width, Screen.height, 60);
            }
        }
        // Debug.Log(deviceIndex);
        // webcamTexture = new WebCamTexture(devices[deviceIndex].name, 1080, 1920, 60);


        webcamTexture.Play();
        m_Display.texture = webcamTexture;
        camAbailable = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!camAbailable) return;

        float ratio = (float)webcamTexture.width / (float)webcamTexture.height;
        m_DisplayFitter.aspectRatio = ratio;



        float scaleY = webcamTexture.videoVerticallyMirrored ? -1f : 1f;
        m_Display.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -webcamTexture.videoRotationAngle;
        m_Display.rectTransform.localEulerAngles = new Vector3(0, 0, orient);

    }
}
