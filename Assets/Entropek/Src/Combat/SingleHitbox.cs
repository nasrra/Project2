using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entropek.Combat
{

    /// <summary>
    /// A Hitbox that registers a hit on a Hurtbox exculsively on the first time it enters the trigger collider.
    /// </summary>

    public class SingleHitbox : Hitbox
    {        
        [Header("Data")]
        protected HashSet<int> hitGameObjectInstanceIds = new HashSet<int>();

        void OnDisable()
        {
            // clear the hit gameobjects whenevering disabling so when it this is
            // re-enabled, it wont have redundant data from the previous lifetime.

            hitGameObjectInstanceIds.Clear();
        }

        /// <summary>
        /// Hit an incoming hurtbox if it is outside the ignore collider parameters and has not already been hit during the current activation interval.
        /// </summary>
        /// <param name="hitCollider"></param>
        /// <exception cref="Exceptions.ComponentNotFoundException"></exception>

        protected override void OnTriggerEnter(Collider hitCollider)
        {
            if (IgnoreCollider(hitCollider) == true)
            {
                return;
            }

            GameObject hitGameObject = hitCollider.gameObject;
            int otherId = hitGameObject.GetInstanceID();

            // short-circuit if we've already hit the collider object.

            if (hitGameObjectInstanceIds.Contains(otherId) == true)
            {
                return;
            }

            // chache the hit the collider game object. 

            hitGameObjectInstanceIds.Add(otherId);

            // Damage the health system of the hit gameobject.

            if(hitGameObject.TryGetComponent(out Hurtbox hurtbox))
            {
                // ensure that the health component hasn't already been hit by another 
                // linked hurtbox previously being hit.
                // a single entity may have multiple hurtboxes that share the same health component.

                GameObject healthGameObject = hurtbox.Health.gameObject;
                int healthInstanceId = healthGameObject.GetInstanceID();

                if (hitGameObjectInstanceIds.Contains(healthInstanceId))
                {
                    return;
                }

                hitGameObjectInstanceIds.Add(healthInstanceId);

                // get the hit point on the hurtbox collider. 

                HitHealthSystemComponent(hitCollider, hurtbox.Health);
            }
            else if (hitGameObject.TryGetComponent(out EntityStats.HealthSystem health))
            {
                HitHealthSystemComponent(hitCollider, health);
            }
            else
            {
                GetHitPoint(hitCollider, out Vector3 hitPoint);
                InvokeHitOther(hitGameObject, hitPoint);
            }
        }

        public override void Enable()
        {
            base.Enable();
            hitGameObjectInstanceIds.Clear(); // clear the cached list for the next call.      
        }

    }    
}

