using System.Collections.Generic;
using Entropek.UnityUtils;
using UnityEngine;
using UnityEngine.AI;

public abstract class Director : MonoBehaviour
{


    /// 
    /// Ray cast cache
    /// 


    private RaycastHit[] hits = new RaycastHit[1];
    private Ray ray;


    /// <summary>
    /// Spawns a Spawn Card Prefab at a random location on a NavMesh relative to the object the mesh was built upon.
    /// </summary>
    /// <param name="spawnLocations">The possible locations that the prefab can spawn at.</param>
    /// <param name="occupiedLocations">The locations that are currently occupied by a spawned prefab.</param>
    /// <param name="spawnCard">The spawn card.</param>
    /// <param name="minRandomSpawnRadius">The minimum area around the center point to try find a random point.</param>
    /// <param name="maxRandomSpawnRadius">The maximum area around the center point to try find a random point.</param>
    /// <param name="randomSpawnQueryRadius">The area around a generated random point to try and connect to a NavMeshSurface</param>
    /// <param name="layerMask">The Layer used for a physics raycast when determing the ground normal of a found random position.</param>
    /// <param name="instantiatedGameObject">The gameobject that was instantiated - if a random position was successfully found.</param>
    /// <param name="locationId">The spawn location id that the spawn card prefab was instantiated at.</param>
    /// <param name="maxIterations">The amount of iterations to find a valid spawn position before failing.</param>
    /// <returns>true, when successfuly spawning a spawn card prefab at a random location; otherwise false.</returns>

    protected bool SpawnAtRandomPosition(
        List<Vector3> spawnLocations,
        bool[] occupiedLocations,
        SpawnCard spawnCard,
        float minRandomSpawnRadius,
        float maxRandomSpawnRadius,
        float randomSpawnQueryRadius,
        LayerMask layerMask,
        out GameObject instantiatedGameObject,
        out int locationId,
        byte maxIterations = 16
    )
    {

        int occupiedLocationsLength = occupiedLocations.Length;

        for(byte i = 0; i < maxIterations; i++)
        {
            locationId = Random.Range(0, occupiedLocationsLength);

            if(occupiedLocations[locationId] == true)
            {
                continue;
            }

            if(SpawnAtRandomPosition(
                spawnLocations[locationId],
                spawnCard,
                minRandomSpawnRadius,
                maxRandomSpawnRadius,
                randomSpawnQueryRadius,
                layerMask,
                out instantiatedGameObject
            ) == true)
            {
                occupiedLocations[locationId] = true;
                return true;
            }
        }

        instantiatedGameObject = null;
        locationId = -1;
        return false;
    }

    /// <summary>
    /// Spawns a Spawn Card Prefab at a random location on a NavMesh relative to the object the mesh was built upon.
    /// </summary>
    /// <param name="spawnLocations">The possible locations that the prefab can spawn at.</param>
    /// <param name="spawnCard">The spawn card.</param>
    /// <param name="minRandomSpawnRadius">The minimum area around the center point to try find a random point.</param>
    /// <param name="maxRandomSpawnRadius">The maximum area around the center point to try find a random point.</param>
    /// <param name="randomSpawnQueryRadius">The area around a generated random point to try and connect to a NavMeshSurface</param>
    /// <param name="layerMask">The Layer used for a physics raycast when determing the ground normal of a found random position.</param>
    /// <param name="instantiatedGameObject">The gameobject that was instantiated - if a random position was successfully found.</param>
    /// <param name="maxIterations">The amount of iterations to find a valid spawn position before failing.</param>
    /// <returns>true, when successfully spawning a spawn card prefab at a random location; otherwise false.</returns>

    protected bool SpawnAtRandomPosition(
        List<Vector3> spawnLocations, 
        SpawnCard spawnCard, 
        float minRandomSpawnRadius,
        float maxRandomSpawnRadius,
        float randomSpawnQueryRadius,
        LayerMask layerMask,
        out GameObject instantiatedGameObject,
        byte maxIterations = 16
    )
    {
        // use a random spawn location.

        int rand = UnityEngine.Random.Range(0, spawnLocations.Count);
        Vector3 origin = spawnLocations[rand];

        return SpawnAtRandomPosition(
            origin, 
            spawnCard,
            minRandomSpawnRadius,
            maxRandomSpawnRadius,
            randomSpawnQueryRadius,
            layerMask,
            out instantiatedGameObject,
            maxIterations
        );
    }

    /// <summary>
    /// Spawns a Spawn Card Prefab at a random location on a NavMesh relative to the object the mesh was built upon.
    /// </summary>
    /// <param name="origin">The point in world-space to find a random position around.</param>
    /// <param name="spawnCard">The spawn card.</param>
    /// <param name="minRandomSpawnRadius">The minimum area around the center point to try find a random point.</param>
    /// <param name="maxRandomSpawnRadius">The maximum area around the center point to try find a random point.</param>
    /// <param name="randomSpawnQueryRadius">The area around a generated random point to try and connect to a NavMeshSurface</param>
    /// <param name="layerMask">The Layer used for a physics raycast when determing the ground normal of a found random position.</param>
    /// <param name="instantiatedGameObject">The gameobject that was instantiated - if a random position was successfully found.</param>
    /// <param name="maxIterations">The amount of iterations to find a valid spawn position before failing.</param>
    /// <returns>true, when successfully spawning a spawn card prefab at a random location; otherwise false.</returns>

    protected bool SpawnAtRandomPosition(
        Vector3 origin, 
        SpawnCard spawnCard, 
        float minRandomSpawnRadius,
        float maxRandomSpawnRadius,
        float randomSpawnQueryRadius,
        LayerMask layerMask,
        out GameObject instantiatedGameObject,
        byte maxIterations = 16
    )
    {
        
        instantiatedGameObject = null;


        // find a valid random point at the spawn location.

        if(Entropek.UnityUtils.NavMeshUtils.GetRandomPoint(
            spawnCard.GetNavMeshQueryFilter(),
            origin,
            minRandomSpawnRadius,
            maxRandomSpawnRadius,
            randomSpawnQueryRadius,
            out NavMeshHit point,
            maxIterations
        ) == false)
        {
            return false;
        }

        // ray cast to down to get the floor hit point and normals to spawn at
        // ; as the GetRandomPoint() uses navmesh queries that are inaccurate.

        ray = new Ray(point.position + Vector3.up, Vector3.down);
        if(Physics.RaycastNonAlloc(ray, hits, float.MaxValue, layerMask) == 0)
        {
            return false;
        }

        Debug.DrawLine(point.position, origin, Color.green, 200);

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
}
