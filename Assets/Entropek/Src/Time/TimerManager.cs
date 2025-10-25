using System.Collections.Generic;
using Entropek.Collections;
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

        public void DeregisterTimer(Timer timer)
        {

            if (IsHalted(timer) == true)
            {
                RemoveTimerFromHaltedList(timer);
            }
            else if (IsActive(timer) == true)
            {
                RemoveTimerFromActiveList(timer);
            }
            else if (IsPaused(timer) == true)
            {
                RemoveTimerFromPausedList(timer);
            }
        }

        public bool BeginTimer(Timer timer)
        {

            // short-circuit if the timer is already started; the lists dont have to be altered.

            if (IsActive(timer) == true)
            {
                return true; // because the timer has already began.
            }

            // check and handle if the timer is in another state.

            if (IsPaused(timer) == true)
            {
                RemoveTimerFromPausedList(timer);
            }
            else if (IsHalted(timer) == true)
            {
                RemoveTimerFromHaltedList(timer);
            }

            AddTimerToActiveList(timer);

            return true;
        }

        public bool HaltTimer(Timer timer)
        {

            // short-circuit if the timer is already stopped; the lists dont have to be altered..

            if (IsHalted(timer) == true)
            {
                return false; // because the timer has already halted.
            }

            // check and handle if the timer is in another state.

            if (IsPaused(timer) == true)
            {
                RemoveTimerFromPausedList(timer);
            }
            else if (IsActive(timer) == true)
            {
                RemoveTimerFromActiveList(timer);
            }

            AddTimerToHaltedList(timer);

            return true;
        }

        public bool PauseTimer(Timer timer)
        {

            // short-circuit if the timer is already stopped; the lists dont have to be altered..

            if (IsPaused(timer) == true)
            {
                return false; // because the timer has already been paused.
            }

            // check and handle if the timer is in another state.

            if (IsHalted(timer) == true)
            {
                RemoveTimerFromHaltedList(timer);
            }
            else if (IsActive(timer) == true)
            {
                RemoveTimerFromActiveList(timer);
            }

            AddTimerToPausedList(timer);

            return true;
        }


        /// 
        /// Timer State Checking.
        /// 


        public bool IsActive(Timer timer)
        {
            return activeTimersIdToListIndexMap.ContainsKey(timer.GetInstanceID());
        }

        public bool IsHalted(Timer timer)
        {
            return haltedTimersIdToListIndexMap.ContainsKey(timer.GetInstanceID());
        }

        public bool IsPaused(Timer timer)
        {
            return pausedTimersIdToListIndexMap.ContainsKey(timer.GetInstanceID());
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
        }


        /// 
        /// Bootstrapper - inject this to Unity PlayerLoop.
        /// 


        static class Bootstrap
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            internal static void Initialise()
            {
                GameObject main = new();
                main.name = "Timer Manager";
                main.AddComponent<TimerManager>();
                DontDestroyOnLoad(main);
                Singleton = main.GetComponent<TimerManager>();
            }
        }

    }

}

