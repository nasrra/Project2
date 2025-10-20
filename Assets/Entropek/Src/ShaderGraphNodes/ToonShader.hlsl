
// References:
//  (Cel Shading - Devlog 3 by Robin Seibold) https://www.youtube.com/watch?v=gw31oF9qITw
//  (A Short Hike Post Mortem) https://www.youtube.com/watch?v=ZW8gWgpptI8&t=434s


// header guard to ensure this is only compiled once when using multiple includes.


#ifndef LIGHTING_CEL_SHADED_INCLUDED 


    #define LIGHTING_CEL_SHADED_INCLUDED


    // ensure that everything witin this block is only 
    // invluded in actual shader compilation for game or runtime.
    // Avoiding compilation errors and unnecessary complexity in the
    // shader preview, provindg a lightweight and stabel preview expereience
    // within the edtior. 

    
    #ifndef SHADERGRAPH_PREVIEW

    struct SurfaceVariables{
        float3 normal;      // the noramlised normal direction of a given point on the mesh.
        float3 view;        // the direction to the camera from this point.
        float specularThreshold;  
        float specularStrength;
        float shininess;   
        float rimLightThreshold;
        float rimLightStrength;
        float stepRamp; 
        float shadowLight;
        float fullLight;
    };
    
    #endif
    
    
    float ApplyStepRamp(SurfaceVariables surface,float value){
        return lerp(surface.shadowLight, 1,floor(value * surface.stepRamp + surface.fullLight) / surface.stepRamp);
    }
    
    float CalculateDiffuse(Light light, SurfaceVariables surface){

        // the diffuse value for a point on a surface is
        // the dot product of the Normal and the direction from that
        // Normal's point to the light source.

        // dot(surface.normal, light.direction)
        //  This dot product can be potentially less than 0
        //  (light coming from behind the surface)
        // or greater than 1 (in some cases) due to floating point in-precision.
    
        // saturate()
        //  clamps the result to be between 0 (full shadow) and 1 (full light)
        //  ensuring a valid lighting range.
        
        float diffuse = saturate(dot(surface.normal, light.direction)); // base shaded diffuse.

        return diffuse;
    }

    float CalculateSpecular(Light light, SurfaceVariables surface, float diffuse){

        // In the Phong reflection model.
        // the reflection vector is given by the light direction and normal vectors.
        // The specular term is calculated by the dot product between this reflection
        // vector and the direction vector to the viewer (camera).
    
        // (use blinn-phong instead because of its better visuals, reduced artifacting, and computational efficiency).
    
        // In the Blinn-Phong reflection model.
        // instead of the reflection vector, a halfway vector is calculated
        // by adding the light direction and view direction vectors; noramlising the result.
        // the specular term is then equal to the dot product between the normal vector and the halfway vector;
        // powered by the shininess value.
    
        
        float3 halfwayVector = SafeNormalize(light.direction + surface.view);
        float specular = saturate(dot(surface.normal, halfwayVector)); 
        
        specular = pow(specular, surface.shininess);
        specular *= diffuse * surface.specularThreshold; // to avoid the specular highlight from going over non-illuminated parts.

        // this works for some reason, dont know why lol.
        
        specular = surface.specularThreshold * smoothstep(
            (1 - surface.specularThreshold) * 0,
            0,
            specular
        );

        return specular * surface.specularStrength;
    }

    float CalculateRimLight(Light light, SurfaceVariables surface, float diffuse){

        // The bigger on an angle between the view an normal,
        // the lighter the surface will appear.

        float rimLight = (1 - dot(surface.view, surface.normal)) * surface.rimLightStrength;
        rimLight *= pow(diffuse, surface.rimLightThreshold); // add or remove the rim lighting effect depending upon the threshold.    
        return rimLight;
    }

    float4 CalculateShadowCoordinates(float3 worldPosition){
        float4 shadowCoord;
        #if SHADOWS_SCREEN
            float4 clipPos      = TransformWorldToHClip(worldPosition);
            shadowCoord  = ComputeScreenPos(clipPos); 
        #else
            shadowCoord = TransformWorldToShadowCoord(worldPosition);
        #endif

        return shadowCoord;
    }

    float3 CalculateShading(Light light, SurfaceVariables surface){

        // keep between 0 - 1 (inclusive) to avoid lighting issues with negatives and values greater than 1.

        float lightAttenuation      = saturate(light.shadowAttenuation); 
        float distanceAttenuation   = saturate(light.distanceAttenuation);
        
        
        float diffuse               = CalculateDiffuse(light, surface);

        // apply the shadow lighting attenuation.

        diffuse *= lightAttenuation; 

        //
        // Calculate Specular and Rim Light outside of base diffuse for more control over the look of the terms.
        // This is not "physically" accurate to the real world but who cares.. 
        //

        float specular = CalculateSpecular(light, surface, diffuse) * distanceAttenuation * lightAttenuation;
        float rimLight = CalculateRimLight(light, surface, diffuse) * distanceAttenuation * lightAttenuation;

        // Apply the stylised toon ramp to the end result.

        diffuse = ApplyStepRamp(surface, diffuse);
        
        // At the very end apply the light's distance attenuation to have a smooth
        // transition from being unlit into lit.

        diffuse *= distanceAttenuation;
        
        // default real time shading model (including rim light).

        return light.color * (diffuse + max(specular, rimLight));
    }

    
    // entry point function,
    // used as the Name paramter for a custom Shader Graph Node.
    
    
    void LightingCelShaded_float(
        float3 WorldPosition, 
        float3 WorldNormal, 
        float3 ViewDirection, 
        float SpecularThreshold,
        float SpecularStrength, 
        float RimLightThreshold, 
        float RimLightStrength, 
        float StepRamp, 
        float ShadowLight, 
        float FullLight,
        out float3 Color
    ){
        
        #if defined(SHADERGRAPH_PREVIEW)  
        
            // nothing...
        
        #else
        
            SurfaceVariables surface;
            surface.normal              = normalize(WorldNormal);           // normalise world normal for correct light intensity values.
            surface.view                = SafeNormalize(ViewDirection);     // use Safe Noramlise for tolerance checks against divide by zero or small magnitudes.
            surface.specularThreshold   = SpecularThreshold;
            surface.specularStrength    = SpecularStrength;
            surface.shininess           = exp2(10 * SpecularThreshold + 1);        // shiniess constant according to the Phong reflection model.
            surface.rimLightStrength    = RimLightStrength;
            surface.rimLightThreshold   = RimLightThreshold;
            surface.stepRamp            = StepRamp;
            surface.shadowLight         = ShadowLight;
            surface.fullLight           = FullLight;

            float4 shadowCoord  = CalculateShadowCoordinates(WorldPosition);
            Light light         = GetMainLight(shadowCoord);
            
            Color = CalculateShading(light, surface);

            // Add additional lighting (point, directional, spot lights, etc).
            // NOTE:
            //  To add additional lighting, renderer must be set to Forward not Forward+ on URP Renderer Asset.
            //  Forward+ additional lights are managed in a way that does not expose the count and data to custom fragment
            //  shaders as with Forward rendering. Forward+ always returns zero.

            int pixelLightCount = GetAdditionalLightsCount();
            for(int i = 0; i < pixelLightCount; i++){

                // GetAdditionalLight(i, WorldPosition, 1);
                // NOTE:
                //  The reason for the 1 in this function call is because the shadow attenuation
                //  for additional lights is not set unless the function with the shadow mask parameter
                //  (in this cast mask = 1) is called.
                //  Since only realtime lighting, no light or shadow maps are taken into account during this stage
                //  of development, this parameter is redundant and can be set to anything.

                light = GetAdditionalLight(i, WorldPosition, 1);
                Color += CalculateShading(light, surface);
            }

        #endif
    }


#endif