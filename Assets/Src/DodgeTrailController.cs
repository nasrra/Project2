using Entropek.Systems.Autoload;
using UnityEngine;

public class DodgeTrailController : MonoBehaviour{
        
    [Header("Components")]
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Timer idleTimer;
    [SerializeField] private Timer snapbackTimer;
    
    [Header("Data")]
    [SerializeField] private float trailMaxTime;
    private bool snapback;


    /// 
    /// Base.
    /// 


    private void OnEnable(){
        LinkEvents();
    }

    private void FixedUpdate(){
        if(snapback==true && snapbackTimer.NormalisedCurrentTime > 0){
            trail.time = trailMaxTime * snapbackTimer.NormalisedCurrentTime;
        }
    }

    private void OnDisable(){
        UnlinkEvents();
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        LinkTimerEvents();
    }

    private void UnlinkEvents(){
        UnlinkTimerEvents();
    }

    public void EnableTrail(){
        idleTimer.Begin();
        trail.time = trailMaxTime;
    }

    public void SnapbackTrail(){
        if(idleTimer.HasTimedOut == false){
            return;
        }
        snapback = true;
        snapbackTimer.Begin();
    }

    private void LinkTimerEvents(){
        idleTimer.Timeout += OnIdleTimeout;
        snapbackTimer.Timeout += OnSnapbackTimeout;
    }

    private void UnlinkTimerEvents(){
        idleTimer.Timeout -= OnIdleTimeout;
        snapbackTimer.Timeout -= OnSnapbackTimeout;    
    }

    private void OnIdleTimeout(){
        SnapbackTrail();
    }

    private void OnSnapbackTimeout(){
        snapback = false;
        trail.time = 0;
    }
}
