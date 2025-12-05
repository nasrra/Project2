using System;
using System.Collections.Generic;
using Entropek.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Entropek.Time{

    [DefaultExecutionOrder(-10)]
    public class TimerManager : MonoBehaviour
    {


        /// 
        /// Data.
        /// 


        // public static UniqueIdGenerator idGenerator {get; private set;} = new UniqueIdGenerator();

        public static TimerManager Singleton { get; private set; }

        public SwapbackList<Timer> ActiveTimers { get; private set; } = new();
        public SwapbackList<Timer> PausedTimers { get; private set; } = new();
        public SwapbackList<Timer> HaltedTimers { get; private set; } = new();

        Dictionary<int, int> activeTimersIdToListIndexMap = new Dictionary<int, int>();
        Dictionary<int, int> pausedTimersIdToListIndexMap = new Dictionary<int, int>();
        Dictionary<int, int> haltedTimersIdToListIndexMap = new Dictionary<int, int>();


        /// 
        /// Public Functions.
        /// 


        public void RegisterTimer(Timer timer)
        {
            // timers are defaulted to stop.
            AddTimerToHaltedList(timer);
        }

        public void DebugTimerState(Timer timer)
        {
            if(GetTimerListState(timer, out TimerState timerState) == false)
            {
                return;
            }
            
            switch (timerState)
            {
                case TimerState.Active:
                    Debug.Log("a");
                    break;
                case TimerState.Halted:
                    Debug.Log("h");
                    break;
                case TimerState.Paused:
                    Debug.Log("p");
                    break;
            }
        }

        public void DeregisterTimer(Timer timer)
        {
            if(GetTimerListState(timer, out TimerState timerState) == false)
            {
                return;
            }
            
            switch (timerState)
            {
                case TimerState.Active:
                    RemoveTimerFromActiveList(timer);
                    break;
                case TimerState.Halted:
                    RemoveTimerFromHaltedList(timer);
                    break;
                case TimerState.Paused:
                    RemoveTimerFromPausedList(timer);
                    break;
            }
        }

        public bool BeginTimer(Timer timer)
        {
            if(GetTimerListState(timer, out TimerState timerState) == false)
            {
                return false;
            }
            
            switch (timerState)
            {
                case TimerState.Active:
                    return true; // already began (but return true as Begin may reset the initial time of the timer.)
                case TimerState.Halted:
                    RemoveTimerFromHaltedList(timer);
                    break;
                case TimerState.Paused:
                    RemoveTimerFromPausedList(timer);
                    break;
            }

            AddTimerToActiveList(timer);
            return true;
        }

        public bool HaltTimer(Timer timer)
        {
            if(GetTimerListState(timer, out TimerState timerState) == false)
            {
                return false;
            }
            
            switch (timerState)
            {
                case TimerState.Active:
                    RemoveTimerFromActiveList(timer);
                    break;
                case TimerState.Halted:
                    return false; // timer is already halted.
                case TimerState.Paused:
                    RemoveTimerFromPausedList(timer);
                    break;
            }

            AddTimerToHaltedList(timer);
            return true;
        }

        public bool PauseTimer(Timer timer)
        {
            if(GetTimerListState(timer, out TimerState timerState) == false)
            {
                return false;
            }
            
            switch (timerState)
            {
                case TimerState.Active:
                    RemoveTimerFromActiveList(timer);
                    break;
                case TimerState.Halted:
                    RemoveTimerFromHaltedList(timer);
                    break;
                case TimerState.Paused:
                    return false; // timer is already paused.
            }
            
            AddTimerToPausedList(timer);
            return true;
        }


        /// 
        /// Timer State Checking.
        /// 


        /// <summary>
        /// Gets what timer state a timer would be, depending upon the list it is stored in.
        /// </summary>
        /// <param name="timer">The timer to check.</param>
        /// <returns>The timer state of the given timer.</returns>
        /// <exception cref="InvalidOperationException"></exception>

        private bool GetTimerListState(Timer timer, out TimerState timerState)
        {
            if (activeTimersIdToListIndexMap.ContainsKey(timer.GetInstanceID()))
            {
                timerState = TimerState.Active;
                return true;
            }
            else if (haltedTimersIdToListIndexMap.ContainsKey(timer.GetInstanceID()))
            {
                timerState = TimerState.Halted;
                return true;
            }
            else if (pausedTimersIdToListIndexMap.ContainsKey(timer.GetInstanceID()))
            {
                timerState = TimerState.Paused;
                return true;
            }

            timerState = TimerState.Halted;
            return false;

            #if UNITY_EDITOR

            throw new InvalidOperationException($"{timer.name +" "+timer.EditorNameTag} timer with intstance id ({timer.GetInstanceID()}) has not been registered in this TimerManager.");
        
            #endif
        }

        /// 
        /// Add Timer.
        /// 


        void AddTimerToActiveList(Timer timer)
        {
            AddTimerToList(ActiveTimers, activeTimersIdToListIndexMap, timer);
        }

        void AddTimerToHaltedList(Timer timer)
        {
            AddTimerToList(HaltedTimers, haltedTimersIdToListIndexMap, timer);
        }

        void AddTimerToPausedList(Timer timer)
        {
            AddTimerToList(PausedTimers, pausedTimersIdToListIndexMap, timer);
        }

        void AddTimerToList(SwapbackList<Timer> timerList, Dictionary<int, int> idToArrayIndexMap, Timer timerToAdd)
        {
            timerList.Add(timerToAdd);
            idToArrayIndexMap.Add(timerToAdd.GetInstanceID(), timerList.Count - 1);
        }


        /// 
        /// Remove Timer.
        /// 


        void RemoveTimerFromActiveList(Timer timer)
        {
            RemoveTimerFromList(ActiveTimers, activeTimersIdToListIndexMap, timer);
        }

        void RemoveTimerFromHaltedList(Timer timer)
        {
            RemoveTimerFromList(HaltedTimers, haltedTimersIdToListIndexMap, timer);
        }

        void RemoveTimerFromPausedList(Timer timer)
        {
            RemoveTimerFromList(PausedTimers, pausedTimersIdToListIndexMap, timer);
        }

        void RemoveTimerFromList(SwapbackList<Timer> timerList, Dictionary<int, int> idToArrayIndexMap, Timer timerToRemove)
        {

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


        void Update()
        {

            // tick down all available timers.

            for (int i = 0; i < ActiveTimers.Count; i++)
            {
                Timer timer = ActiveTimers[i];
                timer.Tick();
            }
        }

        private void Clear()
        {
            ActiveTimers.Clear();
            activeTimersIdToListIndexMap.Clear();
            HaltedTimers.Clear();
            haltedTimersIdToListIndexMap.Clear();
            PausedTimers.Clear();
            pausedTimersIdToListIndexMap.Clear();
        }

        void OnDestroy()
        {
            Clear();
            Singleton = null;
        }


        /// 
        /// Bootstrapper
        /// 


        static class Bootstrap
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            private static void Initialise()
            {
                GameObject main = new();
                main.name = nameof(TimerManager);
                main.AddComponent<TimerManager>();
                DontDestroyOnLoad(main);
                Singleton = main.GetComponent<TimerManager>();
            }
        }

    }

}

