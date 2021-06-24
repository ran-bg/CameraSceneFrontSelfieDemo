using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PoseNetScripts
{
    public class GLRenderer : MonoBehaviour
    {
        static Material lineMaterial;
        PoseNet posenet = new PoseNet();

        static public int _texW = 0;
        static public int _texH = 0;

        static public float _rawImageWidth = 0;
        static public float _rawImageHeight = 0;


        static void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                lineMaterial.SetInt("_ZWrite", 0);
            }
        }

        public void DrawResults(PoseNet.Pose[] poses, int errorCode)
        {
            if (poses.Length == 0)
            {
                return;
            }
            CreateLineMaterial();
            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);
            GL.Begin(GL.QUADS);
            Color color = errorCode == 0 ? new Color(0.125f, 0.854f, 0.811f, 0.8f) : new Color(1f, 0f, 0f, 0.8f);
            GL.Color(color);
            float minPoseConfidence = 0.05f;

            foreach (var pose in poses)
            {
                //DrawResults(poses);
                if (pose.score >= minPoseConfidence)
                {
                    //DrawKeypoint(pose.keypoints,
                    //    minPoseConfidence, 0.02f);
                    DrawSkeleton(pose.keypoints,
                        minPoseConfidence, 1f);
                }
            }

            GL.End();
            GL.PopMatrix();

            foreach (var pose in poses)
            {
                //DrawResults(poses);
                if (pose.score >= minPoseConfidence)
                {
                    DrawKeypoint(pose.keypoints,
                        minPoseConfidence, 1f);
                }
            }
        }

        public void DrawKeypoint(PoseNet.Keypoint[] keypoints, float minConfidence, float scale)
        {
            float radius = 5f;
            int _Width, _Height;

            //frame.ResizePro(Width, Height, false, false);
            if (_texW > _texH)
            {
                _Width = 257;
                _Height = Mathf.CeilToInt(257f * ((float)_texH / (float)_texW));
            }
            else
            {
                _Width = Mathf.CeilToInt(257f * ((float)_texW / (float)_texH));
                _Height = 257;
            }

            // float rw = (float)Screen.height / (float)Screen.width;
            // float canvasHeight = 750f * rw;

            // float rawImageRatio = (float)_texW / (float)_texH;
            // float pixelRatio = (float)Screen.width / Screen.dpi;
            float rawImageHeight = _rawImageHeight;
            float rawImageWidth = _rawImageWidth;

            var excludes = new ArrayList { "nose", "leftEye", "rightEye", "leftEar", "rightEar" };

            foreach (var keypoint in keypoints)
            {
                if (excludes.Contains(keypoint.part)) { continue; }

                if (keypoint.score < minConfidence) { continue; }

                GL.PushMatrix();
                lineMaterial.SetPass(0);
                GL.MultMatrix(transform.localToWorldMatrix);
                // GL.Begin(GL.LINES);
                // GL.Color(new Color());

                var x = keypoint.position.x;
#if UNITY_ANDROID && !UNITY_EDITOR
                x = _Width - x;
#endif
                var y = keypoint.position.y;

                float ratioPosX = x / (float)_Width;
                float ratioPosY = y / (float)_Height;

                float _x = rawImageWidth * ratioPosX;
                float _y = rawImageHeight * ratioPosY;


                DrawCircleFill(new Vector3((_x - rawImageWidth / 2.0f) * 1f, (_y - rawImageHeight / 2.0f) * -1f, 0f), radius, new Color(r: 0.521f, g: 1.000f, b: 0.403f, a: 0.800f));
                GL.PopMatrix();
            }
        }

        public void DrawCircleFill(Vector3 center, float radius, Color col)
        {
            int divCou = 32;
            GL.Begin(GL.TRIANGLES);
            GL.Color(col);

            float dPos = 0.0f;
            float dd = (Mathf.PI * 2.0f) / (float)divCou;
            Vector3 v0 = new Vector3(0.0f, 0.0f);
            Vector3 v1 = new Vector3(0.0f, 0.0f);
            for (int i = 0; i <= divCou; i++)
            {
                v1.x = Mathf.Cos(dPos) * radius;
                v1.y = Mathf.Sin(dPos) * radius;
                v1.z = 0.0f;
                v1 += center;
                if (i != 0)
                {
                    GL.Vertex3(center.x, center.y, center.z);
                    GL.Vertex3(v1.x, v1.y, v1.z);
                    GL.Vertex3(v0.x, v0.y, v0.z);
                }
                v0 = v1;
                dPos += dd;
            }
            GL.End();
        }
        public void DrawSkeleton(PoseNet.Keypoint[] keypoints, float minConfidence, float scale)
        {
            var adjacentKeyPoints = posenet.GetAdjacentKeyPoints(
                keypoints, minConfidence);
            int _Width, _Height;

            //frame.ResizePro(Width, Height, false, false);
            if (_texW > _texH)
            {
                _Width = 257;
                _Height = Mathf.CeilToInt(257f * ((float)_texH / (float)_texW));
            }
            else
            {
                _Width = Mathf.CeilToInt(257f * ((float)_texW / (float)_texH));
                _Height = 257;
            }


            float rawImageHeight = _rawImageHeight;
            float rawImageWidth = _rawImageWidth;

            foreach (var keypoint in adjacentKeyPoints)
            {
                var x1 = keypoint.Item1.position.x;
#if UNITY_ANDROID && !UNITY_EDITOR
                x1 = _Width - x1;
#endif
                var y1 = keypoint.Item1.position.y;

                // 
                var x2 = keypoint.Item2.position.x;
#if UNITY_ANDROID && !UNITY_EDITOR
                x2 = _Width - x2;
#endif
                var y2 = keypoint.Item2.position.y;

                float ratioPosX1 = x1 / (float)_Width;
                float ratioPosY1 = y1 / (float)_Height;
                float ratioPosX2 = x2 / (float)_Width;
                float ratioPosY2 = y2 / (float)_Height;


                float _x1 = rawImageWidth * ratioPosX1;
                float _y1 = rawImageHeight * ratioPosY1;
                float _x2 = rawImageWidth * ratioPosX2;
                float _y2 = rawImageHeight * ratioPosY2;


                // Debug.Log($"{keypoint.Item1.part} ::: {keypoint.Item1.position.x}:{keypoint.Item1.position.y}");
                var px1 = (_x1 - rawImageWidth / 2f);
                var py1 = (_y1 - rawImageHeight / 2f) * -1f;
                var px2 = (_x2 - rawImageWidth / 2f);
                var py2 = (_y2 - rawImageHeight / 2f) * -1f;


                // Debug.Log($"item1 ::: {x1}:{y1}");

                DrawLine2D(new Vector2(px1, py1),
                           new Vector2(px2, py2), 2.5f);
            }
        }

        void DrawLine2D(Vector3 v0, Vector3 v1, float lineWidth)
        {
            Vector3 n = ((new Vector3(v1.y, v0.x, 0.0f)) - (new Vector3(v0.y, v1.x, 0.0f))).normalized * lineWidth;
            GL.Vertex3(v0.x - n.x, v0.y - n.y, 0.0f);
            GL.Vertex3(v0.x + n.x, v0.y + n.y, 0.0f);
            GL.Vertex3(v1.x + n.x, v1.y + n.y, 0.0f);
            GL.Vertex3(v1.x - n.x, v1.y - n.y, 0.0f);
        }
    }

}
