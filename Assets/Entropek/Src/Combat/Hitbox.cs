using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entropek.Combat{
    
    [RequireComponent(typeof(Collider))]
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

        void OnTriggerEnter(Collider hurtboxCollider){
            
            GameObject hurtboxGameObject = hurtboxCollider.gameObject;
            int otherId = hurtboxGameObject.GetInstanceID();

            // short-circuit if we've already hit the collider object.

            if (hitGameObjectInstanceIds.Contains(otherId) == true)
            {
                return;
            }

            // chache the hit the collider game object. 

            hitGameObjectInstanceIds.Add(otherId);

            // get the health system from the gameObject and 
            
            EntityStats.HealthSystem health;

            health = hurtboxGameObject.GetComponent<EntityStats.HealthSystem>();

            if (health == null)
            {
                health = hurtboxGameObject.GetComponent<Hurtbox>().Health;
            }

            // check if weve already hit this health.
            // a single entity may have multiple hurtboxes that share the same health component.

            GameObject healthGameObject = health.gameObject;
            int healthInstanceId = healthGameObject.GetInstanceID();

            if (hitGameObjectInstanceIds.Contains(healthInstanceId))
            {
                return; // this hitbox has already damaged the health component.
            }

            hitGameObjectInstanceIds.Add(healthInstanceId);

            // get the hit point on the hurtbox collider. 

            Vector3 directionToOther = hurtboxGameObject.transform.position - transform.position;
            float distanceToOther = directionToOther.sqrMagnitude;
            int layerMask = 1 << hurtboxGameObject.layer; // bit shift to get actual layer mask.

            if (UnityEngine.Physics.Raycast(transform.position, directionToOther.normalized, out RaycastHit hit, distanceToOther, layerMask))
            {
                // damage and call back that weve hit the health components gameobject.

                health.Damage(damage);
                Hit?.Invoke(healthGameObject, hit.point);
            }

            
        }

        /// 
        /// Functions.
        /// 


        public void Enable(){
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
