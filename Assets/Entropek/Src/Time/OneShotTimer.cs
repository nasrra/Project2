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

        protected override void OnBeforeTimeout()
        {
            // always halt a one shot timer before it times out.
            // allowing begin() to be called again once timed out is invoked.

            Halt();
        }

        protected override void OnAfterTimeout()
        {
            // do nothing...
            return;
        }

    }    
}

