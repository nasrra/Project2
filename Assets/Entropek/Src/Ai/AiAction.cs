using System;
using Entropek.UnityUtils.Attributes;
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
        [DotProductRangeVisualise, SerializeField] protected DotProductRange fov;
        public DotProductRange Fov => fov;

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
            return dotProduct >= fov.Min && dotProduct <= fov.Max; 
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
            fov = fov.RegulateValues();
        }
#endif

    }    
}

