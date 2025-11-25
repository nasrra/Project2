#ifndef SPLAT_MAPPER_INCLUDED 


#define SPLAT_MAPPER_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"

    // returns a similarirt factor (between 0 - 1) for two colours.

    // float GetColourSimilarity(float4 colorA, float4 colorB)
    // {
    //     return 1 - saturate(
    //         abs(colorA.r - colorB.r) +
    //         abs(colorA.g - colorB.g) +
    //         abs(colorA.b - colorB.b)
    //     );
    // }

float GetColourSimilarity(float4 colorA, float4 colorB)
{
    float3 diff = colorA.rgb - colorB.rgb;

    // Euclidean distance between two colors (0–√3 range)
    float dist = length(diff);

    // Normalize distance to 0–1 range and invert for similarity
    // sqrt(3) ≈ 1.732 (max distance between two RGB colors)
    return 1.0 - saturate(dist / 1.7320508);
}

    void EntryPoint_float(
    
        UnityTexture2D splatMap,
        UnitySamplerState splatMapSampler,

        UnityTexture2DArray textures,
        UnitySamplerState textureSampler, 
        
        UnityTexture2DArray splatMapToTextureColorIds, 
        UnitySamplerState colorIdSampler, 
        
        float2 uv,

        float textureCount,

        float uvScale,

        out float3 Color
    ){
        
        #if defined(SHADERGRAPH_PREVIEW)  
        
            return float3(0.5,0.5,0.5);
        
        #else

        Color = textures.Sample(splatMapSampler, float3(uv * uvScale, 0)).rgb;

        // get the pixel color on the splat map texture.

        float4 splatMapColor =  SAMPLE_TEXTURE2D(splatMap, splatMapSampler, uv).rgba;

        int count = (int)textureCount;

        for(int i = 0; i < count; i++){
                        
            // get the decal textures color to apply to the splat map (eg, grass, bricks, stone, etc).

            float4 textureColor = textures.Sample(splatMapSampler, float3(uv * uvScale, i));

            // get the color id for the texture.

            float4 textureColorId = splatMapToTextureColorIds.Sample(colorIdSampler, float3(uv * uvScale, i));

            // apply the texture color based on how similar the actual splat map color is 
            // compared to this textures splat map color id. 

            Color += textureColor * GetColourSimilarity(splatMapColor, textureColorId);
        }


        #endif
    }

    


#endif