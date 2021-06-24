using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Unity.Barracuda;
using System.Linq;
//using MLAgents;
using PoseNetScripts;

public class InferencePoseNet : MonoBehaviour
{
    public string modelName;

    public RawImage testImage;


    PoseNet posenet = new PoseNet();        //Not a good idea maybe cpu wise? 
    PoseNet.Pose[] poses;

    int drawCount = 0;

    private GLRenderer gl;
    private void Awake()
    {
        BetterStreamingAssets.Initialize();
    }

    // Start is called before the first frame update
    void Start()
    {
        Model model; //= ModelLoader.LoadFromStreamingAssets(modelName + ".bytes");

        if (Application.platform == RuntimePlatform.Android)
        {
            byte[] modelBytes = BetterStreamingAssets.ReadAllBytes(modelName + ".bytes");
            model = ModelLoader.Load(modelBytes);
        }
        else
        {
            model = ModelLoader.LoadFromStreamingAssets(modelName + ".bytes");
        }

        var worker = BarracudaWorkerFactory.CreateWorker(BarracudaWorkerFactory.Type.Compute, model);

#if !ENV_PRODUCTION
        foreach (var layer in model.layers)
        {
            Debug.Log("Layer " + layer.name + " does: " + layer.inputs);
        }
#endif

        var inputs = new Dictionary<string, Tensor>();

        Texture2D img = Resources.Load("sample1") as Texture2D;
        posenet.scale(img, 256, 256, FilterMode.Bilinear);
        //    Texture2D img = testImage.mainTexture as Texture2D;

        var tensor = new Tensor(img, 3);

        inputs.Add("image", tensor);

        //worker.ExecuteAndWaitForCompletion(inputs);
        worker.Execute(inputs);
        worker.CopyOutput();


        // var Heatmap = worker.Fetch("heatmap");
        // var Offset = worker.Fetch("offset_2");
        // var Dis_fwd = worker.Fetch("displacement_fwd_2");
        // var Dis_bwd = worker.Fetch("displacement_bwd_2");
        var Heatmap = worker.CopyOutput("heatmap");
        var Offset = worker.CopyOutput("offset_2");
        var Dis_fwd = worker.CopyOutput("displacement_fwd_2");
        var Dis_bwd = worker.CopyOutput("displacement_bwd_2");

        poses = posenet.DecodeMultiplePosesOG(Heatmap, Offset, Dis_fwd, Dis_bwd,
            outputStride: 16, maxPoseDetections: 2, scoreThreshold: 0.02f, nmsRadius: 20);

        gl = GameObject.Find("GLRenderer").GetComponent<GLRenderer>();
#if !ENV_PRODUCTION
        Debug.Log(gl);
        Debug.Log(poses.Length);
#endif

        Heatmap.Dispose();
        Offset.Dispose();
        Dis_fwd.Dispose();
        Dis_bwd.Dispose();
        worker.Dispose();


    }

    private void OnRenderObject()
    {
        if (poses == null) return;
        if (drawCount == 0)
        {
            PosenetUtils.DebugPosesValues(poses);
            drawCount = 100;
        }
        else
        {
            // drawCount--;
        }

    }
}
