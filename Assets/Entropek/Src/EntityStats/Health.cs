using System;
using Entropek.Combat;
using Unity.XR.OpenVR;
using UnityEngine;

namespace Entropek.EntityStats{


    public class Health : HealthSystem{
        

        /// 
        /// Callbacks.
        /// 


        public event Action<int> MaxValueSet;


        /// 
        /// Data.
        /// 


        [Header("Data")]
        [SerializeField] private int value;
        public int Value => value;
        [SerializeField] private int maxValue;
        public int MaxValue => maxValue;
        [HideInInspector] protected HealthState healthState;
        public HealthState HealthState => healthState;

        public override int GetNormalisedValue()
        {
            return value / maxValue;    
        }
        



        /// 
        /// Base.
        /// 


        protected virtual void OnEnable(){
            SetInitialHealthState();
            value = maxValue;
        }


        /// 
        /// State Machine.
        /// 


        private void SetInitialHealthState(){
            if(value <= 0){
                healthState = HealthState.Dead;
            }
            else if(value >= maxValue){
                healthState = HealthState.Full;
            }
            else{
                healthState = HealthState.Alive;
            }
        }

        protected void AliveState(){
            if(healthState==HealthState.Alive){
                return;
            }
            healthState=HealthState.Alive;
        }

        protected void RestoredState(){
            if(healthState==HealthState.Full){
                return;
            }
            healthState=HealthState.Full;
            InvokeRestored();
        }

        protected void DeathState(){
            if(healthState==HealthState.Dead){
                return;
            }
            healthState=HealthState.Dead;
            InvokeDeath();
        }


        /// 
        /// Util Functions.
        /// 


        public override bool Damage(in DamageContext damageContext){
            
            if(Vulnerable == false || healthState == HealthState.Dead){
                return false;
            }

            value -= damageContext.DamageAmount;
            if(value<=0){
                DeathState();
            }
            else{
                AliveState();
                InvokeDamaged(damageContext);
            }

            return true;
        }

        public override void Heal(int amount){
            value += amount;
            if(value >= maxValue){
                RestoredState();
            }
            else{
                AliveState();
                InvokeHealed(amount);
            }
        }

        /// <summary>
        /// Sets the max value of this Health instance.
        /// </summary>
        /// <param name="value">The value to set as the maximum health value.</param>

        public void SetMaxValue(int value)
        {
            maxValue = value;
            if(this.value > maxValue)
            {
                this.value = maxValue;
            }
            MaxValueSet?.Invoke(value);
        }
    }


}
