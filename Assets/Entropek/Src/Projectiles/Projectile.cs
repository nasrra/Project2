using System;
using System.Collections;
using Entropek.Audio;
using Entropek.Combat;
using Entropek.Time;
using Entropek.Vfx;
using UnityEngine;

namespace Entropek.Projectiles
{
    public class Projectile : MonoBehaviour, IDeactivatable{
        
        public event Action Deactivated;
        public event Action Activated;

        [Header("Projectile Components")]
        [SerializeField] Hitbox hitbox;
        [SerializeField] private float speed;
        [SerializeField] private OneShotTimer lifetimeTimer;
        [SerializeField] private bool deactivateOnHitHealth = true;
        [SerializeField] private bool deactivateOnHitOther = true;
        public float Speed => speed;
        private bool paused = false;


        /// 
        /// Base.
        /// 


        private void Awake()
        {
            LinkEvents();
        }

        protected virtual void OnEnable()
        {
            lifetimeTimer.Begin();
        }

        protected virtual void FixedUpdate()
        {
            if(paused == true)
            {   
                return;
            }

            transform.position += transform.forward * speed * UnityEngine.Time.deltaTime;
        }

        protected virtual void OnDisable()
        {
            // stop lifetime coroutine.

            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            UnlinkEvents();
        }

        public void Pause()
        {
            lifetimeTimer.Pause();
            paused = true;
        }

        public void Resume()
        {
            lifetimeTimer.Resume();            
            paused = false;
        }


        /// 
        /// Pooling.
        /// 


        public virtual void Deactivate()
        {
            gameObject.SetActive(false);
            Deactivated?.Invoke();
        }

        public virtual void Activate()
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

        protected virtual void OnHitHealth(HitboxHitContext context)
        {
            if (deactivateOnHitHealth == true)
            {
                Deactivate();
            }
        }

        protected virtual void OnHitOther(HitboxHitContext context)
        {
            if (deactivateOnHitOther == true)
            {
                Deactivate();
            }
        }
    }
}

