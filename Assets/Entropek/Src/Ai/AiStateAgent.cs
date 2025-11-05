using System;
using System.Collections.Generic;
using Entropek.Time;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Ai
{
    public abstract class AiStateAgent : AiAgent
    {
        [Header("Components")]
        public AiState ChosenState{get; protected set;}

        public float ChosenStateCurrentLifetime {get;protected set;} = 0;

        void FixedUpdate()
        {
            ChosenStateCurrentLifetime += UnityEngine.Time.deltaTime;
        }
    }

    public abstract class AiStateAgent<T> : AiStateAgent where T : AiState
    {
        [SerializeField] protected T[] aiStates;
        public T[] AiStates => aiStates;

        protected override void OnPossibleOutcomeChosen(in AiPossibleOutcome chosenOutcome)
        {
            // re-enable the state this agent is leaving.

            if(ChosenState != null)
            {
                ChosenState.Enabled = true;
            }

            ChosenState = aiStates[chosenOutcome.OutcomeIndex];
            
            // disable the chosen state to stop it from being considered in the next evaluations.

            ChosenState.Enabled = false;
            
            ChosenStateCurrentLifetime = 0;
        }


#if UNITY_EDITOR
        /// <summary>
        /// Calls the OnValidate inspector function for AiCombatActions in the unity editor.
        /// Note:
        ///     This should only be called withn an '#if UNITY_EDITOR' macro.
        /// </summary>
        protected virtual void OnValidate()
        {
            for (int i = 0; i < aiStates.Length; i++)
            {
                // call on validate for each action as they are not MonoBehaviour. 

                aiStates[i].OnValidate();
            }

        }
#endif
    }    
}

