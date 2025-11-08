using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Entropek.UnityUtils.Attributes
{
    [CustomPropertyDrawer(typeof(NavMeshAreaMaskField))]
    public class NavMeshAreaFieldPropertyDrawer : PropertyDrawer
    {
        static string[] areaNames;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUILayout.HelpBox("The [NavMeshAreaField] Attribute can only be implemented by an Integer field.", MessageType.Error);
                return;
            }

            // retrieve area names if we havent done so already.

            areaNames??=NavMesh.GetAreaNames();            
            
            int currentIndex = Mathf.Clamp(property.intValue, 0, areaNames.Length);

            // draw popup.
            
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, areaNames);

            // assign when changed.

            if(newIndex != currentIndex)
            {
                property.intValue = newIndex;
            }
        }
    }
}

