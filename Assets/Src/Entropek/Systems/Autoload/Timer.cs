using System;
using System.IO;
using UnityEngine;

namespace Entropek.Systems.Autoload{
    
[Serializable]
public class Timer : MonoBehaviour{

    public event Action Timeout;
    public event Action Began;
    public event Action Halted;
    public event Action Paused;
    public event Action Resumed;
    
    #if UNITY_EDITOR
    [Header("Editor")]
    [SerializeField]private string editorTag;
    #endif

    [Header("Data")]
    [SerializeField] private float initialTime;
    public float InitialTime => initialTime;
    [SerializeField] private float currentTime;
    public float CurrentTime => currentTime;
    public bool BeginOnAwake;
    public bool Loop;

    void OnEnable(){
        TimerManager.RegisterTimer(this);
        if(BeginOnAwake==true){
            Begin();
        }
    }

    void OnDisable(){        
        TimerManager.DeregisterTimer(this);
    }

    public void Tick(){
        currentTime -= Time.deltaTime;
        if(CurrentTime <= 0){
            currentTime = 0;
            Timeout?.Invoke();
            if(Loop==false){
                Halt();
            }
            else{
                Begin();
            }
        }
    } 

    public void Begin(float time){
        if(SetInitialTime(time)==true){
        Begin();
        }
    }

    public void Begin(){
        Reset();
        if(TimerManager.BeginTimer(this)==true){
            Began?.Invoke();
        }
    }

    public void Halt(){
        if(TimerManager.HaltTimer(this)==true){
            Halted?.Invoke();
        }
    }

    public void Pause(){
        if(TimerManager.PauseTimer(this)==true){
            Paused?.Invoke();
        }
    }

    public void Resume(){
        if(TimerManager.BeginTimer(this)==true){
            Resumed?.Invoke();
        }
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

}


}

