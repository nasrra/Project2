using System;
using Entropek.Combat;
using UnityEngine;

namespace Entropek.EntityStats{

    [Serializable]
    public abstract class HealthSystem : MonoBehaviour{
        
        public event Action<float> Healed;
        public event Action<DamageContext> Damaged;
        public event Action Death;
        public event Action Restored;

        public abstract float GetNormalisedValue();
        public abstract bool Damage(in DamageContext damageContext);
        public abstract void Heal(float amount);

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

        protected void InvokeHealed(float amount){
            Healed?.Invoke(amount);
        }

    }


}

