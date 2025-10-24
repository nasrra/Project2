using UnityEngine;

namespace Entropek.Time
{
    public class OneShotTimer : Timer
    {

        ///
        /// Base.
        /// 


        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }


        ///
        /// Callbacks.
        /// 


        protected override void OnAfterTimeout()
        {
            // always halt a one shot timer when it times out.

            Halt();
        }

    }    
}

