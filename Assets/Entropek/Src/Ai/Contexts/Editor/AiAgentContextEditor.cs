using Entropek.UnityUtils;
using UnityEditor;
using UnityEngine;

namespace Entropek.Ai.Contexts
{
    [UnityEditor.CustomEditor(typeof(AiAgentContext), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class AiAgentContextEditor : RuntimeEditor<AiAgentContext>
    {}    
}

