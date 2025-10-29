using Entropek.UnityUtils;
using UnityEditor;
using UnityEngine;

namespace Entropek.Input
{
    [CustomEditor(typeof(InputSystem), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class InputSystemEditor : RuntimeEditor<InputSystem>{}
}

