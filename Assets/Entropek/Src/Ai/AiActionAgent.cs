using System;
using System.Collections.Generic;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Ai
{    
    public abstract class AiActionAgent : AiAgent
    {        
        [Header("Components")]
        public AiAction ChosenAction {get; protected set;}
                    
        /// <summary>
        /// Begins the cooldown timer for the chosen combat action, removing it as a possibility from the
        /// action choice pool until the timer has finished.
        /// </summary>

        public void BeginChosenActionCooldown()
        {
            ChosenAction.CooldownTimer.Begin();
        }
    }

    public abstract class AiActionAgent<T> : AiActionAgent where T : AiAction
    {        
        [Header("Data")]
        [SerializeField] protected T[] aiActions;
        public T[] AiCombatActions => aiActions;

        protected override void OnPossibleOutcomeChosen(in AiPossibleOutcome chosenOutcome)
        {
            // cache the chosen action so the cooldown timer can be called
            // by the Ai entity when ready (e.g. after completing an attack animation).

            ChosenAction = aiActions[chosenOutcome.OutcomeIndex];

            // stop from evaluating any more.

            HaltEvaluationLoop();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Calls the OnValidate inspector function for AiCombatActions in the unity editor.
        /// Note:
        ///     This should only be called withn an '#if UNITY_EDITOR' macro.
        /// </summary>
        protected virtual void OnValidate()
        {
            for (int i = 0; i < aiActions.Length; i++)
            {
                // call on validate for each action as they are not MonoBehaviour. 

                aiActions[i].OnValidate();
            }

        }
#endif

    }
}
