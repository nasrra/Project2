using System.Collections.Generic;
using Entropek.Collections;
using UnityEditor;
using UnityEngine;

namespace Entropek.Time{

    public class TimerManager : MonoBehaviour{


        /// 
        /// Data.
        /// 


        // public static UniqueIdGenerator idGenerator {get; private set;} = new UniqueIdGenerator();

        public static TimerManager Singleton { get; private set; }

        SwapbackList<Timer> activeTimers = new SwapbackList<Timer>();
        SwapbackList<Timer> pausedTimers = new SwapbackList<Timer>();
        SwapbackList<Timer> haltedTimers = new SwapbackList<Timer>();

        Dictionary<int, int> activeTimersIdToListIndexMap    = new Dictionary<int, int>();
        Dictionary<int, int> pausedTimersIdToListIndexMap    = new Dictionary<int, int>();
        Dictionary<int, int> haltedTimersIdToListIndexMap    = new Dictionary<int, int>();


        /// 
        /// Public Functions.
        /// 


        public void RegisterTimer(Timer timer){
            // timers are defaulted to stop.
            AddTimerToHaltedList(timer);
        }

        public void DeregisterTimer(Timer timer){
            
            if(IsHalted(timer)==true){
                RemoveTimerFromHaltedList(timer);
            }
            else if(IsActive(timer)==true){
                RemoveTimerFromActiveList(timer);
            }
            else if(IsPaused(timer)==true){
                RemoveTimerFromPausedList(timer);
            }
        }

        public bool BeginTimer(Timer timer){
            
            // short-circuit if the timer is already started.

            if(IsActive(timer)==true){
                return false;
            }

            // check and handle if the timer is in another state.
            
            if(IsPaused(timer)==true){
                RemoveTimerFromPausedList(timer);
            }
            else if(IsHalted(timer)==true){
                RemoveTimerFromHaltedList(timer);
            }

            AddTimerToActiveList(timer);

            return true;
        }

        public bool HaltTimer(Timer timer){
            
            // short-circuit if the timer is already stopped.

            if(IsHalted(timer)==true){
                return false;
            }

            // check and handle if the timer is in another state.
            
            if(IsPaused(timer)==true){
                RemoveTimerFromPausedList(timer);
            }
            else if(IsActive(timer)==true){
                RemoveTimerFromActiveList(timer);
            }

            AddTimerToHaltedList(timer);

            return true;
        }

        public bool PauseTimer(Timer timer){
        
            // short-circuit if the timer is already stopped.

            if(IsPaused(timer)==true){
                return false;
            }

            // check and handle if the timer is in another state.
            
            if(IsHalted(timer)==true){
                RemoveTimerFromHaltedList(timer);
            }
            else if(IsActive(timer)==true){
                RemoveTimerFromActiveList(timer);
            }

            AddTimerToPausedList(timer);
        
            return true;
        }


        /// 
        /// Timer State Checking.
        /// 


        public bool IsActive(Timer timer){
            return activeTimersIdToListIndexMap.ContainsKey(timer.GetInstanceID());
        }

        public bool IsHalted(Timer timer){
            return haltedTimersIdToListIndexMap.ContainsKey(timer.GetInstanceID());
        }

        public bool IsPaused(Timer timer){
            return pausedTimersIdToListIndexMap.ContainsKey(timer.GetInstanceID());
        }


        /// 
        /// Add Timer.
        /// 


        void AddTimerToActiveList(Timer timer){
            AddTimerToList(activeTimers, activeTimersIdToListIndexMap, timer);
        }

        void AddTimerToHaltedList(Timer timer){
            AddTimerToList(haltedTimers, haltedTimersIdToListIndexMap, timer);
        }

        void AddTimerToPausedList(Timer timer){
            AddTimerToList(pausedTimers, pausedTimersIdToListIndexMap, timer);
        }

        void AddTimerToList(SwapbackList<Timer> timerList, Dictionary<int, int> idToArrayIndexMap, Timer timerToAdd){
            timerList.Add(timerToAdd);
            idToArrayIndexMap.Add(timerToAdd.GetInstanceID(), timerList.Count-1);
        }


        /// 
        /// Remove Timer.
        /// 


        void RemoveTimerFromActiveList(Timer timer){
            RemoveTimerFromList(activeTimers, activeTimersIdToListIndexMap, timer);
        }

        void RemoveTimerFromHaltedList(Timer timer){
            RemoveTimerFromList(haltedTimers, haltedTimersIdToListIndexMap, timer);
        }

        void RemoveTimerFromPausedList(Timer timer){
            RemoveTimerFromList(pausedTimers, pausedTimersIdToListIndexMap, timer);
        }

        void RemoveTimerFromList(SwapbackList<Timer> timerList, Dictionary<int, int> idToArrayIndexMap, Timer timerToRemove){
            
            #if UNITY_EDITOR

            // this check is here as the timer manager is cleared before the timers
            // themselves are freed from memeory. The timers try to remove themselves
            // from the list that doesnt contain them anymore.

            // if(UnityUtils.Editor.InPlaymode==false){
            //     return;
            // }

            #endif

            // swap the indices of the last entry and entry to remove.
            // this is done becuase timers are stored in a swapbacklist.
            
            int timerToRemoveIndex = idToArrayIndexMap[timerToRemove.GetInstanceID()];
            int lastIndex = timerList.Count - 1;
            Timer lastTimer = timerList[lastIndex];

            // set the timer to removes array data to the last entries data.

            timerList[timerToRemoveIndex] = lastTimer;
            idToArrayIndexMap[lastTimer.GetInstanceID()] = timerToRemoveIndex; 

            // then remove the last entry from the list.
            
            idToArrayIndexMap.Remove(timerToRemove.GetInstanceID());
            timerList.RemoveAt(lastIndex);
        }


        /// 
        /// Player Loop Functions.
        /// 


        void Update(){

            // tick down all available timers.
            
            for(int i = 0; i < activeTimers.Count; i++){
                Timer timer = activeTimers[i];
                timer.Tick();
            }
        }

        private void Clear()
        {
            activeTimers.Clear();
            activeTimersIdToListIndexMap.Clear();
            haltedTimers.Clear();
            haltedTimersIdToListIndexMap.Clear();
            pausedTimers.Clear();
            pausedTimersIdToListIndexMap.Clear();
        }

        void OnDestroy()
        {
            Clear();
        }


        /// 
        /// Bootstrapper - inject this to Unity PlayerLoop.
        /// 


        static class Bootstrap{    
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            internal static void Initialise(){
                GameObject main = new();
                main.name = "Timer Manager";
                main.AddComponent<TimerManager>();
                DontDestroyOnLoad(main);
                Singleton = main.GetComponent<TimerManager>();
            }
        }

    }

}

