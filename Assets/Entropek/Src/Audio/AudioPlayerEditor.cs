using System.Collections.Generic;
using Entropek.Collections;
using UnityEditor;
using UnityEngine;

namespace Entropek.Audio
{

    [CustomEditor(typeof(AudioPlayer))]
    [CanEditMultipleObjects]
    public class AudioPlayerEditor : Editor
    {
        private const int ParameterNamePixelWidth = 150;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AudioPlayer audioPlayer = (AudioPlayer)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            DisplayActiveAudioInstances(audioPlayer);

            EditorGUILayout.Space();
            DisplayFreePooledAudioInstances(audioPlayer);
        }

        private void DisplayActiveAudioInstances(AudioPlayer audioPlayer)
        {
            EditorGUILayout.LabelField("Active Audio Instances", EditorStyles.boldLabel);
            List<AudioInstance> activeAudioInstances = audioPlayer.ActiveAudioInstances;
            for (int i = 0; i < activeAudioInstances.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Name: "+activeAudioInstances[i].Name, GUILayout.Width(ParameterNamePixelWidth));
                EditorGUILayout.LabelField("Type: "+activeAudioInstances[i].Type.ToString());
                EditorGUILayout.EndHorizontal();
            }

        }

        private void DisplayFreePooledAudioInstances(AudioPlayer audioPlayer)
        {
            EditorGUILayout.LabelField("Free Pooled Audio Instances", EditorStyles.boldLabel);

            Dictionary<string, SwapbackList<AudioInstance>> freePooledAudioInstances = audioPlayer.FreePooledAudioInstances;

            foreach (KeyValuePair<string, SwapbackList<AudioInstance>> kvp in freePooledAudioInstances)
            {
                string poolName = kvp.Key;
                SwapbackList<AudioInstance> instances = kvp.Value;

                // Foldout for each pool
                bool isExpanded = EditorPrefs.GetBool("Foldout_" + poolName, false);
                isExpanded = EditorGUILayout.Foldout(isExpanded, $"{poolName} ({instances.Count})");
                EditorPrefs.SetBool("Foldout_" + poolName, isExpanded);

                if (isExpanded)
                {
                    EditorGUI.indentLevel++;

                    for (int i = 0; i < instances.Count; i++)
                    {
                        AudioInstance instance = instances[i];

                        // Display something meaningful about the AudioInstance
                        string label = $"Instance: {i} Type: {instance.Type}";

                        EditorGUILayout.LabelField(label);
                    }

                    EditorGUI.indentLevel--;
                }
            }
        }


    }

}

