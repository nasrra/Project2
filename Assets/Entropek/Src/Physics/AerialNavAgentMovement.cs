using Entropek.Physics;
using Entropek.UnityUtils.Attributes;
using UnityEngine;
using UnityEngine.Timeline;

namespace Entropek.Physics
{
    
    public class AerialNavAgentMovement : NavAgentMovement
    {
        private enum HoverState : byte
        {
            Descend,
            Ascend
        }

        [Header(nameof(AerialNavAgentMovement)+" Data")]
        [SerializeField] LayerMask groundLayer;
        [SerializeField] float hoverSpeed = 1;
        [SerializeField] float hoverSmooth = 1;
        [RuntimeField] float hoverWeight;
        [RuntimeField] HoverState hoverState = HoverState.Ascend;
        [RuntimeField] Vector3 hoverMovement;
        RaycastHit hit;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (navAgent.isOnOffMeshLink == false)
            {
                Hover();
                CalculateHover();
            }
        }

        private void Hover()
        {            
            // apply the hover movement.

            Vector3 desiredMovement = 
                hoverState == HoverState.Ascend
                ? Vector3.up
                : Vector3.down;
            
            desiredMovement *= hoverSpeed * hoverWeight;
            hoverMovement = Vector3.MoveTowards(hoverMovement, desiredMovement, hoverSpeed * UnityEngine.Time.deltaTime);

            controller.Move(hoverMovement);
        }

        private void CalculateHover()
        {
            float desiredHeight = navAgent.height;
            
            if(UnityEngine.Physics.Raycast(transform.position, Vector3.down, out hit, desiredHeight * 2, groundLayer))
            {
                float distance = (hit.point - transform.position).magnitude;

                // larger the difference, hover weight is 1
                // smaller the difference (distance == navAgent.height), hover weight is 0.

                hoverWeight = 1 - (1f / (1f + distance));

                // ascend when below our desired height, otherwise descend.

                hoverState = distance <= desiredHeight 
                ? HoverState.Ascend
                : HoverState.Descend;
                // Debug.Log(1);
            }
            else
            {
                hoverState = HoverState.Descend;
                hoverWeight = 1;
            }
        }

    }
}

