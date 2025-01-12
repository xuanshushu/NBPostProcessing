using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MhRender.RendererFeatures
{
    public class ScreenColorOutput : ScriptableRendererFeature
    {
        private ScreenColorRenderPass _screenColorRenderPass;
        public Downsampling downSampling = Downsampling.None;
        public Shader shader;
        private Material _material;
        public override void Create()
        {
            if(!shader)
                return;
          
            _material = CoreUtils.CreateEngineMaterial(shader);
            _screenColorRenderPass = new ScreenColorRenderPass(_material,downSampling);
            _screenColorRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game ||renderingData.cameraData.cameraType == CameraType.SceneView)
            {
                _screenColorRenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
                _screenColorRenderPass.SetUp(renderer.cameraColorTargetHandle);
            }
      
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game ||renderingData.cameraData.cameraType == CameraType.SceneView)
            {
                renderer.EnqueuePass(_screenColorRenderPass);
            }
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(_material);
            _screenColorRenderPass?.Dispose();
        }
    }
}