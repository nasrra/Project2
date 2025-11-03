using UnityEditor;
using UnityEngine;

namespace Entropek.Ai.Combat
{
    [CustomEditor(typeof(BasicAiCombatAgent))]
    [CanEditMultipleObjects]
    public class BasicAiCombatAgentEditor : AiCombatAgentEditor<BasicAiCombatAgent, BasicAiCombatAction>{}    
}


