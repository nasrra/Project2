using UnityEngine;

namespace Entropek.Combat
{

    public struct DamageContext
    {
        /// <summary>
        /// Gets the type of damage stored by this DamageContext.
        /// </summary>

        public DamageType DamageType{get; private set;}
        
        /// <summary>
        /// Gets the amount of damage to deal to a HealthSystem component.
        /// </summary>
        
        public float DamageAmount{get; private set;}
        
        /// <summary>
        /// Gets the position (in world-space) where this DamageContext came from.
        /// </summary>

        [HideInInspector] public Vector3 SourcePosition{get; private set;}

        /// <summary>
        /// Creates a DamageContext instance.
        /// </summary>
        /// <param name="sourcePosition">The position (in world-space) where this DamageContext came from.</param>
        /// <param name="amount">The amount of damage to deal.</param>
        /// <param name="type">The type of damage.</param>

        public DamageContext(Vector3 sourcePosition, float damageAmount, DamageType damageType)
        {
            SourcePosition = sourcePosition;
            DamageAmount = damageAmount;
            DamageType = damageType;
        }
    }
}
