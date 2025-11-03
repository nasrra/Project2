using Entropek.UnityUtils;
using UnityEditor;
using UnityEngine;


namespace Entropek.Ai.Combat
{
    [CustomEditor(typeof(ComplexAiCombatAgent), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class ComplexAiCombatAgentEditor : AiCombatAgentEditor<ComplexAiCombatAgent,ComplexAiCombatAction>{}    
}
