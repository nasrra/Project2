using System;
using System.Collections.Generic;
using Entropek.Collections;
using Entropek.Physics;
using Entropek.UnityUtils.Attributes;
using Unity.VisualScripting;
using UnityEngine;

namespace Entropek.Physics
{
    public class CharacterControllerMovement : MonoBehaviour
    {

        // forces applied overtime (automatic).
        private SwapbackList<LinearForceVelocity> linearForceVelocities = new(); 
        private SwapbackList<DynamicForceVelocity> dynamicForceVelocities = new(); 

        [Header(nameof(CharacterController)+" Components")]
        [SerializeField] protected CharacterController controller;
        public CharacterController CharacterController => controller;
        [SerializeField] protected GroundCheck groundChecker;

        [Header("Data")]
        [RuntimeField] protected Vector3 gravityVelocity;            // gravity.
        [RuntimeField] protected Vector3 moveDirectionVelocity;      // desired move direction by entity.
        [RuntimeField] protected Vector3 totalForceVelocity;

        [RuntimeField] public Vector3 moveDirection;

        [SerializeField] private float acceleration;
        public float Acceleration => acceleration;

        [SerializeField] private float deceleration;
        public float Deceleration => deceleration;

        [SerializeField] private float maxSpeed;
        public float MaxSpeed => maxSpeed;

        [SerializeField] private float gravityModifier;
        protected const float GravityForce = -9.81f;
        protected float gravity;

        private float stickToFloorForce;

        [SerializeField] public bool UseGravity;
        public bool SnapToGround = true;


        /// 
        /// Base.
        /// 


        protected virtual void Awake()
        {
            UpdateGravity();
            stickToFloorForce = -maxSpeed;
        }

        protected virtual void Update()
        {
            UpdateTick();
        }

        protected void UpdateTick()
        {            
            totalForceVelocity = ForceVelocityTick();
            HandleMoveDirection();
            HandleGravity();
            Vector3 velocity = moveDirectionVelocity + gravityVelocity + totalForceVelocity;
            controller.Move(velocity * UnityEngine.Time.deltaTime);
        }

        /// 
        /// Functions.
        /// 


        public void SetMaxSpeed(float value)
        {
            maxSpeed = value;
            // set stick to floor force so this doesnt fly off of slopes.
            stickToFloorForce = -maxSpeed;            
        }

        public void SetAcceleration(float value)
        {
            acceleration = value;
        }

        public void SetDecleration(float value)
        {
            deceleration = value;
        }

        public void HaltMoveDirectionVelocity()
        {
            moveDirectionVelocity = Vector3.zero;
        }

        public void ClearGravityVelocity()
        {
            gravityVelocity = Vector3.zero;
        }

        private void HandleMoveDirection()
        {
            Vector3 targetVelocity;
            float rate;

            if (moveDirection.sqrMagnitude > 0)
            {

                // Accelerate if we want to move somewhere.

                targetVelocity = moveDirection.normalized * maxSpeed;
                rate = acceleration;
            }
            else
            {

                // Decelerate if not.

                targetVelocity = Vector3.zero;
                rate = deceleration;
            }

            moveDirectionVelocity = Vector3.MoveTowards(moveDirectionVelocity, targetVelocity, rate * UnityEngine.Time.deltaTime);
        }

        private void HandleGravity()
        {
            if (UseGravity == false)
            {
                return;
            }

            if (SnapToGround == true && groundChecker.IsGrounded == true && totalForceVelocity.sqrMagnitude == 0)
            {

                // snap to floor when grounded and no forces are being applied to us.

                gravityVelocity.y = stickToFloorForce;
            }
            else if (groundChecker.WasGroundedLastTick == true)
            {

                // unsnap from floor when we were grounded last frame and,
                // currently not grounded or a force acting upon us.
                // So the force isnt cancelled out immediately by ground checks.

                gravityVelocity.y = 0;
            }
            else
            {

                // apply gravity if we are not grounded.

                gravityVelocity.y += gravity * UnityEngine.Time.deltaTime;
            }
        }

        private Vector3 ForceVelocityTick()
        {

            Vector3 velocity = Vector3.zero;

            for(int i = 0; i < linearForceVelocities.Count; i++){ // swap this for backwards.

                // get current force.

                LinearForceVelocity currentVelocity = linearForceVelocities[i]; 

                // decay the current force.            

                Vector3 decayedVelocity = Vector3.MoveTowards(linearForceVelocities[i].Velocity, Vector3.zero, linearForceVelocities[i].DecaySpeed * UnityEngine.Time.deltaTime);
                if(decayedVelocity.sqrMagnitude > 0){
                    
                    // if the force has not completely decayed, add the decayed force.
                    linearForceVelocities.Add(new LinearForceVelocity(decayedVelocity, currentVelocity.DecaySpeed));
                }

                // remove the current force.

                linearForceVelocities.RemoveAt(i);                    
                
                // add the fore to velocity.
                
                velocity += currentVelocity.Velocity;
            }

            for(int i = 0; i < dynamicForceVelocities.Count; i++)
            {

                DynamicForceVelocity currentVelocity = dynamicForceVelocities[i];
                currentVelocity.IncrementElapsedTime(UnityEngine.Time.deltaTime);
                
                // remove if the force has completed.
                
                if (currentVelocity.IsCompleted())
                {
                    dynamicForceVelocities.RemoveAt(i);
                }
                else
                {
                    // reassign with updated values (as DynamicForceVelocity is not accessbile via ref).

                    dynamicForceVelocities[i] = currentVelocity;
                }

                // apply the force velocity.

                velocity += currentVelocity.GetVelocity();
            }


            // floor the velocity when we hit the ground for the first time.
            // This stops the player from flying off or bouncing of the ground when a force is applied.

            if (groundChecker.IsGrounded == true && groundChecker.WasGroundedLastTick == false)
            {
                velocity.y = 0;
            }

            return velocity;
        }

        public void SetGravityModifier(float gravityModifier)
        {
            this.gravityModifier = gravityModifier;
            UpdateGravity();
        }

        private void UpdateGravity()
        {
            gravity = GravityForce * gravityModifier;
        }

    
        ///
        /// Dynamic Force Handling.
        /// 


        public void Impulse(Vector3 direction, AnimationCurve force, float maxTime)
        {
            dynamicForceVelocities.Add(new DynamicForceVelocity(direction, force, maxTime));
        }

        public void Impulse(Vector3 direction, AnimationCurve force)
        {
            Impulse(direction, force, force.keys[force.keys.Length-1].time);
        }

        public void ImpulseRelative(Vector3 direction, Vector3 upAxis, AnimationCurve force)
        {
            direction = Vector3.ProjectOnPlane(direction, upAxis).normalized;
            Impulse(direction, force);
        }

        public void ImpulseRelativeToGround(Vector3 direction, AnimationCurve force)
        {
            // only impulse relative to ground if implemented.
            // apply a regular impulse if not.
            // this is done to ensure compatiblility of all for impulse calls across any entity.
            // the ground check may not even be grounded (no ground normal to project onto) so the force would be impulsed normally anyways.

            if(groundChecker!=null && groundChecker.IsGrounded == true){
                ImpulseRelative(direction, groundChecker.GroundNormal, force);
            }
            else{
                Impulse(direction, force);
            }            
        }


        /// 
        /// Linear Force Handling.
        /// 


        public void Impulse(Vector3 direction, float force, float decaySpeed)
        {
            linearForceVelocities.Add(new LinearForceVelocity(direction * force, decaySpeed));
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
    }
    
}