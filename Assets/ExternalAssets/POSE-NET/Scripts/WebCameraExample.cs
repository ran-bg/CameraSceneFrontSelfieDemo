using System.IO;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.UI;
using VacuumShaders.TextureExtensions;

#if UNITY_2018_3_OR_NEWER
using UnityEngine.Rendering;
#else
using UnityEngine.Experimental.Rendering;
#endif

using PoseNetScripts;

//using TensorFlow;

public class WebCameraExample : MonoBehaviour
{

    public float skipFrameTimeSec = 0.25f;
    private float prevTime = 0f;

    public string modelName;

    public int Width = 256;
    public int Height = 256;

    public NNModel modelSource;

    public int FPS = 30;

    public Text fpsText;
    public Text debugText;
    WebCamTexture webcamTexture;
    public GLRenderer gl;

    public RawImage m_Display;
    public AspectRatioFitter fitter;

    static PoseNet posenet;
    PoseNet.Pose[] poses;
    bool isPosing;

    Model model;
    Unity.Barracuda.IWorker worker;
    Camera _camera;

    Queue<AsyncGPUReadbackRequest> _requests = new Queue<AsyncGPUReadbackRequest>();

    DebugTextController _debugTextController;

    PoseAnalysisController _poseAnalysisController;

    [SerializeField] Image isFindPose = default;
    [SerializeField] Image isFindMultiPose = default;

    [SerializeField] Image isCenterPlaced = default;

    [SerializeField] Image isAllPartsInView = default;

    [SerializeField] Image isFrontPoseOk = default;

    [SerializeField] Image isSidePoseOk = default;


    [SerializeField] RectTransform wrapRect = null;

    private bool camAvailable = false;

    private RenderTexture webcamRenderTexture;

    private float dpiRatio;



    // Use this for initialization
    private void Start()
    {
        dpiRatio = (float)Screen.width / Screen.dpi;
        wrapRect.localScale = new Vector3(1f / dpiRatio, 1f / dpiRatio, 1f);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        _poseAnalysisController = new PoseAnalysisController();
        // カメラ利用をユーザーに確認
        // yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        // // 許可された確認
        // if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        // {
        //     yield break;
        // }

        _camera = GetComponent<Camera>();


        WebCamDevice[] devices = WebCamTexture.devices;
        int deviceIndex = 0;
        if (devices.Length > 1)
        {
            deviceIndex = 1;
        }
        else
        {
            deviceIndex = 0;
        }
        webcamTexture = new WebCamTexture(devices[deviceIndex].name, Screen.width / 2, Screen.height / 2, FPS);

        // webcamTexture = new WebCamTexture(devices[deviceIndex].name, 1920, 1080, 60);
        m_Display.texture = webcamTexture;

        if (posenet == null)
        {

            posenet = new PoseNet();
            debugText.text = "Model Loaded";

        }

        gl = GameObject.FindGameObjectWithTag("GLRenderer").GetComponent<GLRenderer>();

        //    StartCoroutine(PoseUpdateFromStart());

#if !ENV_PRODUCTION
        Debug.Log("Made it to the end of start");
#endif

        //    worker.Dispose();
        webcamTexture.Play();
        camAvailable = true;

    }

    void Awake()
    {
        BetterStreamingAssets.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (!camAvailable)
        {
            return;
        }

        float ratio = (float)webcamTexture.width / (float)webcamTexture.height;
        fitter.aspectRatio = ratio;



        float scaleY = webcamTexture.videoVerticallyMirrored ? -1f : 1f;
        m_Display.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -webcamTexture.videoRotationAngle;
        m_Display.rectTransform.localEulerAngles = new Vector3(0, 0, orient);



        var fps = 1.0f / Time.smoothDeltaTime;

        fpsText.text = Mathf.RoundToInt(fps) + " FPS";

        UpdateWebcamTex();

        while (_requests.Count > 0)
        {
            var req = _requests.Peek();

            if (req.hasError)
            {
#if !ENV_PRODUCTION
                Debug.Log("GPU readback error");
#endif
                _requests.Dequeue();
            }
            else if (req.done)
            {
                var buffer = req.GetData<Color32>();

                if (Time.frameCount % 1 == 0 && !isPosing)
                {
                    //Create Texture2D()
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

    void UpdateWebcamTex()
    {
        float time = Time.realtimeSinceStartup - prevTime;
        var rt = RenderTexture.GetTemporary(webcamTexture.width, webcamTexture.height, 0, RenderTextureFormat.ARGB32);
        if (_requests.Count < 8)
        {
            if (time < skipFrameTimeSec)
            {
                _requests.Enqueue(AsyncGPUReadback.Request(rt));
            }
        }
        else
        {
#if !ENV_PRODUCTION
            Debug.Log("Too many requests take it easy boy");
#endif
        }

        prevTime = Time.realtimeSinceStartup;
        Graphics.Blit(webcamTexture, rt);
        RenderTexture.ReleaseTemporary(rt);

    }


    // void OnRenderImage(RenderTexture src, RenderTexture dest)
    // {

    //     float time = Time.realtimeSinceStartup - prevTime;

    //     if (_requests.Count < 8)
    //     {
    //         if (time < skipFrameTimeSec)
    //         {
    //             _requests.Enqueue(AsyncGPUReadback.Request(src));
    //         }
    //     }
    //     else
    //     {
    //         Debug.Log("Too many requests take it easy boy");
    //     }

    //     prevTime = Time.realtimeSinceStartup;
    //     Graphics.Blit(src, dest);

    // }

    //On Render()
    void OnRenderObject()
    {
        if (poses != null)
        {
            var rectTransform = m_Display.gameObject.GetComponent<RectTransform>();
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
            GLRenderer._rawImageWidth = rectTransform.rect.height / dpiRatio;
            GLRenderer._rawImageHeight = rectTransform.rect.width / dpiRatio;
            _poseAnalysisController._texH = webcamTexture.width;
            _poseAnalysisController._texW = webcamTexture.height;
#endif

            gl.DrawResults(poses, 0);
            _poseAnalysisController.UpdateState(poses);
            // UpdateDebugs();
        }
    }

    void UpdateDebugs()
    {
        var okColor = new Color(0f, 1f, 0f);
        var ngColor = new Color(1f, 0f, 0f);
        isFindPose.color = _poseAnalysisController.isFindPose ? okColor : ngColor;
        isFindMultiPose.color = _poseAnalysisController.isAllPartsInInside ? okColor : ngColor;
        isCenterPlaced.color = _poseAnalysisController.isCenterPlaced ? okColor : ngColor;
        isAllPartsInView.color = _poseAnalysisController.isAllPartsInView ? okColor : ngColor;
        isFrontPoseOk.color = _poseAnalysisController.isFrontPoseOk ? okColor : ngColor;
        isSidePoseOk.color = _poseAnalysisController.isSidePoseOk ? okColor : ngColor;
    }

    IEnumerator PoseUpdateNoTex(NativeArray<Color32> buffer, int width, int height, float secondsToWait)
    {
        //    isPosing = true;
        Model _model;
        if (Application.platform == RuntimePlatform.Android)
        {
            byte[] modelBytes = BetterStreamingAssets.ReadAllBytes(modelName + ".bytes");
            _model = ModelLoader.Load(modelBytes);
        }
        else
        {
            _model = ModelLoader.LoadFromStreamingAssets(modelName + ".bytes");
        }


        //var _model = model;

        var _worker = BarracudaWorkerFactory.CreateWorker(BarracudaWorkerFactory.Type.CSharpBurst, _model);
        //var _worker = worker;
#if UNITY_EDITOR
        var frame = new Texture2D(width, height, TextureFormat.RGB24, false);
        frame.SetPixels32(buffer.ToArray());
        frame.Apply();
        // byte[] bytes = frame.EncodeToPNG();
        // var dirPath = Application.dataPath + "/../SaveImages/";
        // if(!Directory.Exists(dirPath)) {
        //     Directory.CreateDirectory(dirPath);
        // }
        // File.WriteAllBytes(dirPath + "Image" + ".png", bytes);
#else
        var _colors = GetRotatedColors(buffer.ToArray(), width, height, false);
        var frame = new Texture2D(height, width, TextureFormat.RGB24, false);
        frame.SetPixels32(_colors);
        frame.Apply();
        var t = height;
        height = width;
        width = t;
#endif


        // var png = frame.EncodeToPNG();
        // string tempPath = Path.Combine(Application.persistentDataPath, "tex.png");
        // // var dirPath = Application.dataPath + "/../SaveImages/";
        // // if (!Directory.Exists(dirPath))
        // // {
        // //     Directory.CreateDirectory(dirPath);
        // // }
        // File.WriteAllBytes(tempPath, png);

        //yield return new WaitForSeconds(secondsToWait);

        yield return new WaitForEndOfFrame();

        int _Width, _Height;
        //frame.ResizePro(Width, Height, false, false);

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



        // Debug.Log($"PoseNet exec {_Width}, {_Height} ::: {width} {height} ::: {frame.width} {frame.height}");

#if UNITY_EDITOR
        // var p = Resize(frame, _Width, _Height);
        // frame = null;
        // frame = p;
        // p = null;
        frame.ResizePro(_Width, _Height);
        // posenet.scale(frame, _Width, _Height, FilterMode.Bilinear);
#else
            posenet.scale(frame, _Width, _Height, FilterMode.Bilinear);
#endif
        // #if UNITY_EDITOR
        //     byte[] _bytes = frame.EncodeToPNG();
        //     var _dirPath = Application.dataPath + "/../SaveImages/";
        //     if(!Directory.Exists(_dirPath)) {
        //         Directory.CreateDirectory(_dirPath);
        //     }
        //     File.WriteAllBytes(_dirPath + "Image-scaled" + ".png", _bytes);
        // #endif
        // Save frame image jpg to disk for debugging
        /// var randomInt = UnityEngine.Random.Range(0, 100000000000000000);
        /// File.WriteAllBytes(Application.persistentDataPath + "/pose-" + randomInt + ".jpg", frame.EncodeToJPG());
        /// Debug.Log("Saved size converted image path: " + Application.persistentDataPath + "/pose-" + randomInt + ".jpg");

        var inputs = new Dictionary<string, Tensor>();

        var tensor = new Tensor(frame, 3);
        inputs.Add("image", tensor);

        _worker.ExecuteAndWaitForCompletion(inputs);

        //yield return new WaitForSeconds(secondsToWait);
        yield return new WaitForEndOfFrame();

        //var Heatmap = _worker.Fetch("heatmap");
        var Heatmap = _worker.CopyOutput("heatmap");

        //yield return new WaitForSeconds(secondsToWait);
        yield return new WaitForEndOfFrame();

        //        var Offset = _worker.Fetch("offset_2");
        var Offset = _worker.CopyOutput("offset_2");

        //yield return new WaitForSeconds(secondsToWait);
        yield return new WaitForEndOfFrame();

        //var Dis_fwd = _worker.Fetch("displacement_fwd_2");
        var Dis_fwd = _worker.CopyOutput("displacement_fwd_2");

        //yield return new WaitForSeconds(secondsToWait);
        yield return new WaitForEndOfFrame();

        //        var Dis_bwd = _worker.Fetch("displacement_bwd_2");
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


        isPosing = false;

        frame = null;
        inputs = null;

        //    Resources.UnloadUnusedAssets(); 


        yield return null;
    }

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

    Texture2D rotateTexture(Texture2D originalTexture, bool clockwise)
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

    Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        return result;
    }

}
