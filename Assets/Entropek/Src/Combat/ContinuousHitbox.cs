using System;
using Entropek.EntityStats;
using Entropek.Exceptions;
using UnityEngine;

namespace Entropek.Combat
{
    /// <summary>
    /// A Hitbox that registers a hit on a Hurtbox everytime it enters the trigger collider.
    /// </summary>

    public class ContinuousHitbox : Hitbox
    {
        /// <summary>
        /// Hit an incoming hurtbox if it is outside the ignore collider parameters.
        /// </summary>
        /// <param name="hitCollider"></param>
        /// <exception cref="Exceptions.ComponentNotFoundException"></exception>

        protected override void OnTriggerEnter(Collider hitCollider)
        {
            if(IgnoreCollider(hitCollider) == true)
            {
                return;
            }

            GameObject hitGameObject = hitCollider.gameObject;

            // Damage the health system of the hit gameobject.

            if(hitGameObject.TryGetComponent(out Hurtbox hurtbox))
            {
                HitHealthSystemComponent(hitCollider, hurtbox.Health);
            }
            else if(hitGameObject.TryGetComponent(out HealthSystem healthSystem))
            {
                HitHealthSystemComponent(hitCollider, healthSystem);
            }
            else
            {
                throw new ComponentNotFoundException($"{gameObject.name} did not find Hurtbox or HealthSystem component on {hitGameObject.name}");
            }
        }
    }
}
