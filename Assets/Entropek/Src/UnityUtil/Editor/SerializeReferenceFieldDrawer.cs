using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor.PackageManager;

namespace Entropek.UnityUtils
{

    public abstract class SerializeReferenceFieldDrawer<T> where T : UnityEngine.Object
    {
        /// <summary>
        /// Retrieves all derived types (sub-classes) of the assigned field type.
        /// </summary>

        public abstract void RetrieveDerivedTypes();

        /// <summary>
        /// Draws a drop down menu to edit a instance(s) of the FieldType and its subclass(es).
        /// </summary>
        /// <param name="target"></param>
        /// <param name="serializedProperty"></param>

        public abstract void DrawMenu(T target,  in SerializedProperty serializedProperty);
    }

    /// <summary>
    /// An editor specific class for the [SerializeReference] attribute.
    /// Used to store and managed derived types (subclasses) of the set FieldType.
    /// </summary>
    /// <typeparam name="TargetType">The type of the target.</typeparam>
    /// <typeparam name="FieldType">The Type of the field to manage and store its derived types.</typeparam>
    
    public class SerializeReferenceFieldDrawer<TargetType, FieldType> : SerializeReferenceFieldDrawer<TargetType> 
    where TargetType : UnityEngine.Object
    {
        // List types deriving from MySettings for the dropdown
        private Type[] derivedTypes;
        private string[] derivedTypeNames;
        private int selectedTypeIndex = 0;
        private bool isValidType = true;


        public override void RetrieveDerivedTypes()
        {
            Type fieldType = ExtractFieldType(typeof(FieldType));
            isValidType = IsFieldTypePolymorphic(fieldType);
            
            if(isValidType == false)
            {
                return;
            }


            // Now find all subclasses of the element type

            derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => fieldType.IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();

            derivedTypeNames = derivedTypes.Select(t => t.Name).ToArray();
        }


        /// <summary>
        /// Checks whether the field type is capable of polymorphism.
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>


        private bool IsFieldTypePolymorphic(Type fieldType)
        {
            return fieldType.IsClass;
        }


        /// <summary>
        /// Gets the underlying type of the FieldType if it is within a List or Array.
        /// </summary>
        /// <returns></returns>

        private Type ExtractFieldType(Type fieldType)
        {
            if (fieldType.IsArray)
            {
                fieldType = fieldType.GetElementType();
            }

            else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                fieldType = fieldType.GetGenericArguments()[0];
            }

            return fieldType;
        }

        public override void DrawMenu(TargetType target, in SerializedProperty serializedProperty)
        {
            DrawMenu(target, in serializedProperty, serializedProperty.isArray? "Add Selected Type." : "Set To Selected Type.");
        }

        /// <summary>
        /// Draws a drop down menu to add instances of the FieldType and its subclasses.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="serializedProperty"></param>
        /// <param name="buttonName">The text displayed on the modifier button.</param>

        private void DrawMenu(TargetType target, in SerializedProperty serializedProperty, string buttonName)
        {
            // Indent the following controls to visually nest under the foldout
            EditorGUI.indentLevel++;

            // Add a small space for clarity
            EditorGUILayout.Space();

            if (isValidType == true)
            {
                // Draw your dropdown and add button inside the foldout area
                selectedTypeIndex = EditorGUILayout.Popup("Selected Type", selectedTypeIndex, derivedTypeNames);

                if (GUILayout.Button(buttonName))
                {
                    // Create an instance of the selected type
                    var instance = Activator.CreateInstance(derivedTypes[selectedTypeIndex]);
                    
                    // retrieve the field within the target.
                    var field = target.GetType().GetField(serializedProperty.name, BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    
                    // assign the value in the editor to the new instance value.
                    
                    if (field != null)
                    {
                        if (field.FieldType.IsArray)
                        {
                            var array = field.GetValue(target) as Array;
                            // Create a new array with one extra slot.
                            var elementType = array.GetType().GetElementType();
                            var newArray = Array.CreateInstance(elementType, array.Length + 1);
                            Array.Copy(array, newArray, array.Length);
                            newArray.SetValue(instance, array.Length);
                            field.SetValue(target, newArray);
                        }

                        else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            var ilist = field.GetValue(target) as System.Collections.IList;
                            ilist.Add(instance);
                        }

                        else
                        {
                            field.SetValue(target, instance);
                        }
                        
                        EditorUtility.SetDirty(target);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox($"{target} does not contain serilized property: {serializedProperty.name}.",MessageType.Error);                            
                    }     
                }
            }            
            else
            {
                EditorGUILayout.HelpBox("[SerializeReference] attribute for collections should only store classes; not primitives or value types.",MessageType.Error);
            }
            EditorGUI.indentLevel--;
        }
    }    
}

