using System;
using Entropek.Combat;
using Unity.XR.OpenVR;
using UnityEngine;

namespace Entropek.EntityStats{


    public class Health : HealthSystem{
        

        /// 
        /// Data.
        /// 


        [Header("Data")]
        [SerializeField] private float value;
        public float Value => value;
        [SerializeField] private float maxValue;
        public float MaxValue => maxValue;
        [HideInInspector] protected HealthState healthState;
        public HealthState HealthState => healthState;

        public override float GetNormalisedValue()
        {
            return value / maxValue;    
        }
        



        /// 
        /// Base.
        /// 


        protected virtual void OnEnable(){
            SetInitialHealthState();
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

        public override void Heal(float amount){
            value += amount;
            if(value >= maxValue){
                RestoredState();
            }
            else{
                AliveState();
                InvokeHealed(amount);
            }
        }
    }


}
