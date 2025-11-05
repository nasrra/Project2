using Entropek.UnityUtils;
using UnityEditor;
using UnityEngine;


namespace Entropek.Ai
{
    [CustomEditor(typeof(ComplexAiActionAgent), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class ComplexAiActionAgentEditor : AiActionAgentEditor<ComplexAiActionAgent,ComplexAiAction>{}    
}
