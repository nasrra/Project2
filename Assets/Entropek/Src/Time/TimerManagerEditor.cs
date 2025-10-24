using Entropek.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Entropek.Time
{

    [CustomEditor(typeof(TimerManager))]
    public class TimerManagerEditor : Editor
    {
        private const int ParameterNamePixelWidth = 150;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            TimerManager timerManager = (TimerManager)target;

            EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            DisplayTimers("Active Timers", timerManager.ActiveTimers);

            EditorGUILayout.Space();

            DisplayTimers("Paused Timers", timerManager.PausedTimers);

            EditorGUILayout.Space();

            DisplayTimers("Halted Timers", timerManager.HaltedTimers);
        }

        private void DisplayTimers(string timerCategory, SwapbackList<Timer> timers)
        {
            EditorGUILayout.LabelField(timerCategory, EditorStyles.boldLabel);
            for (int i = 0; i < timers.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(timers[i].EditorNameTag);
                EditorGUILayout.EndHorizontal();
            }
        }
    }    
}

