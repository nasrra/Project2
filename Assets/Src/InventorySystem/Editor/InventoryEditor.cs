using Entropek.UnityUtils;
using UnityEditor;
using UnityEngine;

namespace Entropek.InventorySystem
{
    [UnityEditor.CustomEditor(typeof(Inventory))]
    [CanEditMultipleObjects]
    public class InventoryEditor : RuntimeEditor<Inventory>
    {}    
}

