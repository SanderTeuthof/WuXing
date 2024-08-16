using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static Blit;

public class DoGRenderFeature : ScriptableRendererFeature
{
    public class DoGRenderPass : ScriptableRenderPass
    {
        public Material blitMaterial = null;
        public FilterMode filterMode { get; set; }

        private DoGSettings settings;
        private RenderTargetIdentifier source { get; set; }
        private RenderTargetIdentifier destination { get; set; }

        RenderTargetHandle tempTexture1;
        RenderTargetHandle gaussianTex;
        string m_ProfilerTag;

        public DoGRenderPass(RenderPassEvent renderPassEvent, DoGSettings settings, string tag)
        {
            this.renderPassEvent = renderPassEvent;
            this.settings = settings;
            blitMaterial = settings.blitMaterial;
            m_ProfilerTag = tag;
            tempTexture1.Init("_TemporaryColorTexture1");
            gaussianTex.Init("_GaussianTex");
        }

        public void Setup(ScriptableRenderer renderer)
        {
            if (settings.requireDepthNormals)
            {
                ConfigureInput(ScriptableRenderPassInput.Normal);
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            var renderer = renderingData.cameraData.renderer;

            // Set Source / Destination
            if (settings.srcType == Target.CameraColor)
            {
                source = renderer.cameraColorTarget;
            }
            else if (settings.srcType == Target.TextureID)
            {
                source = new RenderTargetIdentifier(settings.srcTextureId);
            }
            else if (settings.srcType == Target.RenderTextureObject)
            {
                source = new RenderTargetIdentifier(settings.srcTextureObject);
            }

            if (settings.dstType == Target.CameraColor)
            {
                destination = renderer.cameraColorTarget;
            }
            else if (settings.dstType == Target.TextureID)
            {
                destination = new RenderTargetIdentifier(settings.dstTextureId);
            }
            else if (settings.dstType == Target.RenderTextureObject)
            {
                destination = new RenderTargetIdentifier(settings.dstTextureObject);
            }

            // Apply Gaussian Blur (Pass 1 and Pass 2)
            cmd.GetTemporaryRT(tempTexture1.id, opaqueDesc, filterMode);
            cmd.GetTemporaryRT(gaussianTex.id, opaqueDesc, filterMode);

            // First Pass (Horizontal Blur)
            Blit(cmd, source, tempTexture1.Identifier(), blitMaterial, 0);

            // Second Pass (Vertical Blur)
            Blit(cmd, tempTexture1.Identifier(), gaussianTex.Identifier(), blitMaterial, 1);

            // Third Pass (Difference of Gaussians)
            Blit(cmd, gaussianTex.Identifier(), destination, blitMaterial, 2);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture1.id);
            cmd.ReleaseTemporaryRT(gaussianTex.id);
        }
    }



    [System.Serializable]
    public class DoGSettings
    {
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

        public Material blitMaterial = null;
        public int blitMaterialPassIndex = 0;
        public bool setInverseViewMatrix = false;
        public bool requireDepthNormals = false;

        public Target srcType = Target.CameraColor;
        public string srcTextureId = "_CameraColorTexture";
        public RenderTexture srcTextureObject;

        public Target dstType = Target.CameraColor;
        public string dstTextureId = "_BlitPassTexture";
        public RenderTexture dstTextureObject;

        public bool overrideGraphicsFormat = false;
        public UnityEngine.Experimental.Rendering.GraphicsFormat graphicsFormat;

        public bool canShowInSceneView = true;
    }

    public enum Target
    {
        CameraColor,
        TextureID,
        RenderTextureObject
    }

    public DoGSettings settings = new DoGSettings();
    public DoGRenderPass blitPass;

    public override void Create()
    {
        var passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
        settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);
        blitPass = new DoGRenderPass(settings.Event, settings, name);

        if (settings.graphicsFormat == UnityEngine.Experimental.Rendering.GraphicsFormat.None)
        {
            settings.graphicsFormat = SystemInfo.GetGraphicsFormat(UnityEngine.Experimental.Rendering.DefaultFormat.LDR);
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.isPreviewCamera || renderingData.cameraData.isSceneViewCamera) return;
        if (!settings.canShowInSceneView && renderingData.cameraData.isSceneViewCamera) return;

        if (settings.blitMaterial == null)
        {
            Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }

        blitPass.Setup(renderer);
        renderer.EnqueuePass(blitPass);
    }
}
