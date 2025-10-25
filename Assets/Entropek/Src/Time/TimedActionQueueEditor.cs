using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Entropek.Time
{

    [CustomEditor(typeof(TimedActionQueue))]
    [CanEditMultipleObjects]
    public class TimedActionQueueEditor : Editor
    {
        private const float MethodNameWidth = 200;
        private const float DeclaringTypeWidth = 150;
        private const float TimeWidth = 50;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TimedActionQueue timedActionQueue = (TimedActionQueue)target;

            EditorGUILayout.Space();

            DisplayQeueudActions(timedActionQueue);

        }

        /// <summary>
        /// Displays the information of actions queued on a table.
        /// </summary>
        /// <param name="timedActionQueue"></param>

        private void DisplayQeueudActions(TimedActionQueue timedActionQueue)
        {
            EditorGUILayout.LabelField("Queued Actions", EditorStyles.boldLabel);

            // Table header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("[Method]", GUILayout.Width(MethodNameWidth));
            EditorGUILayout.LabelField("[Declaring Type]", GUILayout.Width(DeclaringTypeWidth));
            EditorGUILayout.LabelField("[Time]", GUILayout.Width(TimeWidth));
            EditorGUILayout.EndHorizontal();

            // display each entry in the timed action queue on the table.

            foreach ((Action, float) entry in timedActionQueue.Queue)
            {

                EditorGUILayout.BeginHorizontal();

                Action action = entry.Item1;

                if (action != null)
                {
                    MethodInfo methodInfo = action.Method;
                    string methodName = methodInfo.Name;
                    string declaringType = methodInfo.DeclaringType?.Name ?? "UnknownType";
                    EditorGUILayout.LabelField($"{methodName}", GUILayout.Width(MethodNameWidth));
                    EditorGUILayout.LabelField($"{declaringType}", GUILayout.Width(DeclaringTypeWidth));
                    EditorGUILayout.LabelField($"{entry.Item2}", GUILayout.Width(TimeWidth));
                }
                else
                {
                    EditorGUILayout.LabelField($"null", GUILayout.Width(MethodNameWidth));
                    EditorGUILayout.LabelField($"null", GUILayout.Width(DeclaringTypeWidth));
                    EditorGUILayout.LabelField($"{entry.Item2}", GUILayout.Width(TimeWidth));
                }

                EditorGUILayout.EndHorizontal();
            }

        }

    }
}



