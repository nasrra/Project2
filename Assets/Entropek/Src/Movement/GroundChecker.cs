using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Entropek.Physics{

    public class GroundChecker : MonoBehaviour{
        public event Action Airborne;
        public event Action Grounded;
        public Vector3 GroundNormal {get; private set;}
        public Vector3 CheckStartPostion = Vector3.zero;
        public float CheckRadius = 0.5f;
        public float CheckLength = 0.1f; 

        [Header("Data")]
        [SerializeField] private LayerMask groundLayers;

        #if UNITY_EDITOR
        [Header("Editor Tools")]
        [SerializeField] private bool showGroundCheck = false;
        #endif

        public bool IsGrounded {get;private set;}
        public bool WasGroundedLastTick {get;private set;}

        private void FixedUpdate(){
            // check a sphere at the bottom of the character controller at a radius of the character controller.
            // check if there is ground there or not.

            // float radius = controller.radius; 
            // Vector3 controllerPosition = controller.transform.position;

            // // start at just above the very bottom of the character controller collider.
            
            // Vector3 startPosition = new(controllerPosition.x, controllerPosition.y - (controller.height * 0.495f) + radius, controllerPosition.z);

            // set last frame data. 

            WasGroundedLastTick = IsGrounded;
            
            if(UnityEngine.Physics.SphereCast(transform.position + CheckStartPostion, CheckRadius, Vector3.down, out RaycastHit hit, CheckLength, groundLayers, QueryTriggerInteraction.Ignore)==true){
                IsGrounded = true;
                GroundNormal = hit.normal;
                if(WasGroundedLastTick == false)
                {
                    Grounded?.Invoke();
                }
            }
            else{
                IsGrounded = false;
                GroundNormal = Vector3.up;
                if (WasGroundedLastTick == true)
                {
                    Airborne?.Invoke();
                }
            }
        }

        #if UNITY_EDITOR

        void OnDrawGizmos()
        {
            if (!showGroundCheck) return;


            Vector3 origin = transform.position + CheckStartPostion;
            Vector3 end = origin + Vector3.down * CheckLength;

            // If the ground is within or above the red shere (origin sphere area) this is not grounded.
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin, CheckRadius);

            // If the ground is anywhere below the sphere, this is grounded.

            // Draw line between spheres
            
            Gizmos.color = Color.white;
            Gizmos.DrawLine(origin + Vector3.left * CheckRadius, end + Vector3.left * CheckRadius);
            Gizmos.DrawLine(origin + Vector3.right * CheckRadius, end + Vector3.right * CheckRadius);
            Gizmos.DrawLine(origin + Vector3.forward * CheckRadius, end + Vector3.forward * CheckRadius);
            Gizmos.DrawLine(origin + Vector3.back * CheckRadius, end + Vector3.back * CheckRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(end, CheckRadius);
        }


        #endif


    }


}

