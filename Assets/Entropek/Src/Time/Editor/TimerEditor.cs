using Entropek.UnityUtils;
using UnityEditor;
using UnityEngine;

namespace Entropek.Time{

    [UnityEditor.CustomEditor(typeof(Timer), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class TimerEditor : RuntimeEditor<Timer>{}


}

