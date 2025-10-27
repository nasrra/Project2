using UnityEditor;
using UnityEngine;

namespace Entropek.Physics
{
    [CustomEditor(typeof(GroundCheck), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class GroundCheckEditor : UnityUtils.RuntimeEditor<GroundCheck>
    {
        
    }
    
}

