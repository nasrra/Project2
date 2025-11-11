using System;
using Entropek.Combat;
using UnityEngine;

namespace Entropek.EntityStats{

    [Serializable]
    public abstract class HealthSystem : MonoBehaviour{
        
        public event Action<int> Healed;
        public event Action<DamageContext> Damaged;
        public event Action Death;
        public event Action Restored;

        public abstract int GetNormalisedValue();

        /// <summary>
        /// Damages this Health instance
        /// </summary>
        /// <param name="damageContext">The context to apply damage with.</param>
        /// <returns>true, if damage was succesfully dealt (in vulnerable state); otherwise false (invulneable state).</returns>

        public abstract bool Damage(in DamageContext damageContext);

        /// <summary>
        /// Heals this Health instance.
        /// </summary>
        /// <param name="amount">The amount to heal.</param>

        public abstract void Heal(int amount);

        [Header(nameof(HealthSystem))]
        public bool Vulnerable = true; 

        protected void InvokeDeath(){
            Death?.Invoke();
        }
        
        protected void InvokeDamaged(in DamageContext damageContext){
            Damaged?.Invoke(damageContext);
        }

        protected void InvokeRestored(){
            Restored?.Invoke();
        }

        protected void InvokeHealed(int amount){
            Healed?.Invoke(amount);
        }

    }


}

