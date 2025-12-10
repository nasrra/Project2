using UnityEngine;

namespace Entropek.Combat
{    
    public struct HitboxHitContext
    {

        /// <summary>
        /// The hitbox that invoked the hit callback.
        /// </summary>

        public readonly Hitbox Hitbox;

        /// <summary>
        /// The GameObject that was hit by the hitbox.
        /// </summary>

        public readonly GameObject HitGameObject;

        /// <summary>
        /// The point in world-space along the GameObjects surface that was hit.
        /// </summary>

        public readonly Vector3 HitPoint;

        /// <summary>
        /// Creates a new HitboxHitContext.
        /// </summary>
        /// <param name="hitbox">The hitbox that invoked the hit callback.</param>
        /// <param name="hitGameObject">The GameObject that was hit by the hitbox</param>
        /// <param name="hitPoint">The point in world-space along the GameObjects surface that was hit.</param>

        public HitboxHitContext(Hitbox hitbox, GameObject hitGameObject, Vector3 hitPoint)
        {
            Hitbox = hitbox;
            HitGameObject = hitGameObject;
            HitPoint = hitPoint;
        } 
    }
}

