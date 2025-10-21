using UnityEditor;
using UnityEngine;

namespace Entropek.EntityStats{


    [CustomEditor(typeof(Health))]
    public class HealthEditor : Editor{
        
        protected const int PropertyNameLabelWidth = 150; // pixels.

        public override void OnInspectorGUI(){
            DrawDefaultInspector();
            DisplayEditorHeader();
            DisplayHealthState();
        }

        protected void DisplayEditorHeader(){
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);
        }

        protected void DisplayHealthState(){
            Health health = (Health)target;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Health State", GUILayout.Width(PropertyNameLabelWidth));
            EditorGUILayout.LabelField(health.HealthState.ToString());
            EditorGUILayout.EndHorizontal();
        }
    }


}

