using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using System;
using UnityEngine.Rendering;

namespace MhRender.RendererFeatures
{
    [ExecuteAlways]
    [AddComponentMenu("Rendering/Lens Flare (Mh2)")]
    public class Mh2LensFlareComponent : MonoBehaviour
    {
        // Start is called before the first frame update

        [TitleGroup("基础设置")] [LabelText("光晕Mesh")]
        public Mesh LensFlareMesh;

        [TitleGroup("基础设置")] [LabelText("光晕贴图")]
        public Texture LensFlareTexture;

        [TitleGroup("基础设置")] [LabelText("全局颜色")] [ColorUsageAttribute(true, true)]
        public Color LensFlareColor = Color.white;

        [TitleGroup("基础设置")] [LabelText("全局尺寸")] [Range(0.01f, 5.0f)]
        public float LensFlareSize = 1.0f;
        
        [TitleGroup("基础设置")] [LabelText("全局角度")] [Range(0.0f, 360.0f)]
        public float LensFlareRotation = 0.0f;

        [Serializable]
        public enum _BlendMode
        {
            Add = 1,
            PreMul = 2,
            AlphaLerp = 3
        }

        [TitleGroup("基础设置")] [LabelText("混合模式")] [EnumPaging]
        public _BlendMode BlendMode = _BlendMode.Add;

        [TitleGroup("基础设置")] [LabelText("光晕配置列表")]
        public List<Mh2LensFlareSingleConfig> lfList = new List<Mh2LensFlareSingleConfig>();

        [Serializable]
        public class Mh2LensFlareSingleConfig
        {
            [LabelText("展开配置")]
            public Mh2LensFlareSingleConfigDetail lfDetail;
        }

        [Serializable]
        public class Mh2LensFlareSingleConfigDetail
        {
            [LabelText("光晕颜色")] [ColorUsageAttribute(true, true)]
            public Color lfColor = Color.white;

            [LabelText("光晕尺寸")]
            public float lfSize = 1.0f;

            [LabelText("光晕位置")]
            public float lfPosition = 0.0f;

            [LabelText("光晕旋转")] [Wrap(0f, 360f)]
            public float lfRotation = 0.0f;

            [LabelText("自动旋转")] [ToggleLeft]
            public bool lfAutoRotation;

            [LabelText("径向畸变")] [ToggleLeft]
            public bool lfDistortion;

            [ShowIf("lfDistortion")] [LabelText("径向畸变尺寸")]
            public float lfDSize = 1.0f;

        }
        
        [TitleGroup("场景遮挡")] [LabelText("启用")] [ToggleLeft]
        public bool OcclusionToggle = true;

        [ShowIf("OcclusionToggle")] [TitleGroup("场景遮挡")] [LabelText("遮挡半径")] [Range(0.01f, 1.0f)]
        public float OcclusionRadius = 0.05f;
        
        [ShowIf("OcclusionToggle")] [TitleGroup("场景遮挡")] [LabelText("遮挡采样")] [Range(1, 16)]
        public int OcclusionSample = 4;
        
        private void OnEnable()
        {
            //启动flag
            RenderPipelineManager.beginCameraRendering += ExecuteCamera;
            Shader.SetGlobalInt("_LensFlareFlag", (int)BlendMode);
            //设置颜色
            SetLensFlareColor();
            LensFlareRenderPass._LensflareMesh = LensFlareMesh;
            Shader.SetGlobalTexture("_LensFlareTex", LensFlareTexture);
        }

        private void OnDisable()
        {
            //关闭flag
            RenderPipelineManager.beginCameraRendering -= ExecuteCamera;
            Shader.SetGlobalInt("_LensFlareFlag", 0);
            Shader.SetGlobalVector("_lensFlareGlobalVector", new Vector4(0f,0f,0f,0f));
        }

        private String[] LensFlareVar_Color = new[]
        {
            "_lensFlareColor01", "_lensFlareColor02", "_lensFlareColor03", "_lensFlareColor04", "_lensFlareColor05",
            "_lensFlareColor06", "_lensFlareColor07", "_lensFlareColor08", "_lensFlareColor09", "_lensFlareColor10"
        };
        private String[] LensFlareVar_Vector = new[]
        {
            "_lensFlareVector01", "_lensFlareVector02", "_lensFlareVector03", "_lensFlareVector04", "_lensFlareVector05",
            "_lensFlareVector06", "_lensFlareVector07", "_lensFlareVector08", "_lensFlareVector09", "_lensFlareVector10"
        };
        private void SetLensFlareColor()
        {
            for (int i = 0; i < lfList.Count; i++)
            {
                Shader.SetGlobalColor(LensFlareVar_Color[i], lfList[i].lfDetail.lfColor * LensFlareColor);
            }
        }

        private void SetLensFlareVector(Vector2 screenPos, float screenRatio)
        {
            float angularOffset = SystemInfo.graphicsUVStartsAtTop ? LensFlareRotation : -LensFlareRotation;
            float globalCos0 = Mathf.Cos(-angularOffset * Mathf.Deg2Rad);
            float globalSin0 = Mathf.Sin(-angularOffset * Mathf.Deg2Rad);
            
            Vector2 vScreenRatio = new Vector2(screenRatio, 1.0f);
            //每个设置旋转和位置到vector里
            for (int i = 0; i < lfList.Count; i++)
            {
                Vector2 rayOffset = GetLensFlareRayOffset(screenPos, lfList[i].lfDetail.lfPosition, globalCos0, globalSin0);
                float ratRotation = GetLensFlareRotation(vScreenRatio, rayOffset, lfList[i].lfDetail.lfRotation, lfList[i].lfDetail.lfAutoRotation);
                float size = lfList[i].lfDetail.lfSize * LensFlareSize;
                if (lfList[i].lfDetail.lfDistortion) //径向畸变
                {
                    size = ComputeLocalSize(rayOffset, size, lfList[i].lfDetail.lfDSize, screenPos);
                }
                Shader.SetGlobalVector(LensFlareVar_Vector[i], new Vector4(rayOffset.x, rayOffset.y, ratRotation, size));
            }
        }
        
        //求每个面片的偏移位置
        static Vector2 GetLensFlareRayOffset(Vector2 screenPos, float position, float globalCos0, float globalSin0)
        {
            Vector2 rayOff = -(screenPos + screenPos * (position * 0.1f - 1.0f));
            return new Vector2(globalCos0 * rayOff.x - globalSin0 * rayOff.y,
                globalSin0 * rayOff.x + globalCos0 * rayOff.y);
        }
        
        //求每个面片的rotation
        static float GetLensFlareRotation(Vector2 vLocalScreenRatio, Vector2 rayOffset, float angleDeg, bool autoRotate)
        {
            if (!SystemInfo.graphicsUVStartsAtTop)
            {
                angleDeg *= -1;
            }
            float rotation = angleDeg;
            rotation += 180f;
            if (autoRotate)
            {
                Vector2 pos = (rayOffset.normalized * vLocalScreenRatio);
                rotation += -Mathf.Rad2Deg * Mathf.Atan2(pos.y, pos.x);
            }
            rotation *= Mathf.Deg2Rad;
            return -rotation;
        }
        
        //求径向畸变
        static float ComputeLocalSize(Vector2 rayOff, float curSize, float distortionSize, Vector2 screenPos)
        {
            Vector2 localRadPos;
            float localRadius;

            localRadPos = screenPos + rayOff;
            localRadius = Mathf.Clamp01(localRadPos.magnitude); // l2 norm (instead of l1 norm)
            
            return Mathf.Lerp(curSize, distortionSize, localRadius);
        }
        
        private void Update()
        {
            //实时计算当前位置等参数，传入到shader中，颜色、贴图等设置一次即可，位置等需要实时刷新
            //先判断是否在场景内，不然不刷新
            //findshader setglabalxxx
            if (!Application.isPlaying)
            {
                SetLensFlareColor();
                LensFlareRenderPass._LensflareMesh = LensFlareMesh;
                Shader.SetGlobalTexture("_LensFlareTex", LensFlareTexture);   
            }
        }
        private void ExecuteCamera(ScriptableRenderContext context, Camera cam)
        {
            //计算光晕中心屏幕空间位置
            Vector3 positionWS;
            Vector3 viewportPos;
            var gpuNonJitteredProj = GL.GetGPUProjectionMatrix(cam.projectionMatrix, true);
            var gpuVP = gpuNonJitteredProj * cam.worldToCameraMatrix;
            positionWS = -this.transform.forward * cam.farClipPlane;
            viewportPos = WorldToViewportLocal(true, gpuVP, cam.transform.position, positionWS);

            if (viewportPos.x < 0.0f || viewportPos.x > 1.0f ||
                viewportPos.y < 0.0f || viewportPos.y > 1.0f ||
                viewportPos.z < 0.0f)
            {
                //这里不在屏幕内，应该直接flag+不绘制
                Shader.SetGlobalVector("_lensFlareGlobalVector", new Vector4(0f,0f,0f,0f));
                Shader.SetGlobalInt("_LensFlareFlag", 0);
                return;
            }
            Shader.SetGlobalInt("_LensFlareFlag", (int)BlendMode);
            Vector2 screenPos = new Vector2(2.0f * viewportPos.x - 1.0f, -(2.0f * viewportPos.y - 1.0f));
            if(!SystemInfo.graphicsUVStartsAtTop)
                screenPos.y = -screenPos.y;
            
            Shader.SetGlobalVector("_lensFlareGlobalVector", new Vector4(screenPos.x, screenPos.y, 1.0f, 1.0f));

            Vector4 lfOcVec = new Vector4();
            lfOcVec.x = OcclusionToggle ? 1 : 0;
            lfOcVec.y = OcclusionRadius;
            lfOcVec.z = OcclusionSample;
            lfOcVec.w = viewportPos.z * 0.95f;
            Shader.SetGlobalVector("_lensFlareOcclusionVector", lfOcVec);
            SetLensFlareVector(screenPos,(float)cam.pixelWidth / cam.pixelHeight);
        }
        //转换光晕位置到视角空间使用
        static Vector3 WorldToViewportLocal(bool isCameraRelative, Matrix4x4 viewProjMatrix, Vector3 cameraPosWS, Vector3 positionWS)
        {
            Vector3 localPositionWS = positionWS;
            if (isCameraRelative)
            {
                localPositionWS -= cameraPosWS;
            }
            Vector4 viewportPos4 = viewProjMatrix * localPositionWS;
            Vector3 viewportPos = new Vector3(viewportPos4.x, viewportPos4.y, 0f);
            viewportPos /= viewportPos4.w;
            viewportPos.x = viewportPos.x * 0.5f + 0.5f;
            viewportPos.y = viewportPos.y * 0.5f + 0.5f;
            viewportPos.y = 1.0f - viewportPos.y;
            viewportPos.z = viewportPos4.w;
            return viewportPos;
        }
    }
}
