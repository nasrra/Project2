using Entropek.UnityUtils.Attributes;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Entropek.UnityUtils
{
    
    [DefaultExecutionOrder(-5)]
    public class NavMeshSurfaceManager : MonoBehaviour
    {
        [SerializeField] NavMeshSurface[] navMeshSurfaces;
        [RuntimeField] NavMeshDataInstance[] navMeshInstances;
        [RuntimeField] NavMeshTriangulation[] navMeshTriangulations;

        void Awake()
        {
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

        void OnDrawGizmos()
        {
            if (Application.IsPlaying(this) == false)
            {
                return;
            }
            
            Gizmos.color = Color.black;
            
            for(int i = 0; i < navMeshTriangulations.Length; i++)
            {
                ref NavMeshTriangulation triangulation = ref navMeshTriangulations[i];
                for (int t = 0; t < triangulation.indices.Length; t += 3)
                {
                    Vector3 a = triangulation.vertices[triangulation.indices[t]];
                    Vector3 b = triangulation.vertices[triangulation.indices[t + 1]];
                    Vector3 c = triangulation.vertices[triangulation.indices[t + 2]];

                    DrawMidPoint(a, b);
                    DrawMidPoint(b, c);
                    DrawMidPoint(c, a);

                    Gizmos.DrawLine(a, b);
                    Gizmos.DrawLine(b, c);
                    Gizmos.DrawLine(c, a);
                }

            }
        }

        void DrawMidPoint(Vector3 a, Vector3 b)
        {
            Vector3 distance = a - b;
            Gizmos.DrawCube(a - distance * 0.5f, Vector3.one);
        }
    }

}

