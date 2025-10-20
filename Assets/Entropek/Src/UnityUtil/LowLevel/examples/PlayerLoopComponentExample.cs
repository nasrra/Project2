using System;
using System.IO;
using UnityEngine;

///
/// This is an example class for a custom player loop component.
/// 

namespace Entropek.UnityUtils.LowLevel.Examples{
    
[Serializable]
public class PlayerLoopComponentExample : IDisposable{

    public event Action Timeout;

    [HideInInspector] private float currentTime;
    public float CurrentTime => currentTime;
    
    [SerializeField] private float initialTime;
    public float InitialTime => initialTime;

    public uint Id {get;private set;}

    public PlayerLoopComponentExample(float time){
        SetInitialTime(time);
        Initialise();
    }

    public PlayerLoopComponentExample(){
        Initialise();
    }

    private void Initialise(){
        
        // dont initialise if this is being serialized in the editor.

        Debug.Log("init timer");

        Id = PlayerLoopSystemExample.idGenerator.GenerateId();
        PlayerLoopSystemExample.RegisterTimer(this);
    }

    public void Tick(){
        currentTime -= Time.deltaTime;
        if(CurrentTime <= 0){
            Stop();
            Timeout?.Invoke();
        }
    } 

    public void Start(float time){
        SetInitialTime(time);
        PlayerLoopSystemExample.StartTimer(this);
    }

    public void Start(){
        Reset();
        PlayerLoopSystemExample.StartTimer(this);
    }

    public void Stop(){
        PlayerLoopSystemExample.StopTimer(this);
    }

    public void Pause(){
        PlayerLoopSystemExample.PauseTimer(this);
    }

    public void Resume(){
        PlayerLoopSystemExample.StartTimer(this);
    }

    public void Reset(){
        currentTime = initialTime;
    }

    private bool SetInitialTime(float time){
        if(time < 0){
            throw new InvalidDataException("Timer cannot have an initial time of less than zero");
        }
        initialTime = time;
        return true;
    }


    /// 
    /// Destruction.
    /// 


    bool disposed;

    ~PlayerLoopComponentExample(){

        // only deregister if we are explicitly disposing.
        // the timers strong reference should only ever be within the TimeManager.
        // so the finaliser should only 

        Dispose(false);
    }

    public void Dispose(){
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing){
        if(disposed==true){
            return;
        }

        if(disposing==true){
            PlayerLoopSystemExample.DeregisterTimer(this);
            PlayerLoopSystemExample.idGenerator.FreeId(Id);
        }

        Timeout = null;
        disposed = true;
    }
}


}

