// using System;
// using System.Reflection;
// using UnityEditor;
// using UnityEngine;


// namespace Entropek.UnityUtils.Attributes
// {
//     [CustomPropertyDrawer(typeof(DotProductRangeVisualise))]
//     public class DotProductRangeVisualisePropertyDrawer : PropertyDrawer
//     {
//         private bool drawVisualiser = false;

//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         {

//             Type fieldType = fieldInfo.FieldType;
//             EditorGUI.PropertyField(position, property, label);
//             // if (fieldType == typeof(DotProductRange))
//             // {
//             //     DrawToggle();
//             // }
//             // else
//             // {
//             //     EditorGUILayout.HelpBox("The attribute [DotProductRangeVisualise] can only be implemented with DotProductRange structs", MessageType.Error);
//             // }
//         }


//         ///
//         /// FOV debug dusplay.
//         /// 


//         private void DrawToggle()
//         {
//             drawVisualiser = EditorGUILayout.BeginToggleGroup("Action FOV Editor", drawVisualiser);
//             EditorGUILayout.EndToggleGroup();
//         }

//     }    
// }
