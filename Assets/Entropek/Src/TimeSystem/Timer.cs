using System;
using System.IO;
using UnityEngine;

namespace Entropek.Time{
    
    [Serializable]
    public class Timer : MonoBehaviour{

        public event Action Timeout;
        public event Action Began;
        public event Action Halted;
        public event Action Paused;
        public event Action Resumed;




        [Header("Data")]
        [SerializeField] private float initialTime;
        public float InitialTime => initialTime;
        
        private float currentTime;
        public float CurrentTime => currentTime;
        
        private float normalisedCurrentTime;
        public float NormalisedCurrentTime => normalisedCurrentTime;
        
        public bool BeginOnAwake;
        public bool Loop;
        public bool HasTimedOut => CurrentTime == 0;

        #if UNITY_EDITOR
        [Header("Editor")]
        public string EditorNameTag;
        #endif

        void OnEnable(){
            TimerManager.Singleton.RegisterTimer(this);
            if(BeginOnAwake==true){
                Begin();
            }
        }

        void OnDisable(){        
            TimerManager.Singleton.DeregisterTimer(this);
        }

        public void Tick(){
            currentTime -= UnityEngine.Time.deltaTime;
            normalisedCurrentTime = currentTime / initialTime;
            if(CurrentTime <= 0){
                currentTime = 0;
                normalisedCurrentTime = 0;
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
            if(TimerManager.Singleton.BeginTimer(this)==true){
                Began?.Invoke();
            }
        }

        public void Halt(){
            if(TimerManager.Singleton.HaltTimer(this)==true){
                Halted?.Invoke();
            }
        }

        public void Pause(){
            if(TimerManager.Singleton.PauseTimer(this)==true){
                Paused?.Invoke();
            }
        }

        public void Resume(){
            if(TimerManager.Singleton.BeginTimer(this)==true){
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

