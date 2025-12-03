using System;
using System.Collections.Generic;
using Entropek.UnityUtils;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

public class PointOfInterestDirector : Director
{
    [SerializeField] private int navMeshSurfaceId;
    [SerializeField] private float minRandomSpawnRadius = 1;
    [SerializeField] private float maxRandomSpawnRadius = 24;
    [SerializeField] private float randomSpawnQueryRadius = 0.5f;
    [SerializeField] private float duplicateThresholdSqrd = 3.33f;
    [SerializeField] protected SpawnCard[] spawnCards;

    [RuntimeField] protected LayerMask navMeshLayerMask;
    [RuntimeField] protected List<Vector3> spawnLocations = new();
    [RuntimeField] protected bool[] occupiedLocations;


    protected virtual void Awake()
    {
        Initialise();
    }

    private void Initialise()
    {        
        InitialiaseLayerMask();
        InitialiseSpawnLocations();
        InitialiseOccupiedLocations();
    }

    protected virtual void OnDestroy()
    {
        
    }

    private void InitialiaseLayerMask()
    {
        navMeshLayerMask = NavMeshSurfaceManager.Singleton.GetNavMeshSurfaceLayerMask(navMeshSurfaceId);   
    }

    private void InitialiseSpawnLocations()
    {
        Vector3[] midPoints = NavMeshSurfaceManager.Singleton.GetNavMeshSurfaceMidpoints(navMeshSurfaceId);

        // remove any duplicate data.

        for(int i = 0; i < midPoints.Length; i++)
        {
            bool duplicate = false;
            for(int j = i+1; j < midPoints.Length; j++)
            {
                if((midPoints[i] - midPoints[j]).sqrMagnitude < duplicateThresholdSqrd)
                {
                    duplicate = true;
                    break;
                }
            }
            if (duplicate == false)
            {
                spawnLocations.Add(midPoints[i]);
            }
        }
    }

    private void InitialiseOccupiedLocations()
    {
        occupiedLocations = new bool[spawnLocations.Count];
    }

    public bool SpawnPointOfInterestAtRandomPosition(
        int spawnCardId
    ){
        if(SpawnAtRandomPosition(
            spawnLocations,
            occupiedLocations,
            spawnCards[spawnCardId],
            minRandomSpawnRadius,
            maxRandomSpawnRadius,
            randomSpawnQueryRadius,
            navMeshLayerMask,
            out GameObject instantiatedGameObject,
            out int locationId,
            64
        ) == true)
        {
            PointOfInterst poi = instantiatedGameObject.GetComponent<PointOfInterst>();
            poi.LocationId = locationId;
            poi.Destroyed += OnPoiDestroyed; // Destroyed callback is set to null on destroy; so this does not need an unsubscirption.
            occupiedLocations[locationId] = true;
            return true;
        }

        return false;
    }

    private void OnPoiDestroyed(int locationId)
    {
        occupiedLocations[locationId] = false;
    }

    protected void TestSpawn(int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            SpawnPointOfInterestAtRandomPosition(0);
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.IsPlaying(this) == false)
        {
            return;
        }
        
        Gizmos.color = Color.white;
        
        for(int i = 0; i < spawnLocations.Count; i++)
        {                
            Gizmos.DrawCube(spawnLocations[i], Vector3.one);
        }
    }
}
