using UnityEngine;

namespace Entropek.Time
{

    public class LoopedTimer : Timer
    {
        [Header(nameof(LoopedTimer))]
        [SerializeField] public bool Loop = true;


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
        /// Callback Overrides.
        /// 

        protected override void OnBeforeTimeout()
        {
            // do nothing.
            return;
        }

        protected override void OnAfterTimeout()
        {
            if (state == TimerState.Halted)
            {
                return; // do nothing if the timeout callback resulted in this timer being halted.
            }
            else if (Loop == false)
            {
                Halt(); // do not loop any more if loop is set to false.
            }
            else
            {
                Begin(); // continue looping ig nothing has changed.
            }

        }
    }
    

}

