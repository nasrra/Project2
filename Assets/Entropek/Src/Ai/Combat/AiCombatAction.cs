using UnityEngine;

namespace Entropek.Ai.Combat
{
    public abstract class AiCombatAction
    {
        [Header("Action Name")]
        [SerializeField] protected string name;
        public string Name => name; 

        // the cooldown of the action.

        [Header("Cooldown")]
        [SerializeField] protected Time.Timer cooldownTimer;
        public Time.Timer CooldownTimer => cooldownTimer;

        [Header("Parameters")]

        [Tooltip("Fov is measured by dot product value (-1 - 1), not by angle")]
        [SerializeField][Range(-1,1)] protected float maxFov;
        public float MaxFov => maxFov;

        [Tooltip("Fov is measured by dot product value (-1 - 1), not by angle")]
        [SerializeField][Range(-1, 1)] protected float minFov;
        public float MinFov => minFov;

        [Tooltip("The amount of time spent in the idle state after this action has completed.")]
        [SerializeField][Range(0, 10)] protected float idleTime;
        public float IdleTime => idleTime;

        [Tooltip("Whether or not the agent should evaluate for a new action to perform when the idle state has timed out.")]
        [SerializeField] protected bool evaluateOnIdleTimeout;
        public bool EvaluateOnIdleTimeout => evaluateOnIdleTimeout;

        // whether or not to turn towards the target before commiting to this action.
        [SerializeField] protected bool turnToTarget;
        public bool TurnToTarget => turnToTarget;

        // whether or not this action is considered a plausible action to be chosen by this combat agent.
        [SerializeField] protected bool enabled = true;
        public bool Enabled => enabled;
    
        public abstract float GetMaxWeight();

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

                    minFov = Mathf.Clamp((maxFov + 0.01f), -1, 1);
                }
            }

            // ensure that max fov is never less than min fov.

            if (editorLastValidateMaxFov != maxFov)
            {
                if (maxFov < minFov)
                {

                    // subtract 0.1f for margin of error, and clamp value between -1 and 1 (min and max of dot product values).

                    maxFov = Mathf.Clamp((minFov - 0.01f), -1, 1);
                }
            }

            editorLastValidateMaxFov = maxFov;
            editorLastValidateMinFov = minFov;
        }

#endif

    }    
}

