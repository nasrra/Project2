using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Entropek.UnityUtils
{

    /// <summary>
    /// Base editor that automatically displays private fields marked with [RuntimeField].
    /// </summary>

    public class RuntimeEditor<T> : UnityEditor.Editor where T : UnityEngine.Object
    {
        private FieldInfo[] runtimeFields;
        private bool drawRuntimeFields = true;

        protected virtual void OnEnable()
        {
            runtimeFields = typeof(T)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(field => Attribute.IsDefined(field, typeof(Attributes.RuntimeField)))
                .ToArray();

            EditorApplication.update += EditorUpdate;

        }

        protected virtual void OnDisable()
        {
            EditorApplication.update -= EditorUpdate;
        }

        protected virtual void EditorUpdate()
        {
            if (EditorApplication.isPlaying && target != null)
            {
                Repaint();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("[Runtime Editor]", EditorStyles.whiteBoldLabel);

            EditorGUILayout.Space();

            DrawRuntimeFieldsToggle((T)target);
        }

        /// <summary>
        /// Toggles displaying the runtime fields in the inspector.
        /// </summary>
        /// <param name="targetObj"></param>

        protected void DrawRuntimeFieldsToggle(T targetObj)
        {
            drawRuntimeFields = EditorGUILayout.BeginToggleGroup("Runtime Fields", drawRuntimeFields);
            if (drawRuntimeFields == true)
            {
                DrawRuntimeFields(targetObj);
            }
            EditorGUILayout.EndToggleGroup();
        }

        /// <summary>
        /// Draws all the fields marked with [RuntimeField] within the class.
        /// </summary>
        /// <param name="targetObj"></param>

        protected void DrawRuntimeFields(T targetObj)
        {
            if (runtimeFields == null || runtimeFields.Length == 0)
            {
                return;
            }

            foreach (var field in runtimeFields)
            {
                object value = field.GetValue(targetObj);
                string label = ObjectNames.NicifyVariableName(field.Name);
                if (value is UnityEngine.Object unityObj)
                {
                    EditorGUILayout.ObjectField(label, unityObj, field.FieldType, true);
                }
                else
                {
                    EditorGUILayout.LabelField(label, value?.ToString() ?? "null");
                }
            }
        }


    }    
}

