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
        [SerializeField] private GroundChecker groundChecker;

        [Header("Data")]
        private Vector3 initialJumpVelocity;
        private Vector3 jumpVelocity;

        [SerializeField] private float jumpSpeed;
        public float JumpSpeed => jumpSpeed;

        [SerializeField] private float jumpDecay;
        public float JumpDecay => jumpDecay;

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
            if (groundChecker.IsGrounded == true)
            {
                jumpVelocity = initialJumpVelocity;
            }
        }

        public void HandleJumping()
        {
            if (isJumping == true && jumpVelocity.sqrMagnitude > 0)
            {
                jumpVelocity = Vector3.MoveTowards(jumpVelocity, Vector3.zero, jumpDecay * Time.deltaTime);
                controller.Move(jumpVelocity * Time.deltaTime);
                movement.SnapToGround = false;
            }
            else
            {
                movement.SnapToGround = true;
            }
        }

        public void StopJumping()
        {
            jumpVelocity = Vector3.zero;
            isJumping = false;
        }
    }

}

