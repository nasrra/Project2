using System;
using System.Collections.Generic;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Combat{
    
    [RequireComponent(typeof(Time.OneShotTimer))]
    public class TimedSingleHitbox : SingleHitbox{
        
        [Header("Components")]
        [SerializeField] private Time.OneShotTimer timer;
        public Time.OneShotTimer Timer => timer;


        /// 
        /// Base.
        /// 


        void OnEnable(){
            Link();
        }

        void OnDisable(){
            Unlink();
        }



        /// <summary>
        /// Activates the trigger collider (for the amount of time specified as the intial time in the timer) for the hitbox, enabling detection of incoming hurtboxes.
        /// </summary>
        
        public override void Enable()
        {
            base.Enable();
            timer.Begin();
        }        


        /// 
        /// Linkage.
        /// 


        private void Link(){
            timer.Timeout += OnTimerTimeout;
        }

        private void Unlink(){
            timer.Timeout -= OnTimerTimeout;
        }

        private void OnTimerTimeout()
        {
            Disable();
        }
    }

}
