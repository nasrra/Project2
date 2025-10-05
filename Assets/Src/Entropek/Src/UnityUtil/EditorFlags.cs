// using System;
// using UnityEditor;
// using UnityEngine;

// namespace Entropek.UnityUtils{

// public class Editor{
//     public static bool InPlaymode = false; 
    
//     public static event Action ExitedPlaymode;

//     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
//     private static void Initialise(){
//         #if UNITY_EDITOR
        
//         EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
//         EditorApplication.playModeStateChanged += OnPlayModeStateChange;

//         static void OnPlayModeStateChange(PlayModeStateChange state){
//             if(state == PlayModeStateChange.ExitingPlayMode){
//                 InPlaymode = false;
//                 ExitedPlaymode?.Invoke();
//             }
//         }


//         #endif

//         InPlaymode = true;
//     }
// }


// }

