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

        [Header(nameof(CharacterController)+" Components")]
        [SerializeField] protected CharacterController controller;
        [SerializeField] protected GroundCheck groundChecker;
        [SerializeField] protected ForceApplier forceApplier;

        [Header("Data")]
        protected Vector3 gravityVelocity;            // gravity.
        protected Vector3 moveDirectionVelocity;      // desired move direction by entity.
        protected Vector3 totalForceVelocity;

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
            totalForceVelocity = GetTotalForceVelocity();
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

        private Vector3 GetTotalForceVelocity()
        {

            Vector3 totalForceVelocity = forceApplier.GetTotalForceVelocity();

            // floor the velocity when we hit the ground for the first time.
            // This stops the player from flying off or bouncing of the ground when a force is applied.

            if (groundChecker.IsGrounded == true && groundChecker.WasGroundedLastTick == false)
            {
                totalForceVelocity.y = 0;
            }

            return totalForceVelocity;
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

    }
    
}