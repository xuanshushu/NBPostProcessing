using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MhRender.RendererFeatures
{
    public class ScreenColorRenderPass : ScriptableRenderPass
    {
        private RTHandle _screenColorHandle;
        private RTHandle _tempRTHandle;
        private ProfilingSampler _profilingSampler;
        private readonly Downsampling _downSampling;
        readonly Material _material;
        private static readonly int CameraTexture = Shader.PropertyToID("_CameraTexture");
        private static readonly int SampleOffset = Shader.PropertyToID("_SampleOffset");

        public ScreenColorRenderPass(Material material, Downsampling downSampling)
        {
            _material = material;
            _downSampling = downSampling;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(_screenColorHandle);
           
        }

        public void SetUp(RTHandle colorHandle)
        {
            _profilingSampler ??= new ProfilingSampler("ScreenColorRender");
            _screenColorHandle = colorHandle;
            RenderTextureDescriptor descriptor = _screenColorHandle.rt.descriptor;
            switch (_downSampling)
            {
                case Downsampling._2xBilinear:
                    descriptor.width /= 2;
                    descriptor.height /= 2;
                    break;
                case Downsampling._4xBilinear:
                    descriptor.width /= 4;
                    descriptor.height /= 4;
                    break;
                case Downsampling._4xBox:
                    descriptor.width /= 4;
                    descriptor.height /= 4;
                    break;
            }
            RenderingUtils.ReAllocateIfNeeded(ref _tempRTHandle, descriptor,name:"CopyColorRT");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!(renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView))
                return;
            if (_material == null)
                return;
            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, _profilingSampler))
            {
                _material.SetTexture(CameraTexture, _screenColorHandle);
                switch (_downSampling)
                {
                    case Downsampling._2xBilinear:
                        Blitter.BlitTexture(cmd, _screenColorHandle, _tempRTHandle, _material, 0);
                        break;
                    case Downsampling._4xBilinear:
                        Blitter.BlitTexture(cmd, _screenColorHandle, _tempRTHandle, _material, 0);
                        break;
                    case Downsampling._4xBox:
                        _material.SetFloat(SampleOffset,2);
                        Blitter.BlitTexture(cmd, _screenColorHandle, _tempRTHandle, _material, 1);
                        break;
                    default:
                        Blitter.BlitTexture(cmd, _screenColorHandle, _tempRTHandle, _material, 0);  
                        break;
                }
                cmd.SetGlobalTexture("_ScreenColorCopy1", _tempRTHandle);
            }
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            _tempRTHandle?.Release();
        }
    }
}