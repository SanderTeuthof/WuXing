using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Blit : ScriptableRendererFeature
{
    public class BlitPass : ScriptableRenderPass
    {
        public Material blitMaterial = null;
        public FilterMode filterMode { get; set; }

        private BlitSettings settings;
        private RenderTargetIdentifier source { get; set; }
        private RenderTargetIdentifier destination { get; set; }

        RenderTargetHandle tempTexture1;
        RenderTargetHandle tempTexture2;
        string m_ProfilerTag;

        public BlitPass(RenderPassEvent renderPassEvent, BlitSettings settings, string tag)
        {
            this.renderPassEvent = renderPassEvent;
            this.settings = settings;
            blitMaterial = settings.blitMaterial;
            m_ProfilerTag = tag;
            tempTexture1.Init("_TemporaryColorTexture1");
            tempTexture2.Init("_TemporaryColorTexture2");
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

            // Handle passes according to the material's pass count
            if (settings.passCount > blitMaterial.passCount)
            {
                Debug.LogError("Pass count exceeds the number of passes in the material.");
                return;
            }

            if (settings.passCount >= 1)
            {
                cmd.GetTemporaryRT(tempTexture1.id, opaqueDesc, filterMode);
                Blit(cmd, source, tempTexture1.Identifier(), blitMaterial, 0);
            }

            if (settings.passCount >= 2)
            {
                cmd.GetTemporaryRT(tempTexture2.id, opaqueDesc, filterMode);
                Blit(cmd, tempTexture1.Identifier(), tempTexture2.Identifier(), blitMaterial, 1);
            }

            if (settings.passCount >= 3)
            {
                Blit(cmd, tempTexture2.Identifier(), destination, blitMaterial, 2);
            }
            else
            {
                Blit(cmd, tempTexture1.Identifier(), destination, blitMaterial, settings.passCount - 1);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture1.id);
            if (settings.passCount > 1)
            {
                cmd.ReleaseTemporaryRT(tempTexture2.id);
            }
        }
    }

    [System.Serializable]
    public class BlitSettings
    {
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

        public Material blitMaterial = null;
        public int blitMaterialPassIndex = 0;
        public bool setInverseViewMatrix = false;
        public bool requireDepthNormals = false;
        public int passCount = 1; // Number of passes

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

    public BlitSettings settings = new BlitSettings();
    public BlitPass blitPass;

    public override void Create()
    {
        var passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
        settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);
        blitPass = new BlitPass(settings.Event, settings, name);

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
