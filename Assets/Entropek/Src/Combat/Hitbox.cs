using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entropek.Combat{
    
    [RequireComponent(typeof(Time.OneShotTimer))]
    public class Hitbox : MonoBehaviour{
        
        /// <summary>
        /// Returns the Gameobject hit and the position that it was hit on.
        /// </summary>

        public event Action<GameObject, Vector3> Hit;
        
        [Header("Components")]
        [SerializeField] private Collider col;
        [SerializeField] private Time.OneShotTimer timer;
        public Time.OneShotTimer Timer => timer;

        [Header("Data")]
        [SerializeField] Collider[] ignoreColliders = new Collider[0];
        private HashSet<int> hitGameObjectInstanceIds = new HashSet<int>();
        [SerializeField] private float damage;


        /// 
        /// Base.
        /// 


        void OnEnable(){
            Link();
        }

        void OnDisable(){
            Unlink();
        }

        void OnTriggerEnter(Collider hitCollider)
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

            // get the health system from the gameObject and 

            if(hitGameObject.TryGetComponent(out Hurtbox hurtbox))
            {
                HitHurtboxComponent(hitCollider, hurtbox);
            }
            else if (hitGameObject.TryGetComponent(out EntityStats.HealthSystem health))
            {
                HitHealthSystemComponent(hitCollider, health);
            }
            else
            {
                throw new Exceptions.ComponentNotFoundException(message: $"{gameObject.name} did not find Hurtbox or Health System on {hurtbox.gameObject.name}.");
            }



        }

        /// <summary>
        /// Damages a health system, invoking the Hit callback.
        /// </summary>
        /// <param name="hitCollider">The specified collider to get the hit point on.</param>
        /// <param name="healthSystem">The specified health system to damage.</param>
        /// <exception cref="Exception">Thrown if there was no point found on the health system collider that classifies as a 'hit point'.</exception>

        private void HitHealthSystemComponent(Collider hitCollider, EntityStats.HealthSystem healthSystem)
        {
            // get the hit point on the health collider. 

            if (GetHitPoint(hitCollider, out Vector3 hitPoint))
            {

                // damage the health component.

                healthSystem.Damage(damage);

                // return that we hit the health object.

                Hit?.Invoke(healthSystem.gameObject, hitPoint);
            }
            else
            {
                throw new Exception($"'{name}' failed to retrieve a valid hit point to health compoonent collider on '{healthSystem.gameObject.name}'");
            }
        }

        /// <summary>
        /// Damages a health system linked insides a hurtbox, invoking the Hit callback. 
        /// </summary>
        /// <param name="hitCollider">The specified collider to get the hit point on.</param>
        /// <param name="hurtbox">The specified hurtbox to extract a health system to damage.</param>
        /// <exception cref="Exception">Thrown if there was no point found on the health system collider that classifies as a 'hit point'.</exception>

        private void HitHurtboxComponent(Collider hitCollider, Hurtbox hurtbox)
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

            if (GetHitPoint(hitCollider, out Vector3 hitPoint))
            {

                // damage the health component tied to the hurtbox.

                hurtbox.Health.Damage(damage);

                // return that we hit the health object, not the hurtbox.

                Hit?.Invoke(healthGameObject, hitPoint);
            }
            else
            {
                throw new Exception($"'{name}' failed to retrieve a valid hit point to hurtbox '{hurtbox.gameObject.name}'");
            }

        }

        /// <summary>
        /// Performs a raycast intersection check from this hitbox position in world space to the hit collider.
        /// </summary>
        /// <param name="hitCollider">The specified collider to find a hit point on.</param>
        /// <param name="hitPoint">The raycast point of intersection in world space.</param>
        /// <returns>true, if a point was found.</returns>

        private bool GetHitPoint(Collider hitCollider, out Vector3 hitPoint)
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

        private bool IgnoreCollider(Collider collider)
        {
            for (int i = 0; i < ignoreColliders.Length; i++)
            {
                if (collider == ignoreColliders[i])
                {
                    return true;
                }
            }

            return false;
        }


        /// 
        /// Functions.
        /// 


        public void Enable()
        {
            col.enabled = true;
            timer.Begin();
        }

        public void Enable(float time)
        {
            col.enabled = true;
            timer.Begin(time);
        }
        

        /// 
        /// Linkage.
        /// 


        private void Link(){
            timer.Timeout += OnTimerTimeout;
        }

        private void Unlink(){
            timer.Timeout -= OnTimerTimeout;
        }

        private void OnTimerTimeout(){
            col.enabled = false;
            hitGameObjectInstanceIds.Clear();
        }
    }

}
