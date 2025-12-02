using Entropek.Exceptions;
using Entropek.UnityUtils.Attributes;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Entropek.UnityUtils
{
    
    [DefaultExecutionOrder(-5)]
    public class NavMeshSurfaceManager : MonoBehaviour
    {
        public static NavMeshSurfaceManager Singleton {get; private set;}

        [SerializeField] NavMeshSurface[] navMeshSurfaces;
        [RuntimeField] NavMeshDataInstance[] navMeshInstances; // order is relative to navMeshSurfaces;
        [RuntimeField] NavMeshTriangulation[] navMeshTriangulations; // order is relative to navMeshSurfaces;

        void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else if (Singleton != this)
            {
                throw new SingletonException("There can only be one NavMeshSurfaceManager.");
            }

            IntialiseArrays();
            CalculateAllNavMeshSurfacesTriangulation();
            AddAllNavMeshSurfaceData();
        }

        void OnDestroy()
        {
            RemoveAllNavMeshSurfaceData();
        }

        /// <summary>
        /// Creates arrays for NavMeshDataInstance and NavMeshTriangulation.
        /// </summary>

        private void IntialiseArrays()
        {
            int size = navMeshSurfaces.Length;
            navMeshInstances = new NavMeshDataInstance[size];
            navMeshTriangulations = new NavMeshTriangulation[size];
        }

        /// <summary>
        /// Adds all of the stored NavMeshSurface's to the NavMesh.
        /// </summary>

        private void AddAllNavMeshSurfaceData()
        {
            for(int i = 0; i < navMeshSurfaces.Length; i++)
            {
                navMeshInstances[i] = NavMesh.AddNavMeshData(navMeshSurfaces[i].navMeshData);
            }
        }

        /// <summary>
        /// Removes all of the stored NavMeshSurface's data from the NavMesh.
        /// </summary>

        private void RemoveAllNavMeshSurfaceData()
        {
            for(int i = 0; i < navMeshSurfaces.Length; i++)
            {
                navMeshInstances[i].Remove();
            }
        }

        /// <summary>
        /// Calculates the mesh triangulation for all stored NavMeshSurfaces.
        /// Note:
        ///     Make sure to call RemoveAllNavMeshSurfaceData() before calling this;
        ///     As well as adding back all NavMeshSurface's that were previously loaded.
        /// </summary>

        private void CalculateAllNavMeshSurfacesTriangulation()
        {
            // add and remove a single instance of the navmesh data as NavMesh.CalculateTriangulation()
            // calculates for all loaded nav meshes; there is no CalculateTriangulation() function for individual nav mesh
            // data instances because Unity is a dog shit engine :)

            for(int i = 0; i < navMeshSurfaces.Length; i++)
            {
                NavMeshDataInstance instance = NavMesh.AddNavMeshData(navMeshSurfaces[i].navMeshData);
                navMeshTriangulations[i] = NavMesh.CalculateTriangulation();
                NavMesh.RemoveNavMeshData(instance);
            }
        }


        /// <summary>
        /// Calculates the midpoints of vertices for a nav mesh surface's data. 
        /// </summary>
        /// <param name="navMeshSurfaceId">The index of the nav mesh suraface in the internal array.</param>
        /// <returns>A new Vector3 array of the midpoint locations.</returns>

        public Vector3[] GetNavMeshSurfaceMidpoints(int navMeshSurfaceId)
        {
            ref NavMeshTriangulation triangulation = ref navMeshTriangulations[navMeshSurfaceId];
            
            Vector3[] midpoints = new Vector3[triangulation.indices.Length/3];
            
            for(int i = 0; i < triangulation.indices.Length; i+=3)
            {
                Vector3 a = triangulation.vertices[triangulation.indices[i]];
                Vector3 b = triangulation.vertices[triangulation.indices[i+1]];
                Vector3 c = triangulation.vertices[triangulation.indices[i+2]];

                // midpoints[i] = GetMidPoint(a,b);
                // midpoints[i+1] = GetMidPoint(b,c);
                // midpoints[i+2] = GetMidPoint(c,a);
                midpoints[i/3] = GetMidPoint(a,b,c);
            }

            return midpoints;
        }

        public LayerMask GetNavMeshSurfaceLayerMask(int navMeshSurfaceId)
        {
            return navMeshSurfaces[navMeshSurfaceId].layerMask;
        }

        private Vector3 GetMidPoint(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 distanceAB = a - b;
            Vector3 distanceAC = a - c;
            return a - (distanceAB * 0.5f) - (distanceAC * 0.5f);
        }

#if UNITY_EDITOR

        // this is some drawing code for debuggin midpoints.

        // private void OnDrawGizmos()
        // {
        //     if (Application.IsPlaying(this) == false)
        //     {
        //         return;
        //     }
            
        //     Gizmos.color = Color.black;
            
        //     for(int i = 0; i < navMeshTriangulations.Length; i++)
        //     {
        //         ref NavMeshTriangulation triangulation = ref navMeshTriangulations[i];
        //         for (int t = 0; t < triangulation.indices.Length; t += 3)
        //         {
        //             Vector3 a = triangulation.vertices[triangulation.indices[t]];
        //             Vector3 b = triangulation.vertices[triangulation.indices[t + 1]];
        //             Vector3 c = triangulation.vertices[triangulation.indices[t + 2]];
                    
        //             Gizmos.DrawCube(GetMidPoint(a,b), Vector3.one);
        //             Gizmos.DrawCube(GetMidPoint(b,c), Vector3.one);
        //             Gizmos.DrawCube(GetMidPoint(c,a), Vector3.one);
        //         }
        //     }
        // }

#endif

    }

}

