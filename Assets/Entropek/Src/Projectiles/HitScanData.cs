using Entropek.Combat;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Projectiles
{
    [System.Serializable]
    public struct HitScanData
    {
        [TagSelector, SerializeField] private string[] ignoreTags;
        public string[] IgnoreTags => ignoreTags;

        [SerializeField] private float sphereCastRadius;
        public float SphereCastRadius => sphereCastRadius;

        [SerializeField] private int damageAmount;
        public int DamageAmount => damageAmount;

        [SerializeField] private DamageType damageType;
        public DamageType DamageType => damageType;

        /// Layers to hit should include obstruction layers as well.

        [SerializeField] private LayerMask hitLayers;
        public LayerMask HitLayers => hitLayers;
        
        /// Layers that are classified as obstructions.

        [SerializeField] private LayerMask obstructionLayers;
        public LayerMask ObstructionLayers => obstructionLayers;
    }    
}

