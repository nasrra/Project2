using UnityEditor;
using UnityEngine;

namespace Entropek.Systems.Autoload{

[CustomEditor(typeof(Timer))]
[CanEditMultipleObjects]
public class TimerEditor : Editor{
    
    private const int ParameterNamePixelWidth = 150;

    public override void OnInspectorGUI(){
        DrawDefaultInspector();

        Timer timer = (Timer)target;

        // Current Time.

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Current Time", GUILayout.Width(ParameterNamePixelWidth));
        EditorGUILayout.LabelField(timer.CurrentTime.ToString("F2"));
        EditorGUILayout.EndHorizontal();
    }
}


}

