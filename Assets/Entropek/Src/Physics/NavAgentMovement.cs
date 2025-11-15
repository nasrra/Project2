using System;
using Entropek.UnityUtils.Attributes;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor.Rendering;
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

        [Header("Data")]
        [Tooltip("how fast this agent adjusts its move direction after passing a corner.")]
        [SerializeField][Range(0,1)] protected float cornerSpeed = 1;

        [RuntimeField] protected NavMeshPath path;

        private const float cornerDistanceThreshold = 0.05f;
        private const int fixedFramesPerRecalculate = 30; // recalc every 25 frames or half a second.
        
        private int fixedFrameCounter = 0;
        [RuntimeField] protected int currentCornerIndex = 0;

        private bool paused = false;


        protected override void Awake()
        {
            navAgent.isStopped = true;
        }


        /// 
        /// Base.
        /// 


        protected override void Update()
        {
            // put the update tick in Fixed update for enemies,
            // for better performance with minimal difference in smoothness.
        }

        protected void FixedUpdate(){
            UpdateTick();

            // short-circuit if we are paused.

            if (paused == true)
            {
                return;
            }

            // handle the recacluate path recursion.
            
            fixedFrameCounter++;
            if (fixedFrameCounter >= fixedFramesPerRecalculate)
            {
                if (navAgent.isOnOffMeshLink == false)
                {
                    RecalculatePath?.Invoke();
                }
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

        public void StartPath(Vector3 destinationInWorldSpace)
        {
            // create a new path if needed.

            path??= new NavMeshPath();

            currentCornerIndex = 0;

            // set the RecalculatePath to this function call
            // to recursively update.

            RecalculatePath = () =>
            {
                // recalc path until we cant anymore.

                // if (StartPath(destinationInWorldSpace))
                // {
                //     RecalculatePath = null;
                // }
                StartPath(destinationInWorldSpace);
            };

            Vector3 destination = ProjectWorldPositionOnMesh(destinationInWorldSpace);

            navAgent.CalculatePath(destination, path);

            IsOnNavMeshLink();
        }

        /// <summary>
        /// Set the agent to calculate and start moving along a path towards a transform's position.
        /// </summary>
        /// <param name="target"></param>
        /// <returns> true if a path was found. </returns>

        public void StartPath(Transform target)
        {
            if (target == null)
            {
                return;
            }

            // create a new path if needed.

            path??= new NavMeshPath();

            currentCornerIndex = 0;

            // set the RecalculatePath to this function call
            // to recursively update.

            RecalculatePath = () =>
            {
                StartPath(target);
            };

            Vector3 destination = ProjectWorldPositionOnMesh(target.position);

            navAgent.CalculatePath(destination, path);

            IsOnNavMeshLink();
        }

        /// <summary>
        /// Sets the agent to calculate and state moving along a path away from a transform's position.
        /// </summary>
        /// <param name="target">The transform to target.</param>
        /// <param name="distance">The distance away from the NavAgentMovement gameObject to check for a valid point to move to.</param>

        public void MoveAway(Transform target, float distance)
        {

            if(target == null)
            {
                return;
            }

            // create a new path if needed.

            path??= new NavMeshPath();

            currentCornerIndex = 0;

            // set the RecalculatePath to this function call
            // to recursively update.

            RecalculatePath = () =>
            {
                MoveAway(target, distance);
            };

            Vector3 oppositeDirection = (transform.position - target.position).normalized;            
            Vector3 destination = ProjectWorldPositionOnMesh(oppositeDirection * distance);


            // move towards that desired destination.

            navAgent.CalculatePath(destination, path);

            IsOnNavMeshLink();
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

        protected virtual void MoveToNextPathPoint()
        {
            if (path == null || HaveReachedDestination())
            {
                return;
            }

            // traverse along them.

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

                moveDirection = Vector3.MoveTowards(moveDirection, direction, cornerSpeed);
            }
        }

        private bool IsOnNavMeshLink()
        {
            navAgent.CompleteOffMeshLink();
            navAgent.SetDestination(path.corners[Math.Clamp(currentCornerIndex, 0, path.corners.Length-1)]);
            navAgent.isStopped = true;
            return navAgent.isOnOffMeshLink;
        }


        /// <summary>
        /// Updates the existing calcualted path with the current frames position,
        /// Determing which point/corner along the path is the current one to move to
        /// and if this agent has reached its destination.
        /// </summary>

        private void UpdatePath()
        {
            if(path == null)
            {
                return;
            }

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
                    IsOnNavMeshLink();
                    if (currentCornerIndex >= path.corners.Length)
                    {
                        // reached path.
                        path = null;
                        moveDirection = Vector3.zero;
                        ReachedDestination?.Invoke();
                    }
                }
            }
        }

        protected bool HaveReachedDestination()
        {
            return path.corners.Length == 0 || (path.corners.Length > 0 && currentCornerIndex >= path.corners.Length);
        }

        private Vector3 ProjectWorldPositionOnMesh(Vector3 worldPosition)
        {            
            Vector3 currentNavMeshWorldPosition = navAgent.transform.position; 

            // warp to the target position then call Move so that the nav agent snaps
            // back to the grid (if the agent is off the grid).

            navAgent.Warp(worldPosition);
            navAgent.Move(Vector3.zero);

            Vector3 positionOnMesh = navAgent.transform.position;

            // warp back the nav agent to our position so that the nav agent isnt located at the desired destination.

            navAgent.Warp(currentNavMeshWorldPosition);

            // Note:
            //  The equality check is only for the x and z-axis, the y-axis should be freeform
            //  as nav agents can jump, hover, and fly.
            //  this check is needed for when agents go completely off the mesh, allowing them to return
            //  and course correct.

            if(navAgent.transform.position.x != transform.position.x
            || navAgent.transform.position.z != transform.position.z)
            {
                // warp to the closest possible position at our transform.
                navAgent.Warp(transform.position);
                
                // call move to ensure it snaps to the grid.

                navAgent.Move(Vector3.zero);

                if(navAgent.isOnNavMesh == false)
                {
                    
                    // warp back if our transform position is way too far away from a nav mesh
                    // it isnt even snapped to one.

                    navAgent.Warp(currentNavMeshWorldPosition);                    
                }
            }


            return positionOnMesh;
        }

        #if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if(path == null || path.corners.Length == 0)
            {
                return;
            }



            for(int i = 0; i < path.corners.Length; i++)
            {
                if (i == currentCornerIndex)
                {
                    Gizmos.color = Color.red;                    
                }
                else
                {
                    Gizmos.color = Color.green;
                }
                Gizmos.DrawCube(path.corners[i], Vector3.one);
            }
        }

        #endif

    }

}
