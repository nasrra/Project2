using System;
using System.Collections.Generic;
using Entropek.Collections;
using UnityEngine;

public class Movement : MonoBehaviour{


    [Header("Components")]
    [SerializeField] private CharacterController controller;


    [Header("Data")]
    private SwapbackList<ForceVelocity> forceVelocities = new SwapbackList<ForceVelocity>(); // forces applied overtime (automatic).
    private List<Vector3> oneFrameVelocities = new List<Vector3>(); // forces applied for one frame (manual).
    
    private Vector3 gravityVelocity;            // gravity.
    private Vector3 moveDirectionVelocity;      // desired move direction by entity.
    
    [HideInInspector] public Vector3 moveDirection;    

    [SerializeField] private float acceleration;
    public float Acceleration => acceleration;
    
    [SerializeField] private float deceleration;
    public float Deceleration => deceleration;
    
    [SerializeField] private float maxSpeed;
    public float MaxSpeed => maxSpeed;

    [SerializeField] private float gravityModifier;
    private float gravity;

    [SerializeField] private float jumpSpeed;
    public float JumpSpeed => jumpSpeed;
    Vector3 initialJumpVelocity;

    [SerializeField] private float jumpDecay;
    public float JumpDecay => jumpDecay;

    
    [SerializeField] private bool useGravity;


    /// 
    /// Base.
    /// 


    void Awake(){
        UpdateGravity();
        // UpdateInitialJumpVelocity();
    }

    void Update(){
        HandleMoveDirection();
        HandleGravity();
        Vector3 velocity = moveDirectionVelocity + gravityVelocity + GetTotalForceVelocity() + GetTotalOneFrameVelocity();
        controller.Move(velocity * Time.deltaTime);
    }


    /// 
    /// Functions.
    /// 


    private void HandleMoveDirection(){
        Vector3 targetVelocity  = Vector3.zero; 
        float rate              = 0;

        if(moveDirection.sqrMagnitude > 0){
            
            // Accelerate if we want to move somewhere.
            
            targetVelocity = moveDirection.normalized * maxSpeed;
            rate = acceleration;
        }
        else{

            // Decelerate if not.

            targetVelocity = Vector3.zero;
            rate = deceleration;
        }

        moveDirectionVelocity = Vector3.MoveTowards(moveDirectionVelocity, targetVelocity, rate * Time.deltaTime);
    }

    private void HandleGravity(){
        if(useGravity==false){
            return;
        }

        if(controller.isGrounded == true){
            gravityVelocity.y = 0;
        }
        else{
            gravityVelocity.y += gravity * Time.deltaTime; 
        }
    }

    public void Impulse(Vector3 direction, float force, float decaySpeed){
        forceVelocities.Add(new ForceVelocity(direction, force, decaySpeed));
    }

    private Vector3 GetTotalForceVelocity(){
        
        Vector3 totalForceVelocity = Vector3.zero;
        int count = forceVelocities.Count;

        if(count > 0){
         
            for(int i = 0; i < count; i++){
                
                // apply the current force.
                
                ForceVelocity currentVelocity = forceVelocities[i]; 
                totalForceVelocity += currentVelocity.Velocity;


                // decay the current force.            

                Vector3 decayedVelocity = Vector3.MoveTowards(forceVelocities[i].Velocity, Vector3.zero, forceVelocities[i].DecaySpeed * Time.deltaTime);
                if(decayedVelocity.sqrMagnitude > 0){
                    
                    // if the force has not completely decayed, add the decayed force.
                    forceVelocities.Add(new ForceVelocity(decayedVelocity, currentVelocity.DecaySpeed));
                }

                // remove the current force.

                forceVelocities.RemoveAt(i);
            }
        
        }

        return totalForceVelocity;
    }

    public void SetGravityModifier(float gravityModifier){
        this.gravityModifier = gravityModifier;
        UpdateGravity();
    }

    private void UpdateGravity(){
        gravity = -9.81f * gravityModifier;
    }

    public void AddOneFrameVelocity(Vector3 velocity){
        oneFrameVelocities.Add(velocity);
    }

    private Vector3 GetTotalOneFrameVelocity(){
        
        Vector3 totalVelocity = Vector3.zero;
        
        int count = oneFrameVelocities.Count;
        if(count > 0){
            for(int i = 0; i < count; i++){
                totalVelocity+=oneFrameVelocities[i];
            }
            
            oneFrameVelocities.Clear();
        }
        
        return totalVelocity;
    }
}
