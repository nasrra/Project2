using System;
using UnityEngine;
using UnityEngine.AI;

///
/// Note:
///     The Nav Agent should not be a component on the root game object of an AI.
///     It should be a child of the root, with this component being attatched to the root.
///     This allows the nav agent to remain on the mesh (by always snapping itself to the nav mesh surface), providing accurate navigational data.
/// 

// TODO:
//
//  - when returning to nearest nav tile: create origin point (transform.x, agent.y, transform.z) to the found point on the grid.
//
//  - when falling: dont follow the path, 
//      - once first grounded and outside of nav mesh: return to nearest nav tile.
//
//  - when outside of nav mesh: return to nearest nav tile.
//
//  - dont follow any path when not grounded.
// 

namespace Entropek.Physics
{

    public class NavAgentMovement : CharacterControllerMovement
    {

        public event Action ReachedDestination;
        private Action RecalculatePath;

        [Header(nameof(NavAgentMovement) + " Components")]
        [SerializeField] protected NavMeshAgent navAgent;


        NavMeshPath path;

        private const float cornerDistanceThreshold = 0.05f;
        private const int fixedFramesPerRecalculate = 25; // recalc every 25 frames or half a second.
        
        private int fixedFrameCounter = 0;
        private int currentCornerIndex = 0;

        private bool paused = false;


        /// 
        /// Base.
        /// 


        protected void FixedUpdate(){
            // handle the recacluate path recursion.

            fixedFrameCounter++;
            if (fixedFrameCounter >= fixedFramesPerRecalculate)
            {
                RecalculatePath?.Invoke();
                fixedFrameCounter = 0;
            }

            UpdatePath();
            MoveToNextPathPoint();
        }


        /// 
        /// Functions.
        /// 


        /// <summary>
        /// Set the agent to calculate and start moving along a path towards a target position in world space.
        /// </summary>
        /// <param name="destinationInWorldSpace"></param>
        /// <returns> true if a path was found </returns>

        public bool StartPath(Vector3 destinationInWorldSpace)
        {
            currentCornerIndex = 0;

            if (path == null)
            {
                path = new NavMeshPath();
            }

            // set the RecalculatePath to this function call
            // to recursively update.

            RecalculatePath = () =>
            {
                // recalc path until we cant anymore.

                if (StartPath(destinationInWorldSpace))
                {
                    RecalculatePath = null;
                }
            };

            return navAgent.CalculatePath(destinationInWorldSpace, path);
        }

        /// <summary>
        /// Set the agent to calculate and start moving along a path towards a transform targets position.
        /// </summary>
        /// <param name="target"></param>
        /// <returns> true if a path was found. </returns>

        public bool StartPath(Transform target)
        {

            if (target == null)
            {
                return false;
            }

            currentCornerIndex = 0;

            if (path == null)
            {
                path = new NavMeshPath();
            }

            // set the RecalculatePath to this function call
            // to recursively update.

            RecalculatePath = () =>
            {

                // recalc path until we cant anymore.

                if (StartPath(target) == false)
                {
                    // RecalculatePath = null;
                }
            };

            return navAgent.CalculatePath(target.position, path);
        }

        public void PausePath()
        {
            paused = true;
            moveDirection = Vector3.zero;
        }

        public void ResumePath()
        {
            paused = false;
        }

        /// <summary>
        /// Sets the move direction to the direction from the nav agent to the next path point/corner.
        /// </summary>

        private void MoveToNextPathPoint()
        {

            // short-circuit if we are paused.

            if (paused == true)
            {
                return;
            }


            // if there is still path points to traverse along.

            if (path.corners.Length > 0 && currentCornerIndex < path.corners.Length)
            {

                // traverse along them.

                Vector3 corner = path.corners[currentCornerIndex];
                Vector3 direction = (path.corners[currentCornerIndex] - navAgent.transform.position).normalized;

                // The Vector3.up check here is done to ensure to skip over the path point
                // that is incorrectly calculated when the target is above, below, or in the same location as this agent.

                if (direction == Vector3.up)
                {
                    currentCornerIndex++;
                }
                else
                {
                    // move in the direction of the next path point.

                    moveDirection = direction;
                }
            }
        }

        private bool IsOnNavMeshOrMeshLink()
        {
            return navAgent.isOnNavMesh || navAgent.isOnOffMeshLink;
        }


        /// <summary>
        /// Updates the existing calcualted path with the current frames position,
        /// Determing which point/corner along the path is the current one to move to
        /// and if this agent has reached its destination.
        /// </summary>

        private void UpdatePath()
        {
            // if there is still path points to traverse along.

            if (path.corners.Length > 0 && currentCornerIndex < path.corners.Length)
            {

                // get the current speed this agent is moving at and
                // factor that into the distance threshold when determining whether or not
                // this agent has reached its next path point.

                float currentSpeed = controller.velocity.magnitude * 0.1f;
                float speedThreshold = Mathf.Clamp(currentSpeed, cornerDistanceThreshold, 1.0f);

                if (Vector3.Distance(navAgent.transform.position, path.corners[currentCornerIndex]) <= speedThreshold)
                {

                    currentCornerIndex++;
                    if (currentCornerIndex >= path.corners.Length)
                    {
                        // reached path.
                        moveDirection = Vector3.zero;
                        ReachedDestination?.Invoke();
                    }
                }
            }
            else
            {
                moveDirection = Vector3.zero;
            }
        }

    }

}
