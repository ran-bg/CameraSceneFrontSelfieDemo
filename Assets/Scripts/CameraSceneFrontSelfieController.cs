using Unity.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Unity.Barracuda;
using PoseNetScripts;
using VacuumShaders.TextureExtensions;

using System.IO;
using System;
using System.Diagnostics;
using DG.Tweening;
using Language;

using Debug = UnityEngine.Debug;


public class CameraSceneFrontSelfieController : MonoBehaviour
{
    [SerializeField] Camera _camera = default;

    [SerializeField] RectTransform m_DisplayConainerRectTransform = default;

    [SerializeField] RectTransform m_ContainerRect = null;

    [SerializeField] RawImage m_Display = null;
    [SerializeField] AspectRatioFitter m_DisplayFitter = null;

    [SerializeField] RectTransform m_FrontGuideImageRect = null;
    [SerializeField] RectTransform m_FrontGuideTopRect = null;
    [SerializeField] RectTransform m_FrontGuideBottomRect = null;

    // [SerializeField] RectTransform m_SideGuideImageRect = null;
    //
    // [SerializeField] RectTransform m_SideGuideTopRect = null;
    // [SerializeField] RectTransform m_SideGuideBottomRect = null;
    [SerializeField] CanvasGroup m_FrontCanvasGroup = null;
    //[SerializeField] CanvasGroup m_SideCanvasGroup = null;

    [SerializeField] AutoSelfieCountdownController countdownController = null;

    //[SerializeField] AutoSelfieSuggestMsgController suggestMsgController = null;

    // [SerializeField] private Button debugSucess = default;
    // [SerializeField] private Button debugWarning = default;
    
    private bool frontPositionFlag = false;
    private bool sidePositionFlag = false;

    public string modelName = "model-mobilenet_v1_075";

    private float dpiRatio;
    private WebCamTexture webcamTexture;

    // flags
    private bool camAvailable = false;

    private Queue<AsyncGPUReadbackRequest> _requests = new Queue<AsyncGPUReadbackRequest>();
    private float prevRequestTime = 0f;

    private float prevValidateAnnouceTime = 0f;

    // X秒ごとに1回判定
    public float skipFrameTimeSec = 1.0f;

    // X秒ごとに音声ガイドを流す
    private float annouceTiming = 8.0f;

    private GLRenderer gl;

    private bool requiredPosenet = false;
    private bool isPosing = false;

    static PoseNet posenet;
    PoseNet.Pose[] poses;

    Unity.Barracuda.IWorker worker;

    private PoseAnalysisController _poseAnalysisController;

    public string currentPoseType = "front";
    public Action OnFrontPoseCaptured;
    public Action OnSidePoseCaptured;

    public int poseErrorCode = -1;
    public int prevPoseErrorCode = -1;

    private int okFrames = 0;
    private bool poseUpdated = false;

    //X回判定がOKだった場合
    private int arrowOkFrame = 2;
    private bool countdown = false;

    // HACK: ポーズ認定始まる際、フリーズの件解決手段不明のため、一旦Loading画面を表示させる
    [SerializeField] CanvasGroup m_LoadingCanvasGroup = null;

    //「ポーズ判定を始めます」
    private bool startSpeachFlag = false;

    public void OnClickBackButton()
    {

    }

    void Awake()
    {
        BetterStreamingAssets.Initialize();
        _poseAnalysisController = new PoseAnalysisController();
    }

    // Start is called before the first frame update

    void Start()
    {
        // ステータスバーを表示
        Screen.fullScreen = false;

        // #if !ENV_DEVELOP
        //     debugSucess.gameObject.SetActive(false);
        //     debugWarning.gameObject.SetActive(false);
        // #endif
        

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        var resultSetupWebcam = SetupWebcam();
        if (!resultSetupWebcam)
        {
            DebugLogError("DeviceCamera Error");
            return;
        }

        if (posenet == null)
        {
            posenet = new PoseNet();
        }
        gl = GameObject.FindGameObjectWithTag("GLRenderer").GetComponent<GLRenderer>();
        if (gl == null)
        {
            DebugLogError("gl is notactive");
            return;
        }

        webcamTexture.Play();
        m_Display.texture = webcamTexture;

        camAvailable = true;
        SetupGuide();

        DebugLog("Start");

        // TODO
        // requiredPosenet = false;
    }

    public void SetupGuide()
    {
        float userHeight = 175f;


        var ch = m_ContainerRect.rect.height;
        DebugLog($"Setup Guide[height]:{ch}");
        var ratio = Mathf.Min(userHeight / 220f, 1f);
        var gh = ratio * ch;
        _poseAnalysisController._heightRatio = ratio;
        var rect = m_ContainerRect.rect;
        m_FrontGuideImageRect.sizeDelta  = new Vector2(rect.width, gh * 1.2f);
        m_FrontGuideTopRect.sizeDelta    = new Vector2(rect.width, gh * 0.82f);
        m_FrontGuideBottomRect.sizeDelta = new Vector2(rect.width, gh * 0.13f);
        // m_SideGuideImageRect.sizeDelta   = new Vector2(rect.width, gh * 1.2f);
        // m_SideGuideTopRect.sizeDelta     = new Vector2(rect.width, gh * 0.82f);
        // m_SideGuideBottomRect.sizeDelta  = new Vector2(rect.width, gh * 0.13f);

        DebugLog($"Setup:::{userHeight} {gh}");

    }

    public void UpdateRequiredPosenet(bool state)
    {
        if (state) StartPosenet();
        else StopPosenet();
    }

    private void StartPosenet()
    {
        requiredPosenet = true;
        gl.gameObject.SetActive(true);
        // suggestMsgController.ShowCanvas();
        StartValidate();
    }

    private void StopPosenet()
    {
        requiredPosenet = false;
        // suggestMsgController.HideCanvas();
        poses = new PoseNet.Pose[] { };
    }

    private bool SetupWebcam()
    {
        var result = true;
        // dpiRatio = (float)Screen.width / Screen.dpi;
        dpiRatio = 1f;
        // m_DisplayConainerRectTransform.localScale = new Vector3(1f / dpiRatio, 1f / dpiRatio, 1f);

        WebCamDevice[] devices = WebCamTexture.devices;
        //         int deviceIndex = 0;
#if UNITY_EDITOR
        for (int i = 0; i < devices.Length; i++)
        {
            DebugLog(devices[i].name);
            if (devices[i].isFrontFacing)
            {
                webcamTexture = new WebCamTexture(devices[i].name, 1920, 1080, 60);
            }
        }
#else
        if(devices.Length == 0) {
            return false;
        }
        var deviceIndex = 1;
        webcamTexture = new WebCamTexture(devices[deviceIndex].name, 1920, 1080, 60);
#endif
        return result;
    }

    private void UpdateWebcam()
    {

        float ratio = (float)webcamTexture.width / (float)webcamTexture.height;
        m_DisplayFitter.aspectRatio = ratio;

#if !UNITY_EDITOR
        float wrapRatio = m_ContainerRect.rect.height / m_ContainerRect.rect.width;
        m_DisplayConainerRectTransform.localScale = new Vector3(wrapRatio, wrapRatio, 1f);
#endif

        int orient = -webcamTexture.videoRotationAngle;
        m_Display.rectTransform.localEulerAngles = new Vector3(0, 0, orient);

        GameObject obj = m_Display.gameObject;
        Vector3 scale = obj.transform.localScale;

#if UNITY_IOS

#elif UNITY_ANDROID && !UNITY_EDITOR
        scale.y = -1;
        obj.transform.localScale = scale;
#endif

    }

    // Update is called once per frame
    void Update()
    {
        // DebugLog(requiredPosenet);
        if (!camAvailable)
        {
            return;
        }
        UpdateWebcam();
        UpdateWebcamTex();

        if (countdown) return;
        if (!requiredPosenet)
        {
            if (poses != null && poses.Length > 0) poses = new PoseNet.Pose[] { };
            return;
        }
        // DO POSENET FUNC
        ManagePosenetRequest();
    }

    void StartValidate()
    {
        
        if (startSpeachFlag)
            return;

        if (!startSpeachFlag)
        {
            startSpeachFlag = true;
            DOVirtual.DelayedCall(5, () => { startSpeachFlag = false; });
        }
        

        prevValidateAnnouceTime = Time.realtimeSinceStartup;
        EasyTTSUtil.StopSpeech();


        if (DetectLanguage.IsJapanese())
        {
            EasyTTSUtil.SpeechAdd("ポーズ判定を始めます。", 1, 0.5f, 1);
        }
        else
        {
            EasyTTSUtil.SpeechAdd("Starting pose detection.", 1, 0.5f, 1);
        }

    }

    void ValidateError()
    {
        var time = Time.realtimeSinceStartup - prevValidateAnnouceTime;
        // DebugLog($"Time {time}");
        // if ((time > annouceTiming && requiredPosenet) || prevPoseErrorCode != poseErrorCode)
        if (time > annouceTiming && requiredPosenet)
        {
            AnnouceError();
            prevValidateAnnouceTime = Time.realtimeSinceStartup;
        }
    }

    void AnnouceError()
    {
        DebugLog($"error code: {poseErrorCode}");
        _poseAnalysisController.AnnouceError(poseErrorCode);
    }

    void ManagePosenetRequest()
    {
        while (_requests.Count > 0)
        {
            var req = _requests.Peek();

            if (req.hasError)
            {
                DebugLog("GPU readback error");
                _requests.Dequeue();
            }
            else if (req.done)
            {
                var buffer = req.GetData<Color32>();

                if (Time.frameCount % 1 == 0 && !isPosing)
                {
                    //Create Texture2D()
                    // DebugLog($"{_camera.scaledPixelWidth}, {_camera.scaledPixelHeight}");
                    StartCoroutine(PoseUpdateNoTex(buffer, webcamTexture.width, webcamTexture.height, .001f));
                    isPosing = true;
                    // var result = new AsyncCompletionSource<PoseNet.Pose[]>();
                    // StartCoroutine(PoseAsync(result, buffer, GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight));
                }

                _requests.Dequeue();
            }
            else
            {
                break;
            }
        }
    }

    IEnumerator PoseUpdateNoTex(NativeArray<Color32> buffer, int width, int height, float secondsToWait)
    {
        Model _model;

        // 初回のみ読み込み画面を出す処理
        var isEstimatinoStart = false;
        #if UNITY_IOS && !UNITY_EDITOR
        if (!isEstimatinoStart)
        {
            m_LoadingCanvasGroup.alpha = 1;
            m_LoadingCanvasGroup.interactable = true;
            m_LoadingCanvasGroup.blocksRaycasts = true;
            isEstimatinoStart = true;
        }
        #endif

        yield return new WaitForEndOfFrame();
        if (Application.platform == RuntimePlatform.Android)
        {
            byte[] modelBytes = BetterStreamingAssets.ReadAllBytes(modelName + ".bytes");
            _model = ModelLoader.Load(modelBytes);
        }
        else
        {
            _model = ModelLoader.LoadFromStreamingAssets(modelName + ".bytes");
        }

        var _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, _model);
        //var _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, _model);

#if UNITY_EDITOR
        var frame = new Texture2D(width, height, TextureFormat.RGB24, false);
        frame.SetPixels32(buffer.ToArray());
        frame.Apply();
        // byte[] bytes = frame.EncodeToPNG();
        // var dirPath = Application.dataPath + "/../SaveImages/";
        // if (!Directory.Exists(dirPath))
        // {
        //     Directory.CreateDirectory(dirPath);
        // }
        // File.WriteAllBytes(dirPath + "Image" + ".png", bytes);
#else
                var bLot = false;
                if (webcamTexture.videoRotationAngle == 90) {
                    bLot = true;
                }
                var _colors = GetRotatedColors(buffer.ToArray(), width, height, bLot);
                var frame = new Texture2D(height, width, TextureFormat.RGB24, false);
                frame.SetPixels32(_colors);
                frame.Apply();
                var t = height;
                height = width;
                width = t;
#endif

        yield return new WaitForEndOfFrame();

        int _Width, _Height;
        if (width > height)
        {
            _Width = 257;
            _Height = Mathf.CeilToInt(257f * ((float)height / (float)width));
        }
        else
        {
            _Width = Mathf.CeilToInt(257f * ((float)width / (float)height));
            _Height = 257;
        }

        frame.ResizePro(_Width, _Height);


        var inputs = new Dictionary<string, Tensor>();

        var tensor = new Tensor(frame, 3);
        inputs.Add("image", tensor);

        //_worker.ExecuteAndWaitForCompletion(inputs);

        _worker.Execute(inputs);
        _worker.CopyOutput();

        #if UNITY_IOS && !UNITY_EDITOR
        if (isEstimatinoStart)
        {
            m_LoadingCanvasGroup.alpha = 0;
            m_LoadingCanvasGroup.interactable = false;
            m_LoadingCanvasGroup.blocksRaycasts = false;
        }
        #endif
        //yield return new WaitForSeconds(secondsToWait);
        yield return new WaitForEndOfFrame();
        
        //var Heatmap = _worker.Fetch("heatmap");
        var Heatmap = _worker.CopyOutput("heatmap");

        //yield return new WaitForSeconds(secondsToWait);
        yield return new WaitForEndOfFrame();

        //var Offset = _worker.Fetch("offset_2");
        var Offset = _worker.CopyOutput("offset_2");

        //yield return new WaitForSeconds(secondsToWait);
        yield return new WaitForEndOfFrame();

        //var Dis_fwd = _worker.Fetch("displacement_fwd_2");
        var Dis_fwd = _worker.CopyOutput("displacement_fwd_2");

        //yield return new WaitForSeconds(secondsToWait);
        yield return new WaitForEndOfFrame();

        //var Dis_bwd = _worker.Fetch("displacement_bwd_2");
        var Dis_bwd = _worker.CopyOutput("displacement_bwd_2");

        //yield return new WaitForSeconds(secondsToWait);
        yield return new WaitForEndOfFrame();

        poses = posenet.DecodeMultiplePosesOG(Heatmap, Offset, Dis_fwd, Dis_bwd,
            outputStride: 16, maxPoseDetections: 1, scoreThreshold: 0.5f, nmsRadius: 20);


        Offset.Dispose();
        Dis_fwd.Dispose();
        Dis_bwd.Dispose();
        Heatmap.Dispose();
        _worker.Dispose();
        tensor.Dispose();

        poseUpdated = true;
        isPosing = false;

        frame = null;
        inputs = null;

        //    Resources.UnloadUnusedAssets();
        yield return null;
    }

    void OnRenderObject()
    {
        if (countdown) return;
        if (poses != null)
        {
            var rectTransform = m_Display.gameObject.GetComponent<RectTransform>();
            float wrapRatio = m_ContainerRect.rect.height / m_ContainerRect.rect.width;

            // DebugLog($"Update Obj :::{dpiRatio} {rectTransform.rect.width} {rectTransform.rect.height}");
#if UNITY_EDITOR
            GLRenderer._texH = webcamTexture.height;
            GLRenderer._texW = webcamTexture.width;
            GLRenderer._rawImageWidth = rectTransform.rect.width / dpiRatio;
            GLRenderer._rawImageHeight = rectTransform.rect.height / dpiRatio;
            _poseAnalysisController._texH = webcamTexture.height;
            _poseAnalysisController._texW = webcamTexture.width;
#else
            GLRenderer._texH = webcamTexture.width;
            GLRenderer._texW = webcamTexture.height;
            GLRenderer._rawImageWidth = rectTransform.rect.height / dpiRatio * wrapRatio;
            GLRenderer._rawImageHeight = rectTransform.rect.width / dpiRatio * wrapRatio;
            _poseAnalysisController._texH = webcamTexture.width;
            _poseAnalysisController._texW = webcamTexture.height;
#endif
            // gl.DrawResults(poses, poseErrorCode);
        }

        if (poses != null && poseUpdated)
        {
            // suggestMsgController.ShowMsgByErrorCase(poseErrorCode);
            if (currentPoseType == "front")
            {
                CheckFront(poses);
            }
            else if (currentPoseType == "side")
            {
                CheckSide(poses);
            }
            ValidateError();
            poseUpdated = false;
        }

    }

    /// <summary>
    /// ポーズネットの判定を行う
    /// </summary>
    /// <param name="poses"></param>
    void CheckFront(PoseNet.Pose[] poses)
    {
        var result = _poseAnalysisController.ValidateFrontPose(poses);
        prevPoseErrorCode = poseErrorCode;
        poseErrorCode = result;

        DebugLog("[Front]:ポーズ判定中[" + poseErrorCode + "]:["+ okFrames);
        if (result != 0)
        {
            okFrames = 0;
            if (result >= 400 && !frontPositionFlag)
            {
                frontPositionFlag = true;
                //m_FrontCanvasGroup.DOFade(0.1f, 0.1f);
            }
            else if (result < 400 && frontPositionFlag)
            {
                frontPositionFlag = false;
                //m_FrontCanvasGroup.DOFade(1f, 0.1f);
            }

            //カメラの背景色をOn /Offに
            if (result >= 400)
            {
                _camera.backgroundColor = ColorPalette.GetGyroOk();
            }
            else
            {
                _camera.backgroundColor = ColorPalette.GetGyroNg();
            }

            okFrames = 0;
            return;
        }

        _camera.backgroundColor = new Color(0, 0, 0);

        okFrames++;
        if (okFrames < arrowOkFrame) return;

        DebugLog("[Front]:撮影を開始します"+ okFrames);

        // suggestMsgController.HideCanvas();
        countdown = true;
        countdownController.StartCountDown(() =>
        {
            OnFrontPoseCaptured.Invoke();
            UpdateRequiredPosenet(false);
            okFrames = 0;
            countdown = false;
        });

    }

    void CheckSide(PoseNet.Pose[] poses)
    {
        var result = _poseAnalysisController.ValidateSidePose(poses);
        prevPoseErrorCode = poseErrorCode;
        poseErrorCode = result;
        if (result != 0)
        {
            okFrames = 0;
            if (result >= 400 && !sidePositionFlag)
            {
                sidePositionFlag = true;
                //m_SideCanvasGroup.DOFade(0.1f, 0.1f);
            }
            else if (result < 400 && sidePositionFlag)
            {
                sidePositionFlag = false;
                //m_SideCanvasGroup.DOFade(1f, 0.1f);
            }

            if (result >= 400)
            {
                _camera.backgroundColor = ColorPalette.GetGyroOk();
            }
            else
            {
                _camera.backgroundColor = ColorPalette.GetGyroNg();
            }


            okFrames = 0;
            return;
        }
        
        _camera.backgroundColor = new Color(0, 0, 0);

        okFrames++;
        if (okFrames < arrowOkFrame) return;

        DebugLog("[Side]:撮影を開始します" + okFrames);

        // suggestMsgController.HideCanvas();
        countdown = true;
        countdownController.StartCountDown(() =>
        {
            OnSidePoseCaptured.Invoke();
            UpdateRequiredPosenet(false);
            okFrames = 0;
            countdown = false;

        });
    }

    private Texture2D CreateTexture()
    {
        DebugLog("CreateTexture_s");
        var color32 = webcamTexture.GetPixels32();
        // ここで取得できている画像サイズが 1920*1080
        Texture2D texture = new Texture2D(webcamTexture.width, webcamTexture.height);

        texture.SetPixels32(color32);
        texture.Apply();

        Texture2D rotated;

#if UNITY_EDITOR
        rotated = texture;
#else
        bool bLot = false;
        if (webcamTexture.videoRotationAngle == 90)
        {
            bLot = true;
        }
        else
        {
            bLot = false;
        }

        rotated = RotateTexture(texture, bLot);
#endif
        texture = null;

        return rotated;
    }

    void CheckPose(PoseNet.Pose[] poses)
    {
        if (currentPoseType == "front")
        {
            _poseAnalysisController.ValidateFrontPose(poses);
        }
        else
        {
            _poseAnalysisController.ValidateSidePose(poses);
        }
    }

    void UpdateWebcamTex()
    {
        // DebugLog("update");
        float time = Time.realtimeSinceStartup - prevRequestTime;
        var rt = RenderTexture.GetTemporary(webcamTexture.width, webcamTexture.height, 0, RenderTextureFormat.ARGB32);
        if (requiredPosenet)
        {
            if (_requests.Count < 8)
            {
                if (time >= skipFrameTimeSec)
                {
                    _requests.Enqueue(AsyncGPUReadback.Request(rt));
                    prevRequestTime = Time.realtimeSinceStartup;
                }
            }
            else
            {
                DebugLog("Too many requests take it easy boy");
            }
        }

        //prevRequestTime = Time.realtimeSinceStartup;
        Graphics.Blit(webcamTexture, rt);
        RenderTexture.ReleaseTemporary(rt);

    }

    /// <summary>
    /// iOSにて利用中
    /// </summary>
    /// <param name="original"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="clockwise"></param>
    /// <returns></returns>
    Color32[] GetRotatedColors(Color32[] original, int w, int h, bool clockwise)
    {
        Color32[] rotated = new Color32[original.Length];

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }
        return rotated;
    }

    /// <summary>
    /// iOSにて利用中
    /// </summary>
    /// <param name="originalTexture"></param>
    /// <param name="clockwise"></param>
    /// <returns></returns>
    Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
    {
        Color32[] original = originalTexture.GetPixels32();
        Color32[] rotated = new Color32[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels32(rotated);
        rotatedTexture.Apply();
        return rotatedTexture;
    }

    public void OnClickDebugShutter()
    {
        CreateMock();
    }
    
    public void OnClick303DebugShutter()
    {
        Create303Mock();
    }
    
    //成功する画像を送る
    private void CreateMock()
    {
        DebugLog("CreateMock");
    }
    
    private void Create303Mock()
    {
        DebugLog("Create303Mock");
    }
    
    [Conditional("ENV_DEVELOP")]
    private void DebugLog(string message)
    {
        Debug.Log($"{message}");
    }
    
    [Conditional("ENV_DEVELOP")]
    private void DebugLogError(string message)
    {
        Debug.LogError($"{message}");
    }
    
}
