using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;


namespace Entropek.Physics
{
    
    [DefaultExecutionOrder(-10)]
    public class NavAgentMovementTarget : MonoBehaviour
    {
        [SerializeField] NavMeshSurface[] navMeshSurfacePrefabs;
        [SerializeField] NavMeshAgent[] navAgents;
        Dictionary<int, NavMeshAgent> targets = new Dictionary<int, NavMeshAgent>();

        void Awake()
        {
            InitialiseTargets();
        }

        private void InitialiseTargets()
        {
            for(int i = 0; i < navAgents.Length; i++)
            {
                targets.Add(navAgents[i].agentTypeID, navAgents[i]);
            }        
        }

        private void FixedUpdate()
        {
            for(int i = 0; i < navMeshSurfacePrefabs.Length; i++)
            {
                NavMeshSurface surface = navMeshSurfacePrefabs[i];
                ProjectWorldPositionOnNavMeshAgent(targets[surface.agentTypeID], surface);
            }
        }

        /// <summary>
        /// Projects a world position onto the given NavAgents NavMeshSurface.
        /// </summary>
        /// <param name="worldPosition">The specified position to project.</param>
        /// <returns>The projected position.</returns>

        private void ProjectWorldPositionOnNavMeshAgent(NavMeshAgent agent, NavMeshSurface surface)
        {            
            Vector3 currentNavMeshWorldPosition = agent.transform.position; 
            NavMeshQueryFilter filter = new NavMeshQueryFilter()
            {
                agentTypeID = agent.agentTypeID,
                areaMask = NavMesh.AllAreas  
            };
            
            float radius = agent.radius * 2;

            NavMeshHit navHit;
            if (NavMesh.SamplePosition(transform.position, out navHit, radius, filter))
            {
                agent.Warp(navHit.position);
                return;
            }       

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
        }
    }
}

