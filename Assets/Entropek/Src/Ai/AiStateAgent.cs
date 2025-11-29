using System;
using System.Collections.Generic;
using Entropek.Time;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Ai
{
    public class AiStateAgent : AiAgent
    {        
        [Header("Components")]
        public AiState ChosenState{get; protected set;}

        [Header("Data")]


        [SerializeField] private int InitialStateIndex = 0;
        [SerializeReference] protected AiState[] aiStates;
        public AiState[] AiStates => aiStates;
        
        [RuntimeField] private float chosenStateCurrentLifetime;
        
        public float ChosenStateCurrentLifetime => chosenStateCurrentLifetime;

        void Start()
        {
            
            // set the agent state to the inital state set in the inspector.
            
            SetChosenState(InitialStateIndex);
            InvokeOutcomeChosen(ChosenState.Name);
        }

        void FixedUpdate()
        {
            if (halted == false)
            {
                chosenStateCurrentLifetime += UnityEngine.Time.deltaTime;
            }
        }

        protected override void GeneratePossibleOutcomes()
        {

            // get the desirability to swap states based on the current state's lifetime modifier.

            float chosenStateLifetimeWeight = ChosenState.StateSwitchChanceOverLifetime.Evaluate(chosenStateCurrentLifetime);

            for(int i = 0; i < aiStates.Length; i++)
            {
                AiState evaluation = aiStates[i];

                if(evaluation.Enabled == true && evaluation.IsPossible(AiAgentContext) == true)
                {
                    
                    // apply the lifetime modifier to the score, affecting the probality of choosing the state.

                    float score = evaluation.Evaluate(AiAgentContext) * chosenStateLifetimeWeight; 

                    possibleOutcomes.Add(
                        new AiPossibleOutcome(
                            evaluation.Name,
                            score,
                            evaluation.MaxScore,
                            i
                        )
                    );
                }
            }
        }

        protected override void OnPossibleOutcomeChosen(in AiPossibleOutcome chosenOutcome)
        {
            SetChosenState(chosenOutcome.OutcomeIndex);
        }

        /// <summary>
        /// Sets the ChosenState of this AiStateAgent to an entry in the internal AiState collection.
        /// </summary>
        /// <param name="index">The index of the state to choose in the collection.</param>

        protected void SetChosenState(int index)
        {            
            // re-enable the state this agent is leaving.

            if(ChosenState != null)
            {
                ChosenState.Enabled = true;
            }

            ChosenState = aiStates[index];
            
            // disable the chosen state to stop it from being considered in the next evaluations.

            ChosenState.Enabled = false;
            
            chosenStateCurrentLifetime = 0;
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

