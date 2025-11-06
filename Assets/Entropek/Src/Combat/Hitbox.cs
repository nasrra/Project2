using System;
using System.Collections.Generic;
using Entropek.EntityStats;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Combat
{
    // abstract class is this.
    // continous hitbox & timed hitbox are subclasses
    public abstract class Hitbox : MonoBehaviour
    {
        /// <summary>
        /// Returns the Gameobject (with a health component) hit as well as the position (in world space) that it was hit on.
        /// </summary>

        public event Action<GameObject, Vector3> HitHealth;

        /// <summary>
        /// Returns the Gamobject (without a health component) that was hit, as well as the position (in world space) that it was hit on.
        /// </summary>

        public event Action<GameObject, Vector3> HitOther;
        
        [Header("Data")]
        [Tooltip("This trigger collider must always start disabled.")]
        [SerializeField] protected Collider triggerCollider;
        [TagSelector] [SerializeField] protected string[] ignoreTags;
        [SerializeField] protected float damageAmount;
        [SerializeField] protected DamageType damageType;

        /// <summary>
        /// Activates the trigger collider for the hitbox, enabling detection of incoming hurtboxes.
        /// </summary>

        public virtual void Activate(){
            triggerCollider.enabled = true;
        }

        /// <summary>
        /// Deactivates the trigger collider for the hitbox, disabling detection of incoming hurtboxes.
        /// </summary>

        public virtual void Deactivate(){
            triggerCollider.enabled = false;            
        }

        /// <summary>
        /// Damages a health system, invoking the Hit callback.
        /// </summary>
        /// <param name="hitCollider">The specified collider to get the hit point on.</param>
        /// <param name="healthSystem">The specified health system to damage.</param>
        /// <exception cref="Exception">Thrown if there was no point found on the health system collider that classifies as a 'hit point'.</exception>

        protected void HitHealthSystemComponent(Collider hitCollider, HealthSystem healthSystem)
        {
            // get the hit point on the health collider. 

            if (GetHitPoint(hitCollider, out Vector3 hitPoint))
            {

                // damage the health component.

                healthSystem.Damage(new DamageContext(transform.position, damageAmount, damageType));

                // return that we hit the health object.

                HitHealth?.Invoke(healthSystem.gameObject, hitPoint);
            }
            else
            {
                throw new Exception($"'{name}' failed to retrieve a valid hit point to health compoonent collider on '{healthSystem.gameObject.name}'");
            }
        }

        /// <summary>
        /// Invokes the HitHealth event Action callback.
        /// </summary>
        /// <param name="hitGameObject">The gameobject that was hit.</param>
        /// <param name="hitPoint">The point along the hit colliders surface (in world space) that was hit.</param>

        protected void InvokeHitHealth(GameObject hitGameObject, Vector3 hitPoint)
        {
            HitHealth?.Invoke(hitGameObject.gameObject, hitPoint);
        }

        /// <summary>
        /// Invokes the HitOther event Action callback.
        /// </summary>
        /// <param name="hitGameObject">The gameobject that was hit.</param>
        /// <param name="hitPoint">The point along the hit colliders surface (in world space) that was hit.</param>

        protected void InvokeHitOther(GameObject hitGameObject, Vector3 hitPoint)
        {
            HitOther?.Invoke(hitGameObject.gameObject, hitPoint);
        }

        /// <summary>
        /// Performs a raycast intersection check from this hitbox position in world space to the hit collider.
        /// </summary>
        /// <param name="hitCollider">The specified collider to find a hit point on.</param>
        /// <param name="hitPoint">The raycast point of intersection in world space.</param>
        /// <returns>true, if a point was found.</returns>

        protected bool GetHitPoint(Collider hitCollider, out Vector3 hitPoint)
        {

            Vector3 directionToOther =  hitCollider.transform.position - transform.position;
            float distance = directionToOther.sqrMagnitude;
            Ray ray = new Ray(transform.position, directionToOther.normalized);
            RaycastHit hit;

            // get the hit point on the colliders surface.

            if (hitCollider.Raycast(ray, out hit, distance))
            {
                hitPoint = hit.point;
                return true;

            }

            // check if we're inside the collider

            else if (hitCollider.bounds.Contains(transform.position))
            {

                hitPoint = transform.position;
                return true;

            }

            // the hitbox has not hit anything.

            else
            {
                hitPoint = Vector3.zero;
                return false;
            }
        }

        /// <summary>
        /// Checks whether or not a collider is one of the specified colliders to ignore when hitting an object.
        /// </summary>
        /// <param name="collider">The specified collider to check against.</param>
        /// <returns>true, if it should be ignored. false, if not.</returns>

        protected bool IgnoreCollider(Collider collider)
        {
            for (int i = 0; i < ignoreTags.Length; i++)
            {
                if (collider.tag == ignoreTags[i])
                {
                    return true;
                }
            }

            return false;
        }

        protected abstract void OnTriggerEnter(Collider other);
    }

}

