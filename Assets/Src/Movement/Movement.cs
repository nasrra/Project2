using System;
using System.Collections.Generic;
using Entropek.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Movement : MonoBehaviour{


    [Header("Components")]
    [SerializeField] private CharacterController controller;


    [Header("Data")]
    private SwapbackList<ForceVelocity> forceVelocities = new SwapbackList<ForceVelocity>(); // forces applied overtime (automatic).
    
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

    private float stickToFloorForce;

    [SerializeField] private LayerMask groundLayers;

    [SerializeField] private bool useGravity;
    public bool SnapToGround = true;
    public bool IsGrounded {get;private set;}
    private bool IsGroundedLastFrame;

    #if UNITY_EDITOR
    [Header("Editor Tools")]
    [SerializeField] private bool showGroundCheck = false;
    #endif


    /// 
    /// Base.
    /// 


    void Awake(){
        UpdateGravity();
        stickToFloorForce=-maxSpeed;
    }

    void FixedUpdate(){
        CheckGrounded();
    }

    void Update(){
        HandleMoveDirection();
        HandleGravity();
        Vector3 velocity = moveDirectionVelocity + gravityVelocity + GetTotalForceVelocity();
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

        if(SnapToGround == true && IsGrounded == true && forceVelocities.Count == 0){

            // snap to floor when grounded and no forces are being applied to us.

            gravityVelocity.y = stickToFloorForce;
        }
        else if(IsGroundedLastFrame == true){

            // unsnap from floor when we were grounded last frame and,
            // currently not grounded or a force acting upon us.
            // So the force isnt cancelled out immediately by ground checks.

            gravityVelocity.y = 0;
        }
        else{
            
            // apply gravity if we are not grounded.

            gravityVelocity.y += gravity * Time.deltaTime; 
        }
    }

    public void Impulse(Vector3 direction, float force, float decaySpeed){
        Vector3 velocity = direction * force;
        if(IsGrounded==true){
            velocity += Vector3.up * maxSpeed;
        }
        forceVelocities.Add(new ForceVelocity(velocity, decaySpeed));
    }

    private Vector3 GetTotalForceVelocity(){
        
        Vector3 totalForceVelocity = Vector3.zero;
         
        for(int i = 0; i < forceVelocities.Count; i++){
            
            // get current force.

            ForceVelocity currentVelocity = forceVelocities[i]; 

            // floor force when returning to ground.

            if(IsGrounded==true & IsGroundedLastFrame == false){
                currentVelocity.Velocity.y = 0;
            }

            // apply the current force.

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
        
        return totalForceVelocity;
    }

    public void SetGravityModifier(float gravityModifier){
        this.gravityModifier = gravityModifier;
        UpdateGravity();
    }

    private void UpdateGravity(){
        gravity = -9.81f * gravityModifier;
    }

    private void CheckGrounded(){
        
        // check a sphere at the bottom of the character controller at a radius of the character controller.
        // check if there is ground there or not.

        float radius = controller.radius; 
        Vector3 controllerPosition = controller.transform.position;
        Vector3 spherePosition = new Vector3(controllerPosition.x, controllerPosition.y - (controller.height * 0.6f) + radius, controllerPosition.z);
        IsGroundedLastFrame = IsGrounded;
        IsGrounded = Physics.CheckSphere(spherePosition, radius, groundLayers, QueryTriggerInteraction.Ignore); // ignore trigger colliders.
    }

    #if UNITY_EDITOR

    void OnDrawGizmos(){
        if(showGroundCheck==true){
            Gizmos.color = Color.green;
            float radius = controller.radius; 
            Vector3 controllerPosition = controller.transform.position;
            Vector3 spherePosition = new Vector3(controllerPosition.x, controllerPosition.y - (controller.height * 0.5f) + radius, controllerPosition.z);
            Gizmos.DrawSphere(spherePosition, radius);
        }
    }

    #endif
}
