using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Entropek.UnityUtils
{

    /// <summary>
    /// Base editor that automatically displays private fields marked with [RuntimeField].
    /// </summary>

    public class RuntimeEditor<T> : CustomEditor where T : UnityEngine.Object
    {
        private FieldInfo[] runtimeFields;
        private bool drawRuntimeFields = true;
        private Dictionary<string, bool> foldoutStates = new();


        /// 
        /// Base.
        /// 


        protected override void OnEnable()
        {

            base.OnEnable();

            runtimeFields = target.GetType()
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
            if(target == null)
            {
                return;
            }

            // if(Selection.activeObject != target)
            // {
            //     return;
            // }

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

            GUI.enabled = false;

            foreach (var field in runtimeFields)
            {
                object value = field.GetValue(targetObj);
                string label = ObjectNames.NicifyVariableName(field.Name);
                switch (value)
                {
                    case UnityEngine.Object unityObj:
                        EditorGUILayout.ObjectField(label, unityObj, field.FieldType, true);
                        break;
                    case System.Collections.IList list:
                        DrawList(label, list);
                        break;
                    case System.Collections.IDictionary dictionary:
                        DrawDicionary(label, dictionary);
                        break;
                    default:
                        EditorGUILayout.LabelField(label, value?.ToString() ?? "null");
                        break;
                }                
            }

            GUI.enabled = true;
        }

        protected void DrawList(string fieldName, System.Collections.IList list)
        {
            bool foldout = RetrieveFoldoutState(fieldName);
            foldout = EditorGUILayout.Foldout(foldout, $"{fieldName} ({list.Count})");
            foldoutStates[fieldName] = foldout;

            if (foldout)
            {                
                EditorGUI.indentLevel++;
                
                for(int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    if (item is UnityEngine.Object unityObj)
                    {
                        EditorGUILayout.ObjectField(item.GetType().Name, unityObj, unityObj.GetType(), true);
                    }
                    else
                    {
                        EditorGUILayout.LabelField(item.GetType().Name, item?.ToString() ?? "null");                        
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }

        protected void DrawDicionary(string fieldName, IDictionary dictionary)
        {
            bool foldout = RetrieveFoldoutState(fieldName);
            foldout = EditorGUILayout.Foldout(foldout, $"{fieldName} ({dictionary.Count})");
            foldoutStates[fieldName] = foldout;

            if (foldout)
            {
                EditorGUI.indentLevel++;

                foreach(DictionaryEntry kvp in dictionary)
                {
                    EditorGUILayout.BeginHorizontal();
                    switch (kvp.Key)
                    {
                        case UnityEngine.Object unityObj:
                            EditorGUILayout.ObjectField("", unityObj, unityObj.GetType(), true);
                            break;
                        default:
                            EditorGUILayout.LabelField("", kvp.Key.GetType().Name ?? "null");                        
                            break;
                    }

                    switch (kvp.Value)
                    {
                        case UnityEngine.Object unityObj:
                            EditorGUILayout.ObjectField("", unityObj, unityObj.GetType(), true);
                            break;
                        default:
                            EditorGUILayout.LabelField("", kvp.Value.ToString() ?? "null");
                            break;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
            }
        }



        private bool RetrieveFoldoutState(string foldoutName)
        {
            // Get or create a foldout state.

            bool foldout;
            if(foldoutStates.TryGetValue(foldoutName, out foldout) == true)
            {
                foldoutStates[foldoutName] = foldout;
            }

            return foldout;
        }
    }    
}

