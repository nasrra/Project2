
// NOTE:
//  This is a light ramp shader graph node based on the game "A Short Hike"'s stylised lighting ramp. 

// References:
//  (A Short Hike Post Mortem) https://www.youtube.com/watch?v=ZW8gWgpptI8&t=434s


#ifndef ASH_LIGHT_RAMP_INCLUDED


    #define ASH_LIGHT_RAMP_INCLUDED
    
    
    void ApplyLightRamp_float(float StepRamp, float ShadowLight, float FullLight, float InputValue, out float RampedValue){
        RampedValue = lerp(ShadowLight, 1, ceil(InputValue * StepRamp + FullLight) / StepRamp);
    }


#endif