using System.Collections.Generic;
using Entropek.Ai;
using Entropek.UnityUtils;
using UnityEditor;
using UnityEngine;

namespace Entropek.Ai
{   
    [UnityEditor.CustomEditor(typeof(AiAgent), true)]
    [CanEditMultipleObjects]
    public abstract class AiAgentEditor : RuntimeEditor<AiAgent>
    {

        private const int FieldLabelPixelWidth = 150;

        protected bool possibleOutcomesToggled = false;


        ///
        /// Base.
        /// 


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            AiAgent agent = (AiAgent)target;

            EditorGUILayout.Space();

            DrawPossibleOutcomeToggle(agent);
        }


        /// <summary>
        /// Toggles displaying the possible outcomes that can be chosen in an evaluation call for the AiAgent.
        /// </summary>
        /// <param name="aiAgent">The specified AiAgent.</param>

        protected void DrawPossibleOutcomeToggle(AiAgent aiAgent)
        {
            possibleOutcomesToggled = EditorGUILayout.BeginToggleGroup("Possible Outcomes", possibleOutcomesToggled);
            if (possibleOutcomesToggled == true)
            {
                DrawPossibleOutcomes(aiAgent);
            }
            EditorGUILayout.EndToggleGroup();
        }

        /// <summary>
        /// Displays a list of possible outcomes that could have been chosen in an evaluation call.
        /// </summary>

        protected void DrawPossibleOutcomes(AiAgent aiAgent)
        {

            List<AiPossibleOutcome> possibleOutcomes = aiAgent.PossibleOutcomes;

            if (possibleOutcomes == null)
            {
                EditorGUILayout.HelpBox("Possible Outcomes has not been initialised.", MessageType.Warning);
                return;
            }
            else if(possibleOutcomes.Count == 0)
            {
                EditorGUILayout.HelpBox("No Possible Outcomes were found.", MessageType.Info);                
                return;
            }

            for (int i = 0; i < possibleOutcomes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(possibleOutcomes[i].Name, GUILayout.Width(FieldLabelPixelWidth));
                EditorGUILayout.LabelField(possibleOutcomes[i].EvaluationScore.ToString("F2")); // eg: 0.00
                EditorGUILayout.EndHorizontal();
            }
        }

    }
}

