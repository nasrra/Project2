using System;
using System.Collections.Generic;
using System.Reflection;
using Entropek.UnityUtils.Attributes;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;



namespace Entropek.UnityUtils
{
    public class CustomEditor : Editor
    {
        Dictionary<string, SerializeReferenceFieldDrawer> seriliazedReferences;
        Dictionary<string, AdvancedPropertyAttribute> advancedPropertyAttributes;

        protected virtual void OnEnable()
        {
            seriliazedReferences = new();
            advancedPropertyAttributes = new();

            serializedObject.Update();

            var targetType = target.GetType();
            // Draw all properties.

            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (IsSerializeReference(targetType, property) == true)
                {
                    // create a SerializeReferenceFieldDrawer for drawing the seriliazedReference.

                    Type propertyUnderlyingType = property.GetUnderlyingType();
                    
                    Type drawerType = typeof(SerializeReferenceFieldDrawer<>).MakeGenericType(propertyUnderlyingType);

                    SerializeReferenceFieldDrawer drawerInstance = (SerializeReferenceFieldDrawer)Activator.CreateInstance(drawerType);

                    seriliazedReferences.Add(property.name, drawerInstance);
                    
                    drawerInstance.RetrieveDerivedTypes();
                }

                if(IsAdanvedPropertyAttribute(targetType, property, out AdvancedPropertyAttribute advancedPropertyAttribute) == true)
                {
                    advancedPropertyAttributes.Add(property.name, advancedPropertyAttribute);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            // Draw all properties.
            SerializedProperty serializedProperty = serializedObject.GetIterator();
            bool enterChildren = true;

            while (serializedProperty.NextVisible(enterChildren))
            {
                enterChildren = false;

                // Handle AdvancedPropertyAttribute
                if (advancedPropertyAttributes.ContainsKey(serializedProperty.name))
                {
                    advancedPropertyAttributes[serializedProperty.name].OnInspectorGUI(serializedProperty);
                }
                else
                {
                    EditorGUILayout.PropertyField(serializedProperty, true);
                }

                // Handle [SerializeReference] entries
                if (seriliazedReferences.ContainsKey(serializedProperty.name))
                {
                    // Use your separate function to draw the managed reference
                    DrawSerializeReferenceEditor(target, in serializedProperty);
                }
            }

            // apply any modifications to the SerialisedProperty back to the object.

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnSceneGUI()
        {
            foreach (KeyValuePair<string, AdvancedPropertyAttribute> kvp in advancedPropertyAttributes)
            {   
                kvp.Value.OnSceneGUI(target);
            }
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

        protected bool IsSerializeReference(in Type targetType, in SerializedProperty serializedProperty)
        {
            // SerializeReference: A scripting attribute that instructs Unity to serialize a field as a reference instead of as a value; allowing polymorphism for sub-classes.
            var field = targetType.GetField(serializedProperty.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return field != null && field.GetCustomAttribute<SerializeReference>() != null;
        }

        protected bool IsAdanvedPropertyAttribute(in Type targetType, in SerializedProperty serializedProperty, out AdvancedPropertyAttribute advancedPropertyAttribute)
        {
            advancedPropertyAttribute = null;
            
            var field = targetType.GetField(serializedProperty.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                return false;
            }

            advancedPropertyAttribute = field.GetCustomAttribute<DotProductRangeVisualise>();
            return advancedPropertyAttribute != null;            
        }
    }
}
