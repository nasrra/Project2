using UnityEditor;
using UnityEngine;

namespace Entropek.Ai
{
    [CustomEditor(typeof(BasicAiActionAgent))]
    [CanEditMultipleObjects]
    public class BasicAiActionAgentEditor : AiActionAgentEditor<BasicAiActionAgent, BasicAiAction>{}    
}


