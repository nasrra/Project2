using UnityEditor;
using UnityEngine;

namespace Entropek.UnityUtils.Attributes
{
    public abstract class AdvancedPropertyAttribute : PropertyAttribute
    {
        public abstract void OnInspectorGUI(SerializedProperty serializedProperty);
        public abstract void OnSceneGUI(Object target);
    }    
}

