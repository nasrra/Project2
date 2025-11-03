using UnityEngine;
using UnityEditor;

namespace Entropek.UnityUtils.Attributes
{

    /// <summary>
    /// This is a custom property drawerer in the inspector for serialized fields using
    /// the [TagSelector] attribute. Seriliasing the underling string value as a unity tag drop down.
    /// </summary>

    [CustomPropertyDrawer(typeof(TagSelector))]
    public class TagSelectorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(property.propertyType == SerializedPropertyType.String)
            {
                // draw as a tag field when underling field is a string.
                // storing the value in said string.

                property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
            }
            else
            {
                // draw the field normally.

                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}

