// using System.Collections.Generic;
// using Entropek.UnityUtils;
// using Entropek.UnityUtils.Attributes;
// using TreeEditor;
// using UnityEngine;
// using UnityEngine.AI;

// public class ChestDirector : Director
// {
//     // Note:
//     //  nav mesh surface voxel size per agent should be 1 for beste venly ditrubted results.

//     private const int NavMeshSurfaceId = 0;

//     private const float MinRandomSpawnRadius = 1;
//     private const float MaxRandomSpawnRadius = 24;
    
//     private const float RandomSpawnQueryRadius = 0.5f;

//     private const int MaxRandomSpawnIterations = 16;

//     [SerializeField] private ChestSpawnCard[] chestSpawnCards;

//     private LayerMask navMeshLayerMask;

//     Vector3[] spawnLocations;


//     /// 
//     /// Base.
//     /// 


//     void Awake()
//     {
//         Initialise();
//         // TestSpawn();
//     }

//     private void Initialise()
//     {        
//         navMeshLayerMask = NavMeshSurfaceManager.Singleton.GetNavMeshSurfaceLayerMask(NavMeshSurfaceId);   
//         spawnLocations = NavMeshSurfaceManager.Singleton.GetNavMeshSurfaceMidpoints(NavMeshSurfaceId);
//     }

//     private void TestSpawn()
//     {
//         for(int i = 0; i < 256; i++)
//         {   
//             int x = 0;
//             while(true)
//             {
//                 if(x <= MaxRandomSpawnIterations)
//                 {
//                     x++;
                
//                     if(SpawnAtRandomPosition(
//                         spawnLocations,
//                         chestSpawnCards[0],
//                         MinRandomSpawnRadius,
//                         MaxRandomSpawnRadius,
//                         RandomSpawnQueryRadius,
//                         navMeshLayerMask,
//                         out GameObject instantiatedGameObject
//                     ) == false)
//                     {                    
//                         continue;
//                     }
//                     else
//                     {
//                         break;
//                     }
//                 }
//                 else
//                 {
//                     break;
//                 }
//             } 
//         }
//     }

//     // private void OnDrawGizmos()
//     // {
//     //     if (Application.IsPlaying(this) == false)
//     //     {
//     //         return;
//     //     }
        
//     //     Gizmos.color = Color.white;
        
//     //     for(int i = 0; i < spawnLocations.Length; i++)
//     //     {                
//     //         Gizmos.DrawCube(spawnLocations[i], Vector3.one);
//     //     }
//     // }
// }
