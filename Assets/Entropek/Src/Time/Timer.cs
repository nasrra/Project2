using System;
using System.IO;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Time{
    
    [Serializable]
    public abstract class  Timer : MonoBehaviour{


        /// 
        /// Callbacks.
        /// 


        public event Action Timeout;
        public event Action Began;
        public event Action Halted;
        public event Action Paused;
        public event Action Resumed;


        /// <summary>
        /// This Editor specific code is here just so timers can have a name
        /// associated with them to known which timer is which in the editor.
        /// It is stored here as the gameObject/prefab can easily store its value,
        /// meaning I dont have to write any complex editor code for serialisation :).
        /// Do NOT use this anywhere outside of editor code.
        /// </summary>


#if UNITY_EDITOR
        [Header("Editor Name Tag")]
        [SerializeField]private string editorNameTag;
        public string EditorNameTag => editorNameTag;
#endif


        /// 
        /// Data
        /// 


        [Header(nameof(Timer))]
        [SerializeField] protected float initialTime;
        public float InitialTime => initialTime;
        
        [RuntimeField] protected float currentTime;
        public float CurrentTime => currentTime;
        
        [RuntimeField] protected float normalisedCurrentTime;
        public float NormalisedCurrentTime => normalisedCurrentTime;

        [RuntimeField] protected TimerState state = TimerState.Halted; // default state for any timer is alaways halted.
        public TimerState State => state;

        [SerializeField] public bool BeginOnEnable;
        public bool HasTimedOut => CurrentTime == 0;

        private bool registered = false;


        /// 
        /// Base.
        /// 

        protected virtual void Awake()
        {
            RegisterInTimerManager();
        }

        protected virtual void OnEnable()
        {
            if (BeginOnEnable == true)
            {
                Begin();
            }
        }

        protected virtual void OnDisable(){        
        }

        protected virtual void OnDestroy()
        {
            DeregisterFromTimerManager();
        }


        /// 
        /// Functions.
        /// 


        /// <summary>
        /// Registers this Timer in the TimerManager Singleton.
        /// </summary>

        /// Note:
        /// This function should be called in any public function to ensure
        /// this timer is registered in at any stage of initialisation.
        /// A parent object calling Begin on this in their Awake function may
        /// skip this Timers Awake function; meaning the timer is never registered. 

        private void RegisterInTimerManager()
        {
            if (registered == false)
            {
                TimerManager.Singleton.RegisterTimer(this);
                registered = true;
            }
        }

        /// <summary>
        /// Deregisters this Timer from the TimerManager Singleton.
        /// </summary>

        private void DeregisterFromTimerManager()
        {
            if(registered == true)
            {
                // Lazy deregistering to avoid errors when unity destroys objects at random when exiting editor playmode.
                // This is fine as TimerManagerSingleton uses the Bootstrap pattern; if that changes then this will have to
                // as well :)

                TimerManager.Singleton?.DeregisterTimer(this);
                registered = false;
            }
        }


        /// <summary>
        /// Increments down the current time value stored by this timer by Time.deltaTime.
        /// </summary>

        public void Tick()
        {
            currentTime -= UnityEngine.Time.deltaTime * UnityEngine.Time.timeScale;
            normalisedCurrentTime = currentTime / initialTime;
            if (CurrentTime <= 0)
            {
                currentTime = 0;
                normalisedCurrentTime = 0;
                OnBeforeTimeout();
                Timeout?.Invoke();
                OnAfterTimeout();
            }
        }

        /// <summary>
        /// Starts the tick loop for this timer, setting the current time value to a new intial time value.
        /// </summary>
        /// <param name="time">The initial time to start the timer at.</param>

        public void Begin(float time)
        {
            if (SetInitialTime(time) == true)
            {
                Begin();
            }
        }

        /// <summary>
        /// Starts the tick loop for this timer, resetting the current time value to the initial time value.
        /// </summary>

        public void Begin()
        {
            RegisterInTimerManager();
            if (TimerManager.Singleton.BeginTimer(this) == true)
            {
                Reset();
                state = TimerState.Active;
                Began?.Invoke();
            }
        }

        /// <summary>
        /// Stops the tick loop for this timer, setting the current time value to zero.
        /// </summary>

        public void Halt()
        {
            RegisterInTimerManager();
            if(TimerManager.Singleton.HaltTimer(this))
            {
                state = TimerState.Halted;
                currentTime = 0;
                Halted?.Invoke();
            }
        }

        /// <summary>
        /// Stops the tick loop for this timer, retaining the current time value.
        /// </summary>

        public void Pause()
        {
            RegisterInTimerManager();
            if (TimerManager.Singleton.PauseTimer(this) == true)
            {
                state = TimerState.Paused;
                Paused?.Invoke();

            }
        }

        /// <summary>
        /// Continues the tick loop for this timer, resuming from the current time value.
        /// </summary>

        public void Resume()
        {
            RegisterInTimerManager();
            if (TimerManager.Singleton.BeginTimer(this) == true)
            {
                state = TimerState.Active;
                Resumed?.Invoke();
            }
        }

        /// <summary>
        /// Resets the current time value to the initial time value.
        /// </summary>

        public virtual void Reset()
        {
            currentTime = initialTime;
        }

        /// <summary>
        /// Sets the initial time value to countdown from in this timer. 
        /// </summary>
        /// <param name="time">The specified value to set initial time to.</param>
        /// <returns>true, if successfully set, false if failed.</returns>
        /// <exception cref="InvalidDataException">if the specified time value is less than zero.</exception>

        public bool SetInitialTime(float time)
        {
            if (time < 0)
            {
                throw new InvalidDataException("Timer cannot have an initial time of less than zero");
            }
            initialTime = time;
            return true;
        }

        protected abstract void OnAfterTimeout();
        protected abstract void OnBeforeTimeout();
    }


}

