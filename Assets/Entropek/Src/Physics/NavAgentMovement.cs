using System;
using Entropek.UnityUtils.Attributes;
using Unity.AI.Navigation;
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
        [Tooltip("The NavMeshAgent must be on a child object of this scripts gameobject in order to funciton properly.")]
        [SerializeField] protected NavMeshAgent navAgent;
        [SerializeField] protected NavMeshSurface navMeshSurfacePrefab; 

        [Header("Data")]
        [Tooltip("how fast this agent adjusts its move direction after passing a corner.")]
        [SerializeField][Range(0,1)] protected float cornerSpeed = 1;

        [RuntimeField] protected NavMeshPath path;

        private const float cornerDistanceThreshold = 0.66f;
        private const int fixedFramesPerRecalculate = 30; // recalc every 25 frames or half a second.
        
        private int fixedFrameCounter = 0;
        [RuntimeField] protected int currentCornerIndex = 0;

        private bool paused = false;


        protected override void Awake()
        {
            base.Awake();

            // stop the nav agent from moving,
            // as this script uses the SetDestination function to see if
            // the agent is currently on a mesh link.

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

        protected virtual void FixedUpdate(){

            // short-circuit if we are paused.

            if (paused == false)
            {
                if (navAgent.isOnOffMeshLink == false)
                {
                    ProjectWorldPositionOnNavMeshAgent(navAgent, navMeshSurfacePrefab);
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

                if (path != null)
                {
                    if(UpdatePath(out NavAgentPathCornerContext currentCornerContext)){
                        if (path == null || HaveReachedDestination())
                        {
                            return;
                        }

                        MoveToNextPathPoint(ref currentCornerContext);                
                    }

                }
            }


            UpdateTick();
        }


        /// 
        /// Functions.
        /// 


        /// <summary>
        /// Set the agent to calculate and start moving along a path towards a transform's position.
        /// </summary>
        /// <param name="target"></param>
        /// <returns> true if a path was found. </returns>

        public void StartPath(NavAgentMovementTarget target)
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

            Vector3 destination = target.transform.position;

            navAgent.CalculatePath(destination, path);

            IsOnNavMeshLink();
        }

        /// <summary>
        /// Sets the agent to calculate and state moving along a path away from a transform's position.
        /// </summary>
        /// <param name="target">The transform to target.</param>
        /// <param name="distance">The distance away from the NavAgentMovement gameObject to check for a valid point to move to.</param>

        public void MoveAway(NavAgentMovementTarget target, float distance)
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

            Vector3 oppositeDirection = (transform.position - target.transform.position).normalized;            
            Vector3 destination = oppositeDirection * distance;

            Vector3 currentNavMeshWorldPosition = navAgent.transform.position; 

            // warp to the target position then call Move so that the nav agent snaps
            // back to the grid (if the agent is off the grid).

            // navAgent.Warp(destination);
            navAgent.Move(destination);

            Vector3 positionOnMesh = navAgent.transform.position;

            // warp back the nav agent to our position so that the nav agent isnt located at the desired destination.

            navAgent.Warp(currentNavMeshWorldPosition);

            // move towards that desired destination.

            navAgent.CalculatePath(positionOnMesh, path);

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

        protected virtual void MoveToNextPathPoint(ref NavAgentPathCornerContext context)
        {
            // The Vector3.up check here is done to ensure to skip over the path point
            // that is incorrectly calculated when the target is above, below, or in the same location as this agent.

            if (context.DirectionToCorner == Vector3.up)
            {
                currentCornerIndex++;
            }
            else
            {
                // move in the direction of the next path point.

                moveDirection = Vector3.MoveTowards(moveDirection, context.DirectionToCorner, cornerSpeed);
            }
        }

        /// <summary>
        /// Checks whether or not the nav agent is currently on a mesh link.
        /// </summary>
        /// <returns>true, if the agent is on a mesh link; otherwise false.</returns>

        private bool IsOnNavMeshLink()
        {

            // always complete the off mesh link as the NavMeshAgent does not automatically set 
            // the flag to false after moving across a link (Unity is awesome!)

            navAgent.CompleteOffMeshLink();

            if(0 < path.corners.Length - 1)
            {
                navAgent.SetDestination(path.corners[System.Math.Clamp(currentCornerIndex, 0, path.corners.Length-1)]);
            }
            else
            {
                navAgent.SetDestination(navAgent.transform.position);                
            }

            navAgent.isStopped = true;

            return navAgent.isOnOffMeshLink;
        }


        /// <summary>
        /// Updates the existing calcualted path with the current frames position,
        /// Determing which point/corner along the path is the current one to move to
        /// and if this agent has reached its destination.
        /// </summary>

        private bool UpdatePath(out NavAgentPathCornerContext context)
        {
            
            // if there is still path points to traverse along.

            if (path.corners.Length > 0 && currentCornerIndex < path.corners.Length)
            {
                // out the current vorner this agent is moving towards.

                context = CalculateCornerContext(currentCornerIndex);

                // get the current speed this agent is moving at and
                // factor that into the distance threshold when determining whether or not
                // this agent has reached its next path point.

                float currentSpeed = controller.velocity.magnitude * 0.1f;
                float speedThreshold = Mathf.Clamp(currentSpeed, cornerDistanceThreshold, 1.0f);

                if (context.DistanceToCorner <= speedThreshold)
                {
                    currentCornerIndex++;
                    
                    //  check if the next corner is a path on a mesh link.

                    IsOnNavMeshLink();

                    if (currentCornerIndex >= path.corners.Length)
                    {
                        // reached path.
                        path = null;
                        moveDirection = Vector3.zero;
                        ReachedDestination?.Invoke();
                    }
                    else
                    {
                        
                        // out the next corner this agent is trying to move towards.

                        context = CalculateCornerContext(currentCornerIndex);
                    }
                }

                return true;
            }

            // out nothing if there is no path to traverse.

            context = new();

            return false;
        }

        protected bool HaveReachedDestination()
        {
            return path.corners.Length == 0 || (path.corners.Length > 0 && currentCornerIndex >= path.corners.Length);
        }

        /// <summary>
        /// Calculates a data struct relative to the corner index of the path stored by this NavAgentMovement.
        /// </summary>
        /// <param name="cornerIndex">The specified corner index.</param>
        /// <returns>NavAgentPathCornerContext</returns>

        private NavAgentPathCornerContext CalculateCornerContext(int cornerIndex)
        {
            Vector3 vectorDistance = path.corners[currentCornerIndex] - navAgent.transform.position;
            Vector3 direction = vectorDistance.normalized;
            float distance = vectorDistance.magnitude;

            return new NavAgentPathCornerContext(
                vectorDistance,
                direction,
                distance
            );
        }


        /// <summary>
        /// Ensures that the nav mesh agent snaps back to ar recoverable position when this transform moves to an inaccessible space.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="surface"></param>

        private void ProjectWorldPositionOnNavMeshAgent(NavMeshAgent agent, NavMeshSurface surface)
        {            

            // check if the agents position has been offsetted from our actual position.
            // note the exclusion of the y-axis, this is because agents can jump and fly.

            Vector3 agentPosition = agent.transform.position; 
            if(agentPosition.x == transform.position.x && agentPosition.z == transform.position.z)
            {
                return;
            }

            // Only sample the nav mesh that is of the agents type id.

            NavMeshQueryFilter filter = new NavMeshQueryFilter()
            {
                agentTypeID = agent.agentTypeID,
                areaMask = NavMesh.AllAreas  
            };

            // use radius * 2 so there is a wider range of error
            // when querying a possible position to place the agent at.

            float radius = agent.radius * 2;

            // sample a position at the agents transform and snap to it if possible.

            NavMeshHit navHit;
            if (NavMesh.SamplePosition(transform.position, out navHit, radius, filter))
            {
                agent.Warp(navHit.position);
                return;
            }       

            // check below this gameobject and sample a position at the hit position, snapping where possible.

            RaycastHit rayHit;
            if(UnityEngine.Physics.Raycast(
                transform.position, 
                Vector3.down, 
                out rayHit, 
                float.MaxValue,
                surface.layerMask)){
                
                if(NavMesh.SamplePosition(rayHit.point, out navHit, radius, filter))
                {
                    agent.Warp(navHit.position);
                    return;
                }
            }

            // fall back is to snap to this transforms position.

            Vector3 position = new Vector3(transform.position.x, navHit.position.y, transform.position.z); 
            agent.Warp(position);
        }


        /// 
        /// Editor
        /// 


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
