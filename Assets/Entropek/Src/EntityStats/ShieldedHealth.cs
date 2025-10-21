using System;
using Entropek.Time;
using UnityEngine;

namespace Entropek.EntityStats{


    public class ShieldedHealth : Health{


        /// 
        /// Callbacks.
        /// 


        public event Action<float> ShieldRestored;
        public event Action<float> ShieldDamaged;
        public event Action ShieldFull;
        public event Action ShieldBroken;
        public event Action ShieldActive;


        /// 
        /// Data.
        /// 


        [Header(nameof(ShieldedHealth))]
        [SerializeField] protected Timer shieldBrokenStateTimer; 
        [SerializeField] protected float shieldValue;
        [SerializeField] protected float maxShieldValue;
        protected ShieldState shieldState;
        public ShieldState ShieldState => shieldState;


        /// 
        /// Base.
        /// 


        protected override void OnEnable(){
            base.OnEnable();
            SetIntialShieldState();
            LinkEvents();
        }

        private void OnDisable(){
            UnlinkEvents();
        }


        /// 
        /// State Machine.
        /// 

        
        private void SetIntialShieldState(){
            if(shieldValue <= 0){
                shieldState = ShieldState.Depleted;
            }
            else if(shieldValue == maxShieldValue){
                shieldState = ShieldState.Full;
            }
            else{
                shieldState = ShieldState.Active;
            }
        }

        private void ShieldActiveState(){
            
            if(shieldState == ShieldState.Active){
                return;
            }

            shieldState = ShieldState.Active;
            ShieldActive?.Invoke();
        }

        private void ShieldFullState(){
            
            if(shieldState == ShieldState.Full){
                return;
            }
            
            shieldState = ShieldState.Full;
            ShieldFull?.Invoke();   
        }

        private void ShieldBrokenState(){
            
            if(shieldState == ShieldState.Broken){
                return;
            }

            shieldBrokenStateTimer.Begin();
            shieldState = ShieldState.Broken;
            ShieldBroken?.Invoke();
        }


        /// 
        /// Util Functions.
        /// 


        public override bool Damage(float amount){
            
            bool damaged;
            
            // shield absorbs damage until broken.
            
            damaged = DamageShield(amount);

            if(damaged == false){
                damaged = DamageHealth(amount);
            }

            return damaged;
        }

        public bool DamageShield(float amount){

            if(Vulnerable==false 
            || shieldState == ShieldState.Broken 
            || shieldState == ShieldState.Depleted){
                return false;
            }

            
            shieldValue -= amount;
            if(shieldValue <= 0){
                ShieldBrokenState();
            }
            else{
                ShieldActiveState();
                ShieldDamaged?.Invoke(amount);
            }
            return true;
        }


        /// 
        /// Getters.
        /// 


        public float GetShieldValue(){
            return shieldValue;
        }

        public float GetMaxShieldValue(){
            return maxShieldValue;
        }

        public float GetNormalisedShieldValue(){
            return shieldValue / maxShieldValue;
        }

        public void RestoreShield(float amount){
            
            // dont restore shields when broken.
            
            if(shieldState == ShieldState.Broken){
                return;
            }

            shieldValue += amount;
            if(shieldValue >= maxShieldValue){
                ShieldFullState();
            }
            else{
                ShieldRestored?.Invoke(amount);
            }
        }


        /// 
        /// Linkage.
        /// 


        private void LinkEvents(){
            LinkTimerEvents();
        }

        private void UnlinkEvents(){
            UnlinkTimerEvents();
        }

        private void LinkTimerEvents(){
            shieldBrokenStateTimer.Timeout += OnShieldBrokenStateTimeout;
        }

        private void UnlinkTimerEvents(){
            shieldBrokenStateTimer.Timeout -= OnShieldBrokenStateTimeout;
        }

        private void OnShieldBrokenStateTimeout(){
            ShieldActiveState();
        }
    }


}

