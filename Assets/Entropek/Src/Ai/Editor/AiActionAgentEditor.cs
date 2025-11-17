using System;
using System.Collections.Generic;
using Entropek.UnityUtils.Attributes;
using UnityEditor;
using UnityEngine;

namespace Entropek.Ai{

    /// <summary>
    /// This is the generic editor class for providing the base class inspector gui
    /// for AiCombatAgent subclasses. Inherit from this when creating any editor for a
    /// Combat agent type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>

    [UnityEditor.CustomEditor(typeof(AiActionAgent))]
    [CanEditMultipleObjects]
    public class AiActionAgentEditor : AiAgentEditor
    {


        /// 
        /// Fields.
        /// 


        int selectedActionToDebugFov = 0;
        private float fovMinAngle;
        private float fovMaxAngle;
        private static Color OutsideFovColor = Color.red;
        private static Color InsideFovColor = Color.green;

        private bool drawActionFovEditor = false;


        /// 
        /// Base.
        /// 

        public override void OnInspectorGUI()
        {
            // draw default inspector.

            base.OnInspectorGUI();

            // get a reference to the target.

            AiActionAgent agent = (AiActionAgent)target;

            EditorGUILayout.Space();

            DrawActionFovEditorToggle(agent);

        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            DotProductRangeVisualise.DrawVisualiser(target, fovMinAngle, fovMaxAngle);
        }


        ///
        /// FOV debug dusplay.
        /// 


        private void DrawActionFovEditorToggle(AiActionAgent aiCombatAgent)
        {
            drawActionFovEditor = EditorGUILayout.BeginToggleGroup("Action FOV Editor", drawActionFovEditor);
            if (drawActionFovEditor == true)
            {
                DrawActionFovEditor(aiCombatAgent);
            }
            EditorGUILayout.EndToggleGroup();
        }


        /// <summary>
        /// Displays the debug editor for FOV sliders on a selected combat agents action.
        /// </summary>
        /// <param name="aiCombatAgent"></param>

        private void DrawActionFovEditor(AiActionAgent aiActionAgent)
        {

            // options to choose from.

            string[] options = new string[aiActionAgent.AiActions.Length + 1];
            options[0] = "None"; // default to none.

            // add all the names of the combat actions.

            AiAction[] aiCombatActions = aiActionAgent.AiActions;
            for (int i = 0; i < aiCombatActions.Length; i++)
            {
                options[i + 1] = aiCombatActions[i].Name;
            }

            // recieve user input, which action they have selected to debug the fov of.

            selectedActionToDebugFov = EditorGUILayout.Popup("Choose Action", selectedActionToDebugFov, options);
            if (selectedActionToDebugFov == 0)
            {
                EditorGUILayout.HelpBox("No action is currently being visually debuged.", MessageType.Info);
                return; // dont do anything if 0 is selected.
            }

            // find the selected action.

            AiAction selectedAction = null;
            for (int i = 0; i < aiCombatActions.Length; i++)
            {
                if (aiCombatActions[i].Name == options[selectedActionToDebugFov])
                {
                    selectedAction = aiCombatActions[i];
                }
            }

            // error handling.

            if (selectedAction == null)
            {
                EditorGUILayout.HelpBox("The selected AiCombatAction to display fov is currently null.", MessageType.Error);
            }

            // convert the dot product values to actual angles for easier visual debugging.

            fovMinAngle = selectedAction.Fov.GetMinAngle();
            fovMaxAngle = selectedAction.Fov.GetMaxAngle();
        }

    }


}
