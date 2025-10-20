using UnityEditor;
using UnityEngine;

namespace Entropek.Systems{


[CustomEditor(typeof(ShieldedHealth))]
public class ShieldedHealthEditor : HealthEditor{

    public override void OnInspectorGUI(){
        DrawDefaultInspector();
        DisplayEditorHeader();
        DisplayHealthState();
        DisplayShieldState();
    }

    protected void DisplayShieldState(){
        
        ShieldedHealth health = (ShieldedHealth)target;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Shield State", GUILayout.Width(PropertyNameLabelWidth));
        EditorGUILayout.LabelField(health.ShieldState.ToString());
        EditorGUILayout.EndHorizontal();
    }
}


}

