using System;
using Entropek.EntityStats;
using Entropek.Exceptions;
using UnityEngine;

namespace Entropek.Combat
{
    /// <summary>
    /// A Hitbox that registers a hit on a Hurtbox everytime it enters the trigger collider.
    /// </summary>

    public class ContinuousHitbox : SingleHitbox
    {
        /// <summary>
        /// Removes an outgoing hurtbox if it is outside the ignore collider parameters.
        /// </summary>
        /// <param name="hitCollider"></param>
        /// <exception cref="Exceptions.ComponentNotFoundException"></exception>

        protected void OnTriggerExit(Collider hitCollider)
        {
            if(IgnoreCollider(hitCollider) == true)
            {
                return;
            }

            int hitGameObjectId = hitCollider.gameObject.GetInstanceID();

            if(hitGameObjectInstanceIds.Contains(hitGameObjectId))
            {
                hitGameObjectInstanceIds.Remove(hitGameObjectId);
            }
        }
    }
}
