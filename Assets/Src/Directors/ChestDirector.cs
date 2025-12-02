using System.Collections.Generic;
using Entropek.UnityUtils;
using Entropek.UnityUtils.Attributes;
using TreeEditor;
using UnityEngine;
using UnityEngine.AI;

public class ChestDirector : MonoBehaviour
{
    // Note:
    //  nav mesh surface voxel size per agent should be 1 for beste venly ditrubted results.

    private const int NavMeshSurfaceId = 0;
    private const float MinRandomSpawnRadius = 1;
    private const float MaxRandomSpawnRadius = 24;
    private const float RandomSpawnQueryRadius = 0.5f;

    [SerializeField] private ChestSpawnCard[] chestSpawnCards;

    Vector3[] spawnLocations;

    Ray ray = new Ray();
    RaycastHit[] hits = new RaycastHit[1];

    void Awake()
    {
        spawnLocations = NavMeshSurfaceManager.Singleton.GetNavMeshSurfaceMidpoints(NavMeshSurfaceId);

        for(int i = 0; i < 256; i++)
        {   
            int x = 0;
            while(
                SpawnAtRandomPosition(chestSpawnCards[0], out GameObject instantiatedGameObject) == false
                && x <= 16
            )
            {
                x++;
            } 
        }
    }


    public bool SpawnAtRandomPosition(SpawnCard spawnCard, out GameObject instantiatedGameObject)
    {
        instantiatedGameObject = null;

        // use a random spawn location.

        int rand = UnityEngine.Random.Range(0, spawnLocations.Length);
        Vector3 centerPosition = spawnLocations[rand];

        // find a valid random point at the spawn location.

        if(Entropek.UnityUtils.NavMeshUtils.GetRandomPoint(
            spawnCard.GetNavMeshQueryFilter(),
            centerPosition,
            MinRandomSpawnRadius,
            MaxRandomSpawnRadius,
            RandomSpawnQueryRadius,
            out NavMeshHit point
        ) == false)
        {
            return false;
        }

        // ray cast to down to get the floor hit point and normals to spawn at
        // ; as the GetRandomPoint() uses navmesh queries that are inaccurate.

        ray = new Ray(point.position, Vector3.down);
        if(Physics.RaycastNonAlloc(ray, hits, float.MaxValue, NavMeshSurfaceManager.Singleton.GetNavMeshSurfaceLayerMask(NavMeshSurfaceId)) == 0)
        {
            return false;
        }

        instantiatedGameObject = Instantiate(spawnCard.Prefab, hits[0].point, Quaternion.identity);
        Transform t = instantiatedGameObject.transform;
        
        // face a random direction.
        
        Entropek.UnityUtils.TransformUtils.RotateYAxisToDirection(
            t,
            new Vector3(
                Random.Range(-1f,1f),
                Random.Range(-1f,1f),
                Random.Range(-1f,1f)
            ),
            1
        );

        // align the up vector with the ground normal.

        Transform instantiateddTransform = instantiatedGameObject.transform;
        instantiateddTransform.rotation = Entropek.UnityUtils.QuaternionUtils.AlignRotationToVector(
            instantiateddTransform.rotation,
            instantiateddTransform.up,
            hits[0].normal
        );
        return true;
    }

    // private void OnDrawGizmos()
    // {
    //     if (Application.IsPlaying(this) == false)
    //     {
    //         return;
    //     }
        
    //     Gizmos.color = Color.white;
        
    //     for(int i = 0; i < spawnLocations.Length; i++)
    //     {                
    //         Gizmos.DrawCube(spawnLocations[i], Vector3.one);
    //     }
    // }

}
