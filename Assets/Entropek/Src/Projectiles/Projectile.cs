using System;
using System.Collections;
using Entropek.Combat;
using UnityEngine;

namespace Entropek.Projectiles
{
    public class Projectile : MonoBehaviour, IDeactivatable{

        public event Action Deactivated;
        public event Action Activated;

        [Header("Components")]
        [SerializeField] ContinuousHitbox hitbox;
        [SerializeField] private float speed;
        [SerializeField] private float lifetime;
        public float Speed => speed;


        /// 
        /// Base.
        /// 


        private void Awake()
        {
            LinkEvents();
        }

        private void OnEnable()
        {
            StartCoroutine(Lifetime());
        }

        private void LateUpdate()
        {
            transform.position += transform.forward * speed * UnityEngine.Time.deltaTime;
        }

        private void OnDisable()
        {
            // stop lifetime coroutine.

            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            UnlinkEvents();
        }


        ///
        /// Lifetime.
        /// 


        /// <summary>
        /// The IEnumerator (Unity Coroutine) that determines how long a projectile will be active for.
        /// </summary>
        /// <returns></returns>

        private IEnumerator Lifetime()
        {
            yield return new WaitForSeconds(lifetime);
            Deactivate();
        }


        /// 
        /// Pooling.
        /// 


        public void Deactivate()
        {
            gameObject.SetActive(false);
            Deactivated?.Invoke();
        }

        public void Activate()
        {
            gameObject.SetActive(true);
            Activated?.Invoke();            
        }


        /// 
        /// Linkage.
        /// 


        private void LinkEvents()
        {
            LinkHitboxEvents();
        }

        private void UnlinkEvents()
        {
            UnlinkHitboxEvents();
        }

        private void LinkHitboxEvents()
        {
            hitbox.HitHealth += OnHitHealth;
            hitbox.HitOther += OnHitOther;
        }

        private void UnlinkHitboxEvents()
        {
            hitbox.HitHealth -= OnHitHealth;
            hitbox.HitOther -= OnHitOther;            
        }

        private void OnHitHealth(GameObject hitGameObject, Vector3 hitPoint)
        {
            Deactivate();
        }

        private void OnHitOther(GameObject hitGameObject, Vector3 hitPoint)
        {
            Deactivate();
        }
    }
}

