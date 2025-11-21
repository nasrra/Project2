using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenSpaceOutlines : ScriptableRendererFeature
{

    [SerializeField] public RenderPassEvent renderPassEvent;


    /// 
    /// Definitions.
    /// 

    [Serializable]
    private class ViewSpaceNormalsTextureSettings
    {
        [SerializeField] public RenderTextureFormat colorFormat;
        [SerializeField] public Color backgroundColor;
        [SerializeField] public int depthBufferBits;

        public ViewSpaceNormalsTextureSettings (RenderPassEvent renderPassEvent)
        {
        }
    }

    // // This generates the scene view space normals texture.
    
    // private class ViewSpaceNormalsTexturePass : ScriptableRenderPass
    // {

    //     // this is needed as this render pass renders the normasl data to a texture.

    //     [Obsolete] private readonly RenderTargetHandle normals;
    //     private ViewSpaceNormalsTextureSettings viewSpaceNormalsTextureSettings;
    //     private List<ShaderTagId> shaderTagIds;

    //     public ViewSpaceNormalsTexturePass(RenderPassEvent renderPassEvent)
    //     {
    //         viewSpaceNormalsTextureSettings = new(renderPassEvent);

    //         shaderTagIds = new List<ShaderTagId>()
    //         {
    //             new ShaderTagId("UniversalForward"),
    //             new ShaderTagId("UniversalForwardOnly"),
    //             new ShaderTagId("LightweightForward"),
    //             new ShaderTagId("SRPDefaultUnlit")  
    //         };
    //     }

    //     [Obsolete]
    //     // This is called before the render pass.
    //     public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    //     {

    //         RenderTextureDescriptor normalsTextureDescriptor = cameraTextureDescriptor;

    //         normalsTextureDescriptor.colorFormat = viewSpaceNormalsTextureSettings.colorFormat;
    //         normalsTextureDescriptor.depthBufferBits = viewSpaceNormalsTextureSettings.depthBufferBits;

    //         // create a temprorary render texture and set it up as a global shader property.

    //         cmd.GetTemporaryRT(normals.id, normalsTextureDescriptor, FilterMode.Point);

    //         // add this as a render target for this pass.

    //         ConfigureTarget(normals.Identifier());


    //         // clear the render texture with the specified background color.
    //         // because even though the render texture is released in the on camera clean up method;
    //         // the get temprorary rt method can still return a chached render texture.

    //         ConfigureClear(ClearFlag.All, viewSpaceNormalsTextureSettings.backgroundColor); 
    //     }

    //     [Obsolete]
    //     // this executes the render pass.
    //     public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    //     {
    //         CommandBuffer cmd = CommandBufferPool.Get();
    //         using(
    //             new ProfilingScope(cmd, new ProfilingSampler("SceneViewSpaceNormalsTextureCreation"))
    //         )
    //         {
    //             cmd.Clear();
    //             DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagIds, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
    //             FilteringSettings filteringSettings = FilteringSettings.defaultValue;
    //             context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
    //         }

    //         context.ExecuteCommandBuffer(cmd);
    //         CommandBufferPool.Release(cmd);
    //     }

    //     // this is called when the camera has finished rendering.

    //     public override void OnCameraCleanup(CommandBuffer cmd)
    //     {
    //         base.OnCameraCleanup(cmd);
    //     }
    // }

private class ViewSpaceNormalsTexturePass : ScriptableRenderPass
{
    private RTHandle normals;
    private readonly ViewSpaceNormalsTextureSettings settings;
    private readonly List<ShaderTagId> shaderTagIds;

    public ViewSpaceNormalsTexturePass(RenderPassEvent evt)
    {
        renderPassEvent = evt;
        settings = new ViewSpaceNormalsTextureSettings(evt);

        shaderTagIds = new List<ShaderTagId>()
        {
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("LightweightForward"),
            new ShaderTagId("SRPDefaultUnlit")
        };
    }

    [Obsolete]
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = settings.depthBufferBits;
        desc.colorFormat     = settings.colorFormat;

        RenderingUtils.ReAllocateIfNeeded(ref normals, desc, FilterMode.Point, name: "_SceneNormalsTexture");

        ConfigureTarget(normals);  
        ConfigureClear(ClearFlag.All, settings.backgroundColor);
    }

    [Obsolete]
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        using (new ProfilingScope(cmd, new ProfilingSampler("SceneViewSpaceNormalsTextureCreation")))
        {
            var drawingSettings = CreateDrawingSettings(shaderTagIds, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            var filteringSettings = FilteringSettings.defaultValue;

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        // DO NOT release RTHandles here — they persist automatically.
    }


    [Obsolete]
    public override void OnFinishCameraStackRendering(CommandBuffer cmd)
    {
        normals.Release();
    }
}


    private class ScreenSpaceOutlinePass : ScriptableRenderPass
    {
        [SerializeField] private ViewSpaceNormalsTextureSettings viewSpaceNormalsTexureSettings;
    }

    [SerializeField] private ViewSpaceNormalsTexturePass viewSpaceNormalsTexturePass;
    [SerializeField] private ScreenSpaceOutlinePass screenSpaceOutlinesPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Enqueue the renderer passes into the render pipeline.
        // Note:
        //  the normal's pass (for generating a normal texture) is before the screen space outline pass;
        //  as the outline pass needs a generated normals texture (Not supplied by URP Assets.) and depth texture (supplied by URP Assets).

        renderer.EnqueuePass(viewSpaceNormalsTexturePass);
        // renderer.EnqueuePass(screenSpaceOutlinesPass);
    }

    public override void Create()
    {
        viewSpaceNormalsTexturePass = new(renderPassEvent);
    }

}
