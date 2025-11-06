using System;
using Unity.XR.OpenVR;
using UnityEngine;

namespace Entropek.EntityStats{


    public class Health : HealthSystem{
        

        /// 
        /// Data.
        /// 


        [Header("Data")]
        [SerializeField] protected float healthValue;
        [SerializeField] protected float maxHealthValue;
        [HideInInspector] protected HealthState healthState;
        public HealthState HealthState => healthState;


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
            if(healthValue <= 0){
                healthState = HealthState.Dead;
            }
            else if(healthValue >= maxHealthValue){
                healthState = HealthState.Full;
            }
            else{
                healthState = HealthState.Alive;
            }
        }

        protected void HealthAliveState(){
            if(healthState==HealthState.Alive){
                return;
            }
            healthState=HealthState.Alive;
        }

        protected void HealthFullState(){
            if(healthState==HealthState.Full){
                return;
            }
            healthState=HealthState.Full;
            InvokeHealthFull();
        }

        protected void HealthDeadState(){
            if(healthState==HealthState.Dead){
                return;
            }
            healthState=HealthState.Dead;
            InvokeDeath();
        }


        /// 
        /// Util Functions.
        /// 


        public override bool Damage(float amount){
            return DamageHealth(amount);
        }

        protected bool DamageHealth(float amount){
            if(Vulnerable == false || healthState == HealthState.Dead){
                return false;
            }

            healthValue -= amount;
            if(healthValue<=0){
                HealthDeadState();
            }
            else{
                HealthAliveState();
                InvokeHealthDamaged(amount);
            }

            return true;
        }

        public override void Heal(float amount){
            healthValue += amount;
            if(healthValue >= maxHealthValue){
                HealthFullState();
            }
            else{
                HealthAliveState();
                InvokeHealed(amount);
            }
        }


        /// 
        /// Getters.
        /// 


        public override float GetHealthValue(){
            return healthValue;
        }

        public override float GetMaxHealthValue(){
            return maxHealthValue;
        }

        public override float GetNormalisedHealthValue(){
            return healthValue / maxHealthValue;
        }


    }


}
