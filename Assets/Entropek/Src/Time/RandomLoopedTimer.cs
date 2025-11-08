using System;
using Entropek.Time;
using UnityEngine;
using UnityEngine.Rendering;

namespace Entropek.Time
{   
    public class RandomLoopedTimer : LoopedTimer
    {      
        [Header(nameof(RandomLoopedTimer))]
        [SerializeField] private float initialTimeMin = 0;
        [SerializeField] private float initialTimeMax = 0;

        public void SetInitialTimeRange(float min, float max)
        {

#if UNITY_EDITOR

            if(min > max)
            {
                throw new InvalidOperationException($"{gameObject.name} Random Looped Timer InitialTimeMin ({initialTimeMin}) is greater than InitialTimeMax ({initialTimeMax}).");
            }

#endif

            initialTimeMin = min;
            initialTimeMax = max;
        }

        ///
        /// Overrides.
        /// 

        public override void Reset()
        {
            // set the initial time to be a random number within the set range.

            initialTime = UnityEngine.Random.Range(initialTimeMin, initialTimeMax);
            base.Reset();
        }

#if  UNITY_EDITOR

        private float editorLastValidateMinRange;
        private float editorLastValidateMaxRange;

        /// <summary>
        /// Regulate initial time range bounds.
        /// </summary>

        void OnValidate()
        {
            
            if(initialTimeMin != editorLastValidateMinRange)
            {
                if(initialTimeMin > initialTimeMax)
                {
                    // add a 0.001f for margin of error
                    // and ensure the value is never greater than float max value or less than 0.

                    initialTimeMin = Mathf.Clamp(initialTimeMax-0.001f,0,float.MaxValue);
                }
            }

            if(initialTimeMax != editorLastValidateMaxRange)
            {
                if(initialTimeMax < initialTimeMin)
                {
                    // add a 0.001f margin of error.
                    // ensure the value is never greater than float max value or below zero.

                    initialTimeMax = Mathf.Clamp(initialTimeMin+0.001f,0,float.MaxValue);
                }
            }

            editorLastValidateMinRange = initialTimeMin;
            editorLastValidateMaxRange = initialTimeMax;
        }

#endif

    }
}

