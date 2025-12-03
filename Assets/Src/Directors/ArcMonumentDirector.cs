using System;
using System.Diagnostics;
using Entropek.Time;
using Entropek.UnityUtils;
using UnityEngine;

public class ArcMonumentDirector : PointOfInterestDirector
{
    [SerializeField] private LoopedTimer spawnEventTimer;


    /// 
    /// Base.
    /// 


    protected override void Awake()
    {
        spawnEventTimer.Begin();
        LinkEvents();    
        base.Awake();
        TestSpawn(128);
    }

    protected override void OnDestroy()
    {
        UnlinkEvents();
        base.OnDestroy();
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
        PlaythroughStopwatch.Singleton.Started -= OnPlaythroughStopwatchStarted;
        PlaythroughStopwatch.Singleton.Stopped -= OnPlaythroughStopwatchStopped;
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
        // TestSpawn(12);
    }

}