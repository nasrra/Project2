using Entropek.Combat;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Ai.Contexts
{
    public interface IDamageIntervalContext
    {


        /// <summary>
        /// Components. 
        /// </summary>


        public Time.LoopedTimer DamageTakenIntervalTimer{get; protected set;}
        public EntityStats.HealthSystem SelfHealth{get; protected set;}


        /// 
        /// Data.
        /// 


        public float DamageTakenInCurrentInterval {get; protected set;}

        public void HaltEvaluationLoop()
        {
            DamageTakenInCurrentInterval = 0;
            DamageTakenIntervalTimer.Halt();
        }

        public void BeginEvaluationLoop()
        {
            DamageTakenIntervalTimer.Begin();
        }

        protected void LinkEvents()
        {
            SelfHealth.Damaged += OnHealthDamaged; 
        }

        protected void UnlinkEvents()
        {
            SelfHealth.Damaged -= OnHealthDamaged;             
        }

        /// <summary>
        /// Increments the counter for damage taken this interval by a specified amount.
        /// </summary>
        /// <param name="amount">The amount to increment by.</param>

        private void OnHealthDamaged(DamageContext damageContext)
        {
            DamageTakenInCurrentInterval += damageContext.DamageAmount;
        }
    }    
}

