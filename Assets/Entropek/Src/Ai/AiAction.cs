using System;
using UnityEngine;

namespace Entropek.Ai
{
    [Serializable]
    public abstract class AiAction : AiOutcome
    {
        // the cooldown of the action.

        [Header("Cooldown")]
        [SerializeField] protected Time.Timer cooldownTimer;
        public Time.Timer CooldownTimer => cooldownTimer;

        [Header("Parameters")]
        [Tooltip("Fov is measured by dot product value (-1 - 1), not by angle")]
        [SerializeField][Range(-1,1)] private float maxFov;
        public float MaxFov => maxFov;

        [Tooltip("Fov is measured by dot product value (-1 - 1), not by angle")]
        [SerializeField][Range(-1, 1)] private float minFov;
        public float MinFov => minFov;

        [Header("Parameters")]

        [Tooltip("The amount of time spent in the idle state after this action has completed.")]
        [SerializeField][Range(0, 10)] protected float idleTime;
        public float IdleTime => idleTime;

        [Tooltip("Whether or not the agent should evaluate for a new action to perform when the idle state has timed out.")]
        [SerializeField] protected bool evaluateOnIdleTimeout;
        public bool EvaluateOnIdleTimeout => evaluateOnIdleTimeout;


        /// <summary>
        /// Checks whether or not a dot product value is within this actions fov radius. 
        /// </summary>
        /// <param name="dotProduct">The dot product to check against (-1 to 1).</param>
        /// <returns>true, if the specified value is within the fov; otherwise false.</returns>

        public bool WithinFov(float dotProduct)
        {
            return dotProduct >= MinFov && dotProduct <= MaxFov; 
        }

        /// <summary>
        /// Checks whether or not this action is currently on cooldown.
        /// </summary>
        /// <returns>true, if on cooldown; otherwise false.</returns>

        public bool IsOnCooldown()
        {
            return cooldownTimer.CurrentTime > 0;
        }

#if UNITY_EDITOR


        /// <summary>
        /// Calls the OnValidate inspector function for the unity editor.
        /// Note:
        ///     This should only be called withn an '#if UNITY_EDITOR' macro. 
        /// </summary>

        public virtual void OnValidate()
        {
            EditorRegulateFovValues();
        }

        /// <summary>
        /// Ensures that min fov is never greater than max fov and max fov is never less than min fov.
        /// </summary>

        private float editorLastValidateMinFov;
        private float editorLastValidateMaxFov;
        
        private void EditorRegulateFovValues()
        {

            /// Ensures that min fov is never greater than max fov.

            if (editorLastValidateMinFov != minFov)
            {                
                if (minFov > maxFov)
                {
                    // add 0.1f for margin of error, and clamp value between -1 and 1 (min and max of dot product values).

                    minFov = Mathf.Clamp(maxFov + 0.01f, -1, 1);
                }
            }

            // ensure that max fov is never less than min fov.

            if (editorLastValidateMaxFov != maxFov)
            {
                if (maxFov < minFov)
                {

                    // subtract 0.1f for margin of error, and clamp value between -1 and 1 (min and max of dot product values).

                    maxFov = Mathf.Clamp(minFov - 0.01f, -1, 1);
                }
            }

            editorLastValidateMaxFov = maxFov;
            editorLastValidateMinFov = minFov;
        }

#endif

    }    
}

