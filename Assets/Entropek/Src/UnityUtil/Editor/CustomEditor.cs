using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;



namespace Entropek.UnityUtils
{
    public class CustomEditor : Editor
    {
        Dictionary<string, SerializeReferenceFieldDrawer> seriliazedReferences;

        protected virtual void OnEnable()
        {
            seriliazedReferences = new();

            serializedObject.Update();

            // Draw all properties.

            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (IsSerializeReference(property) == true)
                {
                    // create a SerializeReferenceFieldDrawer for drawing the seriliazedReference.

                    Type propertyUnderlyingType = property.GetUnderlyingType();
                    
                    Type drawerType = typeof(SerializeReferenceFieldDrawer<>).MakeGenericType(propertyUnderlyingType);

                    SerializeReferenceFieldDrawer drawerInstance = (SerializeReferenceFieldDrawer)Activator.CreateInstance(drawerType);

                    seriliazedReferences.Add(property.name, drawerInstance);
                    
                    drawerInstance.RetrieveDerivedTypes();
                } 
            }
   
           // Your custom list rendering here...

            serializedObject.ApplyModifiedProperties();            
        }

        public override void OnInspectorGUI()
        {
            // Draw all properties.

            SerializedProperty serializedProperty = serializedObject.GetIterator();
            bool enterChildren = true;
            while (serializedProperty.NextVisible(enterChildren))
            {
                enterChildren = false;
                
                EditorGUILayout.PropertyField(serializedProperty, true);
                
                if (seriliazedReferences.ContainsKey(serializedProperty.name))
                {
                    DrawSerializeReferenceEditor(target, in serializedProperty);    
                }
            }
   
           // Your custom list rendering here...

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the menu in the inspector to modify a field implementing the SerializeReference attribute. 
        /// </summary>
        /// <param name="target">The target script in the editor.</param>
        /// <param name="serializedProperty">The SerializeProperty to draw.</param>

        protected void DrawSerializeReferenceEditor(UnityEngine.Object target, in SerializedProperty serializedProperty)
        {
            seriliazedReferences[serializedProperty.name].DrawMenu(target, in serializedProperty);
        }

        /// <summary>
        /// Determines whether a SerialisedProperty implements the [SerializeReference] attrbute.
        /// </summary>
        /// <param name="serializedProperty">The SerialisedProperty to check.</param>
        /// <returns>true, if it implements the attribute; otherwise false.</returns>

        protected bool IsSerializeReference(SerializedProperty serializedProperty)
        {
            // SerializeReference: A scripting attribute that instructs Unity to serialize a field as a reference instead of as a value; allowing polymorphism for sub-classes.
            
            var targetType = target.GetType();
            var field = targetType.GetField(serializedProperty.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            return field != null && field.GetCustomAttribute<SerializeReference>() != null;
        }


    }
}
