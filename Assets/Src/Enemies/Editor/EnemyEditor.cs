using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Enemy), editorForChildClasses: true)]
[CanEditMultipleObjects]
public class EnemyEditor : Entropek.UnityUtils.CustomEditor{}
