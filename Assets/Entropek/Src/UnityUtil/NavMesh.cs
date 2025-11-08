using UnityEngine;
using UnityEngine.AI;

namespace Entropek.UnityUtils
{
    public class NavMeshUtils : MonoBehaviour
    {

        /// <summary>
        /// Gets a random point on the NavMesh.
        /// </summary>
        /// <param name="navMeshQueryFilter">The baked NavMeshSurface to target.</param>
        /// <param name="center">The point (in world-space) to start the random search at.</param>
        /// <param name="randomRadius">The area around the center point to try find a random point.</param>
        /// <param name="queryRadius">The area around a generated random point to try and connect to a NavMeshSurface</param>
        /// <param name="foundPoint">Whether or not a point was successgully found.</param>
        /// <param name="iterations">The amount of iterations to find a random point.</param>
        /// <returns>A random point on the NavMeshSurface; otherwise the center if foundPoint is returned as false.</returns>

        public static Vector3 GetRandomPoint(in NavMeshQueryFilter navMeshQueryFilter, Vector3 center, float randomRadius, float queryRadius, out bool foundPoint, byte iterations = 16)
        {

            // try finding random point over max iterations.

            for(int i = 0; i < iterations; i++)
            {
                Vector3 randomPoint = center + Random.insideUnitSphere * Random.Range(0, randomRadius);
                
                if(NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, queryRadius, navMeshQueryFilter))
                {
                    
                    // the random point was successfully validated.
                    
                    foundPoint = true;
                    return hit.position;
                }
            }

            // no random point was found.

            foundPoint = false;
            return center;
        }
    }    
}

