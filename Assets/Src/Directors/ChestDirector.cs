using System.Collections.Generic;
using Entropek.UnityUtils;
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

    void Awake()
    {
        spawnLocations = NavMeshSurfaceManager.Singleton.GetNavMeshSurfaceMidpoints(NavMeshSurfaceId);

        for(int i = 0; i < 25; i++)
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

    private void OnDrawGizmos()
    {
        if (Application.IsPlaying(this) == false)
        {
            return;
        }
        
        Gizmos.color = Color.white;
        
        for(int i = 0; i < spawnLocations.Length; i++)
        {                
            Gizmos.DrawCube(spawnLocations[i], Vector3.one);
        }
    }

    public bool SpawnAtRandomPosition(SpawnCard spawnCard, out GameObject instantiatedGameObject)
    {

        int rand = UnityEngine.Random.Range(0, spawnLocations.Length);

        Vector3 centerPosition = spawnLocations[rand];

        bool foundPoint = Entropek.UnityUtils.NavMeshUtils.GetRandomPoint(
            spawnCard.GetNavMeshQueryFilter(),
            centerPosition,
            MinRandomSpawnRadius,
            MaxRandomSpawnRadius,
            RandomSpawnQueryRadius,
            out Vector3 position
        );

        if(foundPoint == false)
        {
            instantiatedGameObject = null;
            return false;
        }

        instantiatedGameObject = Instantiate(spawnCard.Prefab, centerPosition, Quaternion.identity);
        return true;
    }

}
