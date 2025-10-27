using System;
using UnityEngine;

namespace Entropek.UnityUtils.Attributes
{

    /// <summary>
    /// Marks a private or internal field to be shown in a custom
    /// Runtime Editor for a GameObject Component in the inspector;
    /// Only while in Play Mode (intended for debugging).
    /// </summary>

    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class RuntimeField : PropertyAttribute{}    
}

