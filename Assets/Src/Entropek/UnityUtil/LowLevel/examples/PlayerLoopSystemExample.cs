using System.Collections.Generic;
using Entropek.Collections;

///
/// This is an example class for a custom player loop system.
/// NOTE:
///     Uncomment out boot strap to enable.
/// 

namespace Entropek.UnityUtils.LowLevel.Examples{

public class PlayerLoopSystemExample{


    /// 
    /// Data.
    /// 

    public static UniqueIdGenerator idGenerator {get; private set;} = new UniqueIdGenerator();

    static SwapbackList<PlayerLoopComponentExample> activeTimers    = new SwapbackList<PlayerLoopComponentExample>();
    static SwapbackList<PlayerLoopComponentExample> pausedTimers    = new SwapbackList<PlayerLoopComponentExample>();
    static SwapbackList<PlayerLoopComponentExample> stoppedTimers   = new SwapbackList<PlayerLoopComponentExample>();

    static Dictionary<uint, int> activeTimersIdToListIndexMap = new Dictionary<uint, int>();
    static Dictionary<uint, int> pausedTimersIdToListIndexMap = new Dictionary<uint, int>();
    static Dictionary<uint, int> stoppedTimersIdToListIndexMap = new Dictionary<uint, int>();

    /// 
    /// Public Functions.
    /// 


    public static void RegisterTimer(PlayerLoopComponentExample timer){
    
        // timers are defaulted to stop.
        AddTimerToStoppedList(timer);
    }

    public static void DeregisterTimer(PlayerLoopComponentExample timer){
        if(IsStopped(timer)==true){
            RemoveTimerFromStoppedList(timer);
        }
        else if(IsActive(timer)==true){
            RemoveTimerFromActiveList(timer);
        }
        else{
            RemoveTimerFromPausedList(timer);
        }
    }

    public static void StartTimer(PlayerLoopComponentExample timer){
        
        // short-circuit if the timer is already started.

        if(IsActive(timer)==true){
            return;
        }

        // check and handle if the timer is in another state.
        
        if(IsPaused(timer)==true){
            RemoveTimerFromPausedList(timer);
        }
        else if(IsStopped(timer)==true){
            RemoveTimerFromStoppedList(timer);
        }

        AddTimerToActiveList(timer);

    }

    public static void StopTimer(PlayerLoopComponentExample timer){
        
        // short-circuit if the timer is already stopped.

        if(IsStopped(timer)==true){
            return;
        }

        // check and handle if the timer is in another state.
        
        if(IsPaused(timer)==true){
            RemoveTimerFromPausedList(timer);
        }
        else if(IsActive(timer)==true){
            RemoveTimerFromActiveList(timer);
        }

        AddTimerToStoppedList(timer);

    }

    public static void PauseTimer(PlayerLoopComponentExample timer){
    
        // short-circuit if the timer is already stopped.

        if(IsPaused(timer)==true){
            return;
        }

        // check and handle if the timer is in another state.
        
        if(IsStopped(timer)==true){
            RemoveTimerFromStoppedList(timer);
        }
        else if(IsActive(timer)==true){
            RemoveTimerFromActiveList(timer);
        }

        AddTimerToPausedList(timer);
    
    }


    /// 
    /// Timer State Checking.
    /// 


    public static bool IsActive(PlayerLoopComponentExample timer){
        return activeTimersIdToListIndexMap.ContainsKey(timer.Id);
    }

    public static bool IsStopped(PlayerLoopComponentExample timer){
        return stoppedTimersIdToListIndexMap.ContainsKey(timer.Id);
    }

    public static bool IsPaused(PlayerLoopComponentExample timer){
        return pausedTimersIdToListIndexMap.ContainsKey(timer.Id);
    }


    /// 
    /// Add Timer.
    /// 


    static void AddTimerToActiveList(PlayerLoopComponentExample timer){
        AddTimerToList(activeTimers, activeTimersIdToListIndexMap, timer);
    }

    static void AddTimerToStoppedList(PlayerLoopComponentExample timer){
        AddTimerToList(stoppedTimers, stoppedTimersIdToListIndexMap, timer);
    }

    static void AddTimerToPausedList(PlayerLoopComponentExample timer){
        AddTimerToList(pausedTimers, pausedTimersIdToListIndexMap, timer);
    }

    static void AddTimerToList(SwapbackList<PlayerLoopComponentExample> timerList, Dictionary<uint, int> idToArrayIndexMap, PlayerLoopComponentExample timerToAdd){
        timerList.Add(timerToAdd);
        idToArrayIndexMap.Add(timerToAdd.Id, timerList.Count-1);
    }


    /// 
    /// Remove Timer.
    /// 


    static void RemoveTimerFromActiveList(PlayerLoopComponentExample timer){
        RemoveTimerFromList(activeTimers, activeTimersIdToListIndexMap, timer);
    }

    static void RemoveTimerFromStoppedList(PlayerLoopComponentExample timer){
        RemoveTimerFromList(stoppedTimers, stoppedTimersIdToListIndexMap, timer);
    }

    static void RemoveTimerFromPausedList(PlayerLoopComponentExample timer){
        RemoveTimerFromList(pausedTimers, pausedTimersIdToListIndexMap, timer);
    }

    static void RemoveTimerFromList(SwapbackList<PlayerLoopComponentExample> timerList, Dictionary<uint, int> idToArrayIndexMap, PlayerLoopComponentExample timerToRemove){
        
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
        
        int timerToRemoveIndex = idToArrayIndexMap[timerToRemove.Id];
        int lastIndex = timerList.Count - 1;
        PlayerLoopComponentExample lastTimer = timerList[lastIndex];

        // set the timer to removes array data to the last entries data.

        timerList[timerToRemoveIndex] = lastTimer;
        idToArrayIndexMap[lastTimer.Id] = timerToRemoveIndex; 

        // then remove the last entry from the list.
        
        idToArrayIndexMap.Remove(timerToRemove.Id);
        timerList.RemoveAt(lastIndex);
    }


    /// 
    /// Player Loop Functions.
    /// 


    static void Update(){

        // tick down all available timers.
        
        for(int i = 0; i < activeTimers.Count; i++){
            PlayerLoopComponentExample timer = activeTimers[i];
            timer.Tick();
        }
    }

    static void Clear(){
        activeTimers.Clear();
        activeTimersIdToListIndexMap.Clear();
        stoppedTimers.Clear();
        stoppedTimersIdToListIndexMap.Clear();
        pausedTimers.Clear();
        pausedTimersIdToListIndexMap.Clear();
        idGenerator.Clear();
    }


    /// 
    /// Bootstrapper - inject this to Unity PlayerLoop.
    /// 

    /// 
    /// NOTE: 
    ///     Uncomment boot strap to enable this example.
    /// 


    // static class Bootstrap{
        
    //     static PlayerLoopSystem timerManagerSystem;
        
    //     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        
    //     internal static void Initialise(){

    //         // get the current player loop.
            
    //         PlayerLoopSystem playerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            
    //         // create the player loop system of timer manager.

    //         timerManagerSystem = new PlayerLoopSystem(){
    //             type = typeof(PlayerLoopSystemExample),
    //             updateDelegate = Update,
    //             subSystemList = null
    //         };

    //         // insert the timer manager system.

    //         if(UnityUtils.LowLevel.PlayerLoop.InsertSystem<UnityEngine.PlayerLoop.Update>(ref playerLoop, timerManagerSystem,0)==false){
    //             throw new System.Exception("Failed to insert Timer System into Player Loop.");
    //         }


    //         // link into editor playmode enter and exit
    //         // for applying clean-up of the data when editor is not set
    //         // to reload the domain when re-entering playmode.

    //         #if UNITY_EDITOR

    //         Entropek.UnityUtils.Editor.ExitedPlaymode -= OnPlayModeStateChange;
    //         Entropek.UnityUtils.Editor.ExitedPlaymode += OnPlayModeStateChange;

    //         static void OnPlayModeStateChange(){
                    
    //             PlayerLoopSystem playerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
                
    //             // clear all data from timer manager.
                
    //             Clear(); 
                
    //             // remove the system. 

    //             UnityUtils.LowLevel.PlayerLoop.RemoveSystem<UnityEngine.PlayerLoop.Update>(ref playerLoop, timerManagerSystem);

    //             // update the player loop.

    //             UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(playerLoop);
    //         }

    //         #endif

    //         // update the player loop.

    //         UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(playerLoop);

    //         // check the state of the system graph.

    //         UnityUtils.LowLevel.PlayerLoop.PrintPlayerLoop(playerLoop);    
    //     }
    // }

}

}

