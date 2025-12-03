using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder;

namespace Entropek.UnityUtils
{
    public class NavMeshUtils : MonoBehaviour
    {

        /// <summary>
        /// Gets a random point on the NavMesh.
        /// </summary>
        /// <param name="navMeshQueryFilter">The baked NavMeshSurface to target.</param>
        /// <param name="center">The point (in world-space) to start the random search at.</param>
        /// <param name="randomRadiusMin">The minimum area around the center point to try find a random point.</param>
        /// <param name="randomRadiusMax">The maximum area around the center point to try find a random point.</param>
        /// <param name="queryRadius">The area around a generated random point to try and connect to a NavMeshSurface</param>
        /// <param name="position">A random point on the NavMeshSurface; otherwise the center if foundPoint is returned as false.</param>
        /// <param name="iterations">The amount of iterations to find a random point.</param>
        /// <returns>true, if a point was successfully found; otherwise false.</returns>

        public static bool GetRandomPoint(
            in NavMeshQueryFilter navMeshQueryFilter, 
            Vector3 center, 
            float randomRadiusMin, 
            float randomRadiusMax, 
            float queryRadius, 
            out NavMeshHit point, 
            byte iterations = 16)
        {

            // try finding random point over max iterations.

            for(int i = 0; i < iterations; i++)
            {
                // get a random point within a 1 unit scaled sphere.

                Vector3 randomPoint = Random.insideUnitSphere;
                
                // scale the random point; keeping it within the min and max bounds.

                randomPoint = (randomPoint * randomRadiusMin) + (randomPoint * Random.Range(0, randomRadiusMax));

                // shift to the center position.

                randomPoint += center;
                
                // check if that point is on (or near) the nav mesh surface.

                if (NavMesh.SamplePosition(randomPoint, out point, queryRadius, navMeshQueryFilter))
                {
                    return true;


                    // NavMeshHit edgeHit;
                    // if (NavMesh.FindClosestEdge(point.position, out edgeHit, navMeshQueryFilter))
                    // {

                    //     // replace the returned point if you want the normal included
                    //     point = edgeHit;

                    //     return true;
                    // }
                }

            }

            // no random point was found.

            point = new();
            return false;
        }
    }    
}

