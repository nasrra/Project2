using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Physics
{

    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CharacterControllerMovement))]
    public class JumpMovement : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CharacterController controller;
        [SerializeField] private CharacterControllerMovement movement;

        [Header("Data")]
        [RuntimeField] private Vector3 initialJumpVelocity;
        [RuntimeField] private Vector3 jumpVelocity;

        [SerializeField] private float jumpSpeed;
        public float JumpSpeed => jumpSpeed;

        [SerializeField] private float jumpDecay;
        public float JumpDecay => jumpDecay;

        [SerializeField] private float jumpCancelDecay;
        public float JumpCancelDecay => jumpCancelDecay;

        [SerializeField] private float gravityThresholdSqrd;
        public float GravityThresholdSqrd => gravityThresholdSqrd; 

        private bool isJumping = false;

        private void Awake()
        {
            UpdateInitialJumpVelocity();
        }

        private void Update()
        {
            HandleJumping();
        }

        public void SetJumpSpeed(float jumpSpeed)
        {
            this.jumpSpeed = jumpSpeed;
            UpdateInitialJumpVelocity();
        }

        public void SetJumpDecay(float jumpDecay)
        {
            this.jumpDecay = jumpDecay;
        }

        public void UpdateInitialJumpVelocity()
        {
            initialJumpVelocity = jumpSpeed * Vector3.up;
        }

        public void StartJumping()
        {
            isJumping = true;
            jumpVelocity = initialJumpVelocity;
        }

        public void HandleJumping()
        {
            if (isJumping == true)
            {
                if(jumpVelocity.sqrMagnitude > 0)
                {
                    jumpVelocity = Vector3.MoveTowards(jumpVelocity, Vector3.zero, jumpDecay * UnityEngine.Time.deltaTime);
                    controller.Move(jumpVelocity * UnityEngine.Time.deltaTime);
                    movement.SnapToGround = false;                    
                }
                
                if(jumpVelocity.sqrMagnitude > GravityThresholdSqrd)
                {
                    movement.ClearGravityVelocity();
                }
            }
            else
            {
                jumpVelocity = Vector3.MoveTowards(jumpVelocity, Vector3.zero, jumpCancelDecay * UnityEngine.Time.deltaTime);
                controller.Move(jumpVelocity * UnityEngine.Time.deltaTime);
                movement.SnapToGround = true;
            }
        }

        public void StopJumping()
        {
            isJumping = false;
        }
    }

}

