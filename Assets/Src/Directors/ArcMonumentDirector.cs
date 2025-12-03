// using Entropek.UnityUtils;
// using UnityEngine;

// public class ArcMonumentDirector : Director
// {
//     private const int NavMeshSurfaceId = 1;

//     private const float MinRandomSpawnRadius = 1;
//     private const float MaxRandomSpawnRadius = 24;
//     private const float RandomSpawnQueryRadius = 0.5f;

//     private const int MaxRandomSpawnIterations = 16;

//     [SerializeField] private SpawnCard[] spawnCards;

//     private LayerMask navMeshLayerMask;

//     Vector3[] spawnLocations;
//     bool[] occupiedSpawnLocations; // ordering is relative to spawn locations array.


//     ///
//     /// Base.
//     /// 


//     void Awake()
//     {
//         Initialise();
//         TestSpawn();
//     }

//     void Initialise()
//     {

//         navMeshLayerMask = NavMeshSurfaceManager.Singleton.GetNavMeshSurfaceLayerMask(NavMeshSurfaceId);

//         spawnLocations = NavMeshSurfaceManager.Singleton.GetNavMeshSurfaceMidpoints(NavMeshSurfaceId);
//         occupiedSpawnLocations = new bool[spawnLocations.Length];
//     }

//     void TestSpawn()
//     {
//         for(int i = 0; i < 32; i++)
//         {
//             int x = 0;
//             while(x <= MaxRandomSpawnIterations)
//             {
//                 x++;

//                 if(SpawnAtRandomPosition(
//                     spawnLocations, 
//                     spawnCards[0],
//                     MinRandomSpawnRadius,
//                     MaxRandomSpawnRadius,
//                     RandomSpawnQueryRadius,
//                     navMeshLayerMask,
//                     out GameObject instantiatedGameObject
//                 ) == false)
//                 {
//                     continue;
//                 }
//                 else
//                 {
//                     break;
//                 }
//             }
//         }
//     }

//     private void OnDrawGizmos()
//     {
//         if (Application.IsPlaying(this) == false)
//         {
//             return;
//         }
        
//         Gizmos.color = Color.white;
        
//         for(int i = 0; i < spawnLocations.Length; i++)
//         {                
//             Gizmos.DrawCube(spawnLocations[i], Vector3.one);
//         }
//     }
// }
