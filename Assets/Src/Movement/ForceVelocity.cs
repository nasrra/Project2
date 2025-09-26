using UnityEngine;

public struct ForceVelocity{
    public Vector3 Velocity;
    public float DecaySpeed {get;private set;}

    public ForceVelocity(Vector3 direction, float force, float decaySpeed){
        Velocity    = direction * force;
        DecaySpeed  = decaySpeed;
    }
    
    public ForceVelocity(Vector3 velocity, float decaySpeed){
        Velocity    = velocity;
        DecaySpeed  = decaySpeed;
    }
}
