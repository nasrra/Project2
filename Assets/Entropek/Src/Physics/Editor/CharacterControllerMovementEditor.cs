using Entropek.UnityUtils;
using UnityEditor;
using UnityEngine;

namespace Entropek.Physics
{    
    [UnityEditor.CustomEditor(typeof(CharacterControllerMovement), true)]
    [CanEditMultipleObjects]
    public class CharacterControllerMovementEditor : RuntimeEditor<CharacterControllerMovement>{}
}

