using System;
using Entropek.Combat;
using Entropek.Time;
using Entropek.UnityUtils.Attributes;
using Unity.XR.OpenVR;
using UnityEngine;

namespace Entropek.EntityStats{


    public class Health : MonoBehaviour{
        

        /// 
        /// Bleed.
        /// 


        private const int BleedDamagePerStack = 3;
        private const int MaxBleedTicksPerInterval = 3;
        private const int BleedTickTimeInSeconds = 1;


        /// 
        /// Callbacks.
        /// 


        public event Action<int> MaxValueSet;
        public event Action<int> Healed;
        public event Action<DamageContext> Damaged;
        public event Action Death;
        public event Action Restored;


        /// 
        /// Components.
        /// 


        [Header("Components")]
        [SerializeField] LoopedTimer bleedTickTimer;


        /// 
        /// Data.
        /// 


        [Header("Data")]
        [SerializeField] private int value;
        public int Value => value;
        [SerializeField] private int maxValue;
        public int MaxValue => maxValue;
        [RuntimeField] private int bleedStacks = 0;
        [RuntimeField] private int bleedCurrentTick = MaxBleedTicksPerInterval;
        [HideInInspector] protected HealthState healthState;
        public HealthState HealthState => healthState;
        public bool Vulnerable = true; 



        /// 
        /// Base.
        /// 


        private void Awake()
        {
            bleedTickTimer.SetInitialTime(BleedTickTimeInSeconds);
            LinkEvents();
        }

        protected virtual void OnEnable(){
            SetInitialHealthState();
            value = maxValue;
        }

        private void OnDestroy()
        {
            UnlinkEvents();
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
            Restored?.Invoke();
        }

        protected void DeathState(){
            if(healthState==HealthState.Dead){
                return;
            }
            healthState=HealthState.Dead;
            Death?.Invoke();
        }


        /// 
        /// Util Functions.
        /// 


        public int GetNormalisedValue()
        {
            return value / maxValue;    
        }        

        /// <summary>
        /// Damages this Health instance
        /// </summary>
        /// <param name="damageContext">The context to apply damage with.</param>
        /// <returns>true, if damage was succesfully dealt (in vulnerable state); otherwise false (invulneable state).</returns>

        public bool Damage(in DamageContext damageContext){
            
            if(Vulnerable == false || healthState == HealthState.Dead){
                return false;
            }

            value -= damageContext.DamageAmount;
            if(value<=0){
                DeathState();
            }
            else{
                AliveState();
                Damaged?.Invoke(damageContext);
            }

            return true;
        }

        /// <summary>
        /// Heals this Health instance.
        /// </summary>
        /// <param name="amount">The amount to heal.</param>
        
        public void Heal(int amount){
            value += amount;
            if(value >= maxValue){
                RestoredState();
            }
            else{
                AliveState();
                Healed?.Invoke(amount);
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

        ///
        /// Functions.
        /// 


        public void ApplyBleedStacks(int amount)
        {
            bleedStacks += amount; 
            if(bleedTickTimer.State == TimerState.Halted)
            {
                bleedTickTimer.Begin();
            }
        }


        /// 
        /// Linkage.
        /// 


        private void LinkEvents()
        {
            LinkTimerEvents();
        }

        private void UnlinkEvents()
        {
            UnlinkTimerEvents();
        }

        private void LinkTimerEvents()
        {
            bleedTickTimer.Timeout += OnBleedTickTimeout;   
        }

        private void UnlinkTimerEvents()
        {
            bleedTickTimer.Timeout -= OnBleedTickTimeout;               
        }

        private void OnBleedTickTimeout()
        {
            bleedCurrentTick -= 1;

            Damage(
                new DamageContext(
                    transform.position,
                    BleedDamagePerStack * bleedStacks,
                    DamageType.Light
                )
            );

            Debug.Log("bleed");

            if(bleedCurrentTick <= 0)
            {
                bleedCurrentTick = MaxBleedTicksPerInterval;                
                bleedStacks -= 1;
                if(bleedStacks <= 0)
                {
                    bleedTickTimer.Halt();
                }
            }
        }
    }


}
