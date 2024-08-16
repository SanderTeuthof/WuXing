using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ExtendedDoGRenderFeature : ScriptableRendererFeature
{
    public class ExtendedDoGRenderPass : ScriptableRenderPass
    {
        public Material blitMaterial = null;
        public FilterMode filterMode { get; set; }

        private ExtendedDoGSettings settings;
        private RenderTargetIdentifier source { get; set; }
        private RenderTargetIdentifier destination { get; set; }

        RenderTargetHandle tempTexture1;
        RenderTargetHandle tempTexture2;
        RenderTargetHandle tempLabTex;

        string m_ProfilerTag;

        public ExtendedDoGRenderPass(RenderPassEvent renderPassEvent, ExtendedDoGSettings settings, string tag)
        {
            this.renderPassEvent = renderPassEvent;
            this.settings = settings;
            blitMaterial = settings.blitMaterial;
            m_ProfilerTag = tag;
            tempTexture1.Init("_TemporaryColorTexture1");
            tempTexture2.Init("_TemporaryColorTexture2");
            tempLabTex.Init("_TemporaryLabTexture");
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

            // Apply settings to the material
            blitMaterial.SetFloat("_SigmaC", settings.SigmaC);
            blitMaterial.SetFloat("_SigmaE", settings.SigmaE);
            blitMaterial.SetFloat("_SigmaM", settings.SigmaM);
            blitMaterial.SetFloat("_SigmaA", settings.SigmaA);
            blitMaterial.SetFloat("_K", settings.stdevScaleK);
            blitMaterial.SetFloat("_Tau", settings.SharpnessTau);
            blitMaterial.SetFloat("_Phi", settings.softThresholdPhi);
            blitMaterial.SetFloat("_Threshold", settings.whitePoint);
            blitMaterial.SetFloat("_Threshold2", settings.secondWhitePoint);
            blitMaterial.SetFloat("_Threshold3", settings.thirdWhitePoint);
            blitMaterial.SetFloat("_Threshold4", settings.fourthWhitePoint);
            blitMaterial.SetFloat("_Thresholds", settings.quantizerStep);
            blitMaterial.SetFloat("_BlendStrength", settings.blendStrength);
            blitMaterial.SetFloat("_DoGStrength", settings.termStrength);
            blitMaterial.SetFloat("_HatchTexRotation", settings.hatchRotation);
            blitMaterial.SetFloat("_HatchTexRotation1", settings.secondHatchRotation);
            blitMaterial.SetFloat("_HatchTexRotation2", settings.thirdHatchRotation);
            blitMaterial.SetFloat("_HatchTexRotation3", settings.fourthHatchRotation);
            blitMaterial.SetFloat("_HatchRes1", settings.hatchResolution);
            blitMaterial.SetFloat("_HatchRes2", settings.hatchResolution2);
            blitMaterial.SetFloat("_HatchRes3", settings.hatchResolution3);
            blitMaterial.SetFloat("_HatchRes4", settings.hatchResolution4);
            blitMaterial.SetInt("_EnableSecondLayer", settings.enableSecondLayer ? 1 : 0);
            blitMaterial.SetInt("_EnableThirdLayer", settings.enableThirdLayer ? 1 : 0);
            blitMaterial.SetInt("_EnableFourthLayer", settings.enableFourthLayer ? 1 : 0);
            blitMaterial.SetInt("_EnableColoredPencil", settings.enableColoredPencil ? 1 : 0);
            blitMaterial.SetFloat("_BrightnessOffset", settings.brightnessOffset);
            blitMaterial.SetFloat("_Saturation", settings.saturation);
            blitMaterial.SetVector("_IntegralConvolutionStepSizes", new Vector4(settings.lineConvolutionStepSizes.x, settings.lineConvolutionStepSizes.y, settings.edgeSmoothStepSizes.x, settings.edgeSmoothStepSizes.y));
            blitMaterial.SetVector("_MinColor", settings.minColor);
            blitMaterial.SetVector("_MaxColor", settings.maxColor);
            blitMaterial.SetInt("_Thresholding", (int)settings.thresholdMode);
            blitMaterial.SetInt("_BlendMode", (int)settings.blendMode);
            blitMaterial.SetInt("_Invert", settings.invert ? 1 : 0);
            blitMaterial.SetInt("_CalcDiffBeforeConvolution", settings.calcDiffBeforeConvolution ? 1 : 0);
            blitMaterial.SetInt("_HatchingEnabled", settings.enableHatching ? 1 : 0);
            blitMaterial.SetTexture("_HatchTex", settings.hatchTexture);

            cmd.GetTemporaryRT(tempTexture1.id, opaqueDesc, filterMode);
            cmd.GetTemporaryRT(tempTexture2.id, opaqueDesc, filterMode);
            cmd.GetTemporaryRT(tempLabTex.id, opaqueDesc, filterMode);
            RenderTexture tempRT1 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.Default);
            RenderTexture tempRT2 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.Default);

            // 1. RGB to LAB conversion
            Blit(cmd, source, tempLabTex.id, blitMaterial, 0);


            // 2. Calculate eigenvectors
            Blit(cmd, tempLabTex.id, tempTexture2.id, blitMaterial, 1);

            // 3. TFM Blur Pass 1
            Blit(cmd, tempTexture2.id, tempTexture1.id, blitMaterial, 2);

            // 4. TFM Blur Pass 2
            Blit(cmd, tempTexture1.id, tempTexture2.id, blitMaterial, 3);

            Blit(cmd, tempTexture2.id, tempRT1);
            settings.blitMaterial.SetTexture("_TFM", tempRT1);


            if (settings.useFlow) // make bool _useFlow
            {
                // 5. FDoG Blur Pass 1
                Blit(cmd, tempLabTex.id, tempTexture1.id, blitMaterial, 4);

                // 6. FDoG Blur Pass 2 + Thresholding
                Blit(cmd, tempTexture1.id, tempTexture2.id, blitMaterial, 5);
            }
            else
            {
                // 7. Non FDoG Blur Pass 1 (if needed)
                Blit(cmd, tempLabTex.id, tempTexture1.id, blitMaterial, 6);

                // 8. Non FDoG Blur Pass 2 (if needed)
                Blit(cmd, tempTexture1.id, tempTexture2.id, blitMaterial, 7);
            }
            
            if (settings.smoothEdge) // make bool _smoothEdge
            {
                // 9. Anti-Aliasing Pass (if needed)
                Blit(cmd, tempTexture2.id, tempTexture1.id, blitMaterial, 8);

                //cmd.SetGlobalTexture("_DogTex", tempTexture1.id);
            }
            else
            {
                Blit(cmd, tempTexture2.id, tempTexture1.id);
            }
            Blit(cmd, tempTexture1.id, tempRT2);

            settings.blitMaterial.SetTexture("_DogTex", tempRT2);

            //Blit(cmd, tempRT1, destination);

            // 10. Blend Pass
            Blit(cmd, source, destination, blitMaterial, 9);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            RenderTexture.ReleaseTemporary(tempRT1);
            RenderTexture.ReleaseTemporary(tempRT2);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture1.id);
            cmd.ReleaseTemporaryRT(tempTexture2.id);
            cmd.ReleaseTemporaryRT(tempLabTex.id);
        }
    }

    [System.Serializable]
    public class ExtendedDoGSettings
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

        [Header("Edge Tangent Flow Settings")]
        public bool useFlow = true;
        [Range(0.0f, 20.0f)]
        public float SigmaC = 5.0f;
        [Range(0.0f, 20.0f)]
        public float SigmaM = 20.0f;
        public Vector2 lineConvolutionStepSizes = new Vector2(3.0f, 3.0f);
        public bool calcDiffBeforeConvolution = true;

        [Header("Difference Of Gaussians Settings")]
        [Range(0.0f, 10.0f)]
        public float SigmaE = 2.0f;
        [Range(0.1f, 5.0f)]
        public float stdevScaleK = 1.6f;
        [Range(0.0f, 100.0f)]
        public float SharpnessTau = 1.0f;

        [Header("Threshold Settings")]
        public ThresholdMode thresholdMode = ThresholdMode.NoThreshold;
        [Range(1, 16)] 
        public int quantizerStep = 2;
        [Range(0.0f, 100.0f)] 
        public float whitePoint = 50.0f;
        [Range(0.0f, 10.0f)] 
        public float softThresholdPhi = 1.0f;
        public bool invert = false;

        [Header("Anti Aliasing Settings")]
        public bool smoothEdge = true;
        [Range(0.0f, 10.0f)] 
        public float SigmaA = 1.0f;
        public Vector2 edgeSmoothStepSizes = new Vector2(1.0f, 1.0f);

        [Header("Cross Hatch Settings")]
        public bool enableHatching = false;
        public Texture hatchTexture = null;
        [Space(10)]
        [Range(0.0f, 8.0f)]
        public float hatchResolution = 1.0f;
        [Range(-180.0f, 180.0f)]
        public float hatchRotation = 90.0f;

        [Space(10)]
        public bool enableSecondLayer = true;
        [Range(0.0f, 100.0f)]
        public float secondWhitePoint = 20.0f;
        [Range(0.0f, 8.0f)]
        public float hatchResolution2 = 1.0f;
        [Range(-180.0f, 180.0f)]
        public float secondHatchRotation = 60.0f;

        [Space(10)]
        public bool enableThirdLayer = true;
        [Range(0.0f, 100.0f)]
        public float thirdWhitePoint = 30.0f;
        [Range(0.0f, 8.0f)]
        public float hatchResolution3 = 1.0f;
        [Range(-180.0f, 180.0f)]
        public float thirdHatchRotation = 120.0f;

        [Space(10)]
        public bool enableFourthLayer = true;
        [Range(0.0f, 100.0f)]
        public float fourthWhitePoint = 30.0f;
        [Range(0.0f, 8.0f)]
        public float hatchResolution4 = 1.0f;
        [Range(-180.0f, 180.0f)]
        public float fourthHatchRotation = 120.0f;

        [Space(10)]
        public bool enableColoredPencil = false;
        [Range(-1.0f, 1.0f)]
        public float brightnessOffset = 0.0f;
        [Range(0.0f, 5.0f)]
        public float saturation = 1.0f;

        [Header("Blend Settings")]
        [Range(0.0f, 5.0f)]
        public float termStrength = 1.0f;
        public BlendMode blendMode = BlendMode.NoBlend;
        public Color minColor = new Color(0.0f, 0.0f, 0.0f);
        public Color maxColor = new Color(1.0f, 1.0f, 1.0f);
        [Range(0.0f, 2.0f)] 
        public float blendStrength = 1.0f;
    }

    public enum ThresholdMode
    {
        NoThreshold = 0,
        Tanh,
        Quantization,
        SmoothQuantization
    }

    public enum BlendMode
    {
        NoBlend = 0,
        Interpolate,
        TwoPointInterpolate
    }

    public enum Target
    {
        CameraColor,
        TextureID,
        RenderTextureObject
    }

    public ExtendedDoGSettings settings = new ExtendedDoGSettings();
    public ExtendedDoGRenderPass blitPass;

    public override void Create()
    {
        var passIndex = settings.blitMaterial != null ? settings.blitMaterial.passCount - 1 : 1;
        settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);
        blitPass = new ExtendedDoGRenderPass(settings.Event, settings, name);

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
