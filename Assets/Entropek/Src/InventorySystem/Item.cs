using UnityEngine;

namespace Entropek.InventorySystem
{
    public abstract class Item : ScriptableObject
    {
        #if UNITY_EDITOR

        /// <summary>
        /// The asset menu name for creating items in the unity editor.
        /// </summary>

        protected const string EditorAssetMenuName = "Entropek/InventorySystem/";

        #endif

        [SerializeField] private ushort id;
        public ushort Id => id;

        public abstract void Use();
        public abstract bool CanUse();
        public abstract void GetRequiredComponents();
    }
}

