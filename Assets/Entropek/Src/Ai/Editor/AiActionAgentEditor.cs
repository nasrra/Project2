using System;
using System.Collections.Generic;
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

    public abstract class AiActionAgentEditor<T,U> : AiAgentEditor<T>
        where T : AiActionAgent<U>
        where U : AiAction
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

            T agent = (T)target;

            EditorGUILayout.Space();

            DrawActionFovEditorToggle(agent);

        }

        protected void OnSceneGUI()
        {
            DebugDrawSelectedActionFov();
        }


        ///
        /// FOV debug dusplay.
        /// 


        private void DrawActionFovEditorToggle(T aiCombatAgent)
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

        private void DrawActionFovEditor(T aiCombatAgent)
        {

            // options to choose from.

            string[] options = new string[aiCombatAgent.AiCombatActions.Length + 1];
            options[0] = "None"; // default to none.

            // add all the names of the combat actions.

            AiAction[] aiCombatActions = aiCombatAgent.AiCombatActions;
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

            fovMinAngle = Mathf.Acos(selectedAction.MinFov) * Mathf.Rad2Deg;
            fovMaxAngle = Mathf.Acos(selectedAction.MaxFov) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Draws two discs, one along the x-axis and another along the y-axis, visually showing the fov of an selected action.
        /// </summary>

        private void DebugDrawSelectedActionFov()
        {
            if (drawActionFovEditor == false || selectedActionToDebugFov == 0)
            {
                return; // dont draw anything if no action has been selected.
            }

            Transform transform = ((T)target).transform;


            // draw the user defined max angle, where the combat angent cannot see in relation to the front of it.

            DrawHorizontalFovArc(transform, OutsideFovColor, -fovMaxAngle, fovMaxAngle);
            DrawVerticalFovArc(transform, OutsideFovColor, 0, fovMaxAngle);
            DrawVerticalFovArc(transform, OutsideFovColor, 0, -fovMaxAngle);

            // draw the remaining area that is less than the minimum viewing angle,

            DrawHorizontalFovArc(transform, OutsideFovColor, -fovMinAngle, -180);
            DrawHorizontalFovArc(transform, OutsideFovColor, fovMinAngle, 180);
            DrawVerticalFovArc(transform, OutsideFovColor, -fovMinAngle, -180);
            DrawVerticalFovArc(transform, OutsideFovColor, fovMinAngle, 180);

            // draw the min angle, where the combat agent can consider the action when the target is within said angle.

            DrawHorizontalFovArc(transform, InsideFovColor, fovMaxAngle, fovMinAngle);
            DrawHorizontalFovArc(transform, InsideFovColor, -fovMaxAngle, -fovMinAngle);
            DrawVerticalFovArc(transform, InsideFovColor, -fovMaxAngle, -fovMinAngle);
            DrawVerticalFovArc(transform, InsideFovColor, fovMaxAngle, fovMinAngle);
        }

        /// <summary>
        /// Draws a filled arc along the local x-axis in relation to a transforms position. 
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="color">The fill color of the arc.</param>
        /// <param name="minAngle">The minimum angle of the arc.</param>
        /// <param name="maxAngle">The maximum angle of the ar.</param>

        private void DrawHorizontalFovArc(Transform transform, Color color, float minAngle, float maxAngle)
        {
            Handles.color = color;

            // Start direction (rotated from forward by minAngle around local up)
            Vector3 minDir = Quaternion.AngleAxis(minAngle, transform.up) * transform.forward;

            // Draw around local up axis
            Handles.DrawSolidArc(transform.position, transform.up, minDir, maxAngle - minAngle, 1);
        }

        /// <summary>
        /// Draws a filled arc along the local y-axis in relation to a transforms position. 
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="color">The fill color of the arc.</param>
        /// <param name="minAngle">The minimum angle of the arc.</param>
        /// <param name="maxAngle">The maximum angle of the ar.</param>

        private void DrawVerticalFovArc(Transform transform, Color color, float minAngle, float maxAngle)
        {
            Handles.color = color;

            // Start direction (rotated up/down from forward around local right)
            Vector3 minDir = Quaternion.AngleAxis(minAngle, transform.right) * transform.forward;

            // Draw around local right axis
            Handles.DrawSolidArc(transform.position, transform.right, minDir, maxAngle - minAngle, 1);
        }



    }


}
