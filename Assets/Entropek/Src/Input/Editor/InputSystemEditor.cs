using Entropek.UnityUtils;
using UnityEditor;
using UnityEngine;

namespace Entropek.Input
{
    [UnityEditor.CustomEditor(typeof(InputSystem), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class InputSystemEditor : RuntimeEditor<InputSystem>{}
}

