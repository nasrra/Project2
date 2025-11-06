using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Entropek.Ai{

    /// <summary>
    /// This is the generic editor class for providing the base class inspector gui
    /// for AiCombatAgent subclasses. Inherit from this when creating any editor for a
    /// Combat agent type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>

    [UnityEditor.CustomEditor(typeof(AiStateAgent))]
    [CanEditMultipleObjects]
    public class AiStateAgentEditor : AiAgentEditor
    {}
}
