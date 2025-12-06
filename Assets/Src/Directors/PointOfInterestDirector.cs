using System;
using System.Collections.Generic;
using Entropek.Time;
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
    [SerializeField] private int spawnAmount;
    [SerializeField] protected SpawnCard spawnCard;
    [SerializeField] private LoopedTimer spawnEventTimer;

    [RuntimeField] protected LayerMask navMeshLayerMask;
    [RuntimeField] protected List<Vector3> spawnLocations = new();
    [RuntimeField] protected bool[] occupiedLocations;


    protected virtual void Awake()
    {
        Initialise();
        spawnEventTimer.Begin();
        LinkEvents();
        Spawn(spawnAmount);
    }

    private void Initialise()
    {        
        InitialiaseLayerMask();
        InitialiseSpawnLocations();
        InitialiseOccupiedLocations();
    }

    protected virtual void OnDestroy()
    {
        UnlinkEvents();
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

    public bool SpawnPointOfInterestAtRandomPosition(){
        if(SpawnAtRandomPosition(
            spawnLocations,
            occupiedLocations,
            spawnCard,
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


    /// <summary>
    /// The callback for a Point Of Interest's Destroy event; marking its occupied location as free for other
    /// Point of Interests to spawn at.
    /// </summary>
    /// <param name="locationId">The id of the location in the internal array where this Point of Interst is occupying.</param>

    private void OnPoiDestroyed(int locationId)
    {
        occupiedLocations[locationId] = false;
    }

    protected void Spawn(int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            SpawnPointOfInterestAtRandomPosition();
        }
    }
   

    /// 
    /// Event Linkage.
    /// 


    private void LinkEvents()
    {
        LinkPlaythroughStopwatchEvents();
        LinkTimerEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkPlaythroughStopwatchEvents();
        UnlinkTimerEvents();
    }


    /// 
    /// PlaythroughStopwatch Event Linkage.
    /// 


    private void LinkPlaythroughStopwatchEvents()
    {        
        PlaythroughStopwatch.Singleton.Started += OnPlaythroughStopwatchStarted;
        PlaythroughStopwatch.Singleton.Stopped += OnPlaythroughStopwatchStopped;
    }

    private void UnlinkPlaythroughStopwatchEvents()
    {
        if (PlaythroughStopwatch.Singleton != null)
        {            
            PlaythroughStopwatch.Singleton.Started -= OnPlaythroughStopwatchStarted;
            PlaythroughStopwatch.Singleton.Stopped -= OnPlaythroughStopwatchStopped;
        }
    }

    private void OnPlaythroughStopwatchStarted()
    {
        spawnEventTimer.Begin();
    }

    private void OnPlaythroughStopwatchStopped()
    {
        spawnEventTimer.Halt();
    }


    /// 
    /// Timer Event Linkage.
    /// 


    private void LinkTimerEvents()
    {
        spawnEventTimer.Timeout += OnSpawnEventTimerTimeout;
    }

    private void UnlinkTimerEvents()
    {
        spawnEventTimer.Timeout -= OnSpawnEventTimerTimeout;        
    }

    private void OnSpawnEventTimerTimeout()
    {
        Spawn(spawnAmount);
    }

#if UNITY_ENGINE

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

#endif

}
