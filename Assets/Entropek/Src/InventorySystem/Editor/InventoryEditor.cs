using Entropek.UnityUtils;
using UnityEditor;
using UnityEngine;

namespace Entropek.InventorySystem
{
    [CustomEditor(typeof(Inventory))]
    [CanEditMultipleObjects]
    public class InventoryEditor : RuntimeEditor<Inventory>
    {}    
}

