using Entropek.Physics;
using Entropek.Collections;
using UnityEngine;


namespace Entropek.Physics{


public class ForceApplier : MonoBehaviour{
    
    private readonly SwapbackList<ForceVelocity> forceVelocities = new(); // forces applied overtime (automatic).
    [Header("Optional Components")]
    private GroundCheck groundChecker;

    private void LateUpdate(){
        DecayForces();
    }

    public void Impulse(Vector3 direction, float force, float decaySpeed){
        forceVelocities.Add(new ForceVelocity(direction * force, decaySpeed));
    }

    public void ImpulseRelative(Vector3 direction, Vector3 upAxis, float force, float decaySpeed){
        direction = Vector3.ProjectOnPlane(direction, upAxis).normalized;
        Impulse(direction, force, decaySpeed);
    }

    public void ImpulseRelativeToGround(Vector3 direction, float force, float decaySpeed){
        
        // only impulse relative to ground if implemented.
        // apply a regular impulse if not.
        // this is done to ensure compatiblility of all for impulse calls across any entity.
        // the ground check may not even be grounded (no ground normal to project onto) so the force would be impulsed normally anyways.

        if(groundChecker!=null && groundChecker.IsGrounded == true){
            ImpulseRelative(direction, groundChecker.GroundNormal, force, decaySpeed);
        }
        else{
            Impulse(direction, force, decaySpeed);
        }
    }

    public Vector3 GetTotalForceVelocity(){
        Vector3 velocity = Vector3.zero;
        for(int i = 0; i < forceVelocities.Count; i++){
            velocity += forceVelocities[i].Velocity;
        }
        return velocity;
    }

    private void DecayForces(){
        for(int i = 0; i < forceVelocities.Count; i++){

            // get current force.

            ForceVelocity currentVelocity = forceVelocities[i]; 

            // decay the current force.            

            Vector3 decayedVelocity = Vector3.MoveTowards(forceVelocities[i].Velocity, Vector3.zero, forceVelocities[i].DecaySpeed * UnityEngine.Time.deltaTime);
            if(decayedVelocity.sqrMagnitude > 0){
                
                // if the force has not completely decayed, add the decayed force.
                forceVelocities.Add(new ForceVelocity(decayedVelocity, currentVelocity.DecaySpeed));
            }

            // remove the current force.

            forceVelocities.RemoveAt(i);
        }
    }
}
    
}
