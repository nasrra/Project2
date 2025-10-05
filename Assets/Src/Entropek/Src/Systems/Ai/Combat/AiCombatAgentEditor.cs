using UnityEditor;
using UnityEngine;

namespace Entropek.Systems.Ai.Combat{


[CustomEditor(typeof(AiCombatAgent))]
public class AiCombatAgentEditor : Editor{

    private const int NameLabelPixelWidth = 150;

    public override void OnInspectorGUI(){
        
        // draw default inspector.
        
        DrawDefaultInspector();

        // get a reference to the target.

        AiCombatAgent agent = (AiCombatAgent)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Debug Editor:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Possble Combat Actions");

        // ensure there are possible actions to display.

        (AiCombatAction, float)[] possibleCombatActions = agent.GetPossibleCombatActions();
        if(possibleCombatActions==null || possibleCombatActions.Length == 0){
            EditorGUILayout.HelpBox("No combat actions available.", MessageType.Warning);
            return;
        }

        for(int i = 0; i < possibleCombatActions.Length; i++){
            (AiCombatAction action, float score) = possibleCombatActions[i];
            if(action==null){
                continue;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(action.ActionName, GUILayout.Width(NameLabelPixelWidth));
            EditorGUILayout.LabelField(score.ToString("F2")); // eg: 0.00
            EditorGUILayout.EndHorizontal();
        }

    }

}


}

