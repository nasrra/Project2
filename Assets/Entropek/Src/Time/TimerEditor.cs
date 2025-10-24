using UnityEditor;
using UnityEngine;

namespace Entropek.Time{

    [CustomEditor(typeof(Timer), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class TimerEditor : Editor
    {

        private const int ParameterNamePixelWidth = 150;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Timer timer = (Timer)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Private Field Peek", EditorStyles.boldLabel);
            DisplayTimerState(timer);
            DisplayCurrentTime(timer);

        }

        private void DisplayTimerState(Timer timer)
        {            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Timer State", GUILayout.Width(ParameterNamePixelWidth));
            EditorGUILayout.LabelField(timer.State.ToString());
            EditorGUILayout.EndHorizontal();
        }

        private void DisplayCurrentTime(Timer timer)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Time", GUILayout.Width(ParameterNamePixelWidth));
            EditorGUILayout.LabelField(timer.CurrentTime.ToString("F2"));
            EditorGUILayout.EndHorizontal();
        }
    }


}

