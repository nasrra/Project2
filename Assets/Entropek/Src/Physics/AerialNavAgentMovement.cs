using Entropek.Physics;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

public class AerialNavAgentMovement : NavAgentMovement
{
    [Header(nameof(AerialNavAgentMovement)+" Components")]
    [SerializeField] private float entityRadius;
    [SerializeField] GameObject test;
    [SerializeField] LayerMask obstructionLayers;
    RaycastHit hit;

    protected override void Update()
    {
        base.Update();
        if (navAgent.isOnOffMeshLink)
        {
            // Agent is currently traversing a link
            Debug.Log("Agent is on a link!");
        }
    }

    protected override void MoveToNextPathPoint()
    {
        if(path == null || HaveReachedDestination() == true)
        {
            return;
        }

        // NOTE:
        //  Unlink the base NavAgentMovement, we use the actual transform position, rather than the
        //  NavMeshAgent as we need y-positional data.

        Vector3 distance = path.corners[currentCornerIndex] - navAgent.transform.position;
        
        Vector3 direction = distance.normalized;

        // The Vector3.up check here is done to ensure to skip over the path point
        // that is incorrectly calculated when the target is above, below, or in the same location as this agent.

        if (direction == Vector3.up || direction == -Vector3.up)
        {
            currentCornerIndex++;
            return;
        }

        moveDirection = Vector3.MoveTowards(moveDirection, direction, cornerSpeed);
    }
}
