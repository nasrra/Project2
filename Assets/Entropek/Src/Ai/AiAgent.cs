using System;
using System.Collections.Generic;
using Entropek.Ai.Contexts;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Ai
{
    public abstract class AiAgent : MonoBehaviour
    {


        /// 
        /// Callbacks.
        /// 


        public  event Action<string> OutcomeChosen;

        [Header("Components")]
        [Tooltip("An AiAgent that halts and begins its evaluation loop when this AiAgent is halted or began.")]
        [SerializeField] private AiAgent aiAgentLinkedToEvaluationLoopToggle;

        [SerializeField] private AiAgentContext aiAgentContext;
        public AiAgentContext AiAgentContext => aiAgentContext;

        [SerializeField] private Time.LoopedTimer evaluationIntervalTimer;
        public Time.LoopedTimer EvaluationIntervalTimer => evaluationIntervalTimer;

        [Header("Data")]
        [Tooltip("The probability at which a action is chosen based on the curve.")]
        [SerializeField] protected AnimationCurve scoreProbabtilityCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        protected List<AiPossibleOutcome> possibleOutcomes = new();
        public List<AiPossibleOutcome> PossibleOutcomes => possibleOutcomes;

                

        /// 
        /// Base.
        /// 


        protected virtual  void Awake()
        {
            LinkEvents();
        }

        protected virtual void OnDestroy()
        {
            UnlinkEvents();
        }
    

        /// <summary>
        /// Evaluates the agent in relation to its opponent and invokes the ActionChosen callback if an desireable action was found; possibly not invoking at all.
        /// </summary>
        /// <returns>true, if an outcome was chosen; otherwise false.</returns>

        public bool Evaluate()
        {
            
            aiAgentContext.Evaluate();

            possibleOutcomes.Clear();

            GeneratePossibleOutcomes();

            // short-circuit if there are no possible outcomes.

            if(possibleOutcomes.Count == 0)
            {
                return false;
            }

            // sort in descending order.

            possibleOutcomes.Sort((a,b) => b.EvaluationScore.CompareTo(a.EvaluationScore));

            return ChoosePossibleOutcome();
        }

        /// <summary>
        /// Updates the possible results array stored by this agent to contain a new set of desireable results that can be chosen; sorted in descending order from most to least desireable.
        /// </summary>
        /// <param name ="outcomeIndex">The index of the outcome in the collection that stores the outcome.
        /// Note:
        ///     Outcomes should always be in a collection where ordering does not change (array or list).
        /// </param>

        protected abstract void GeneratePossibleOutcomes();

        /// <summary>
        /// Executes a random possible result, with probability of an result depending on how desirable it is.
        /// (eg. 100% desirability will allways occur, 50% desirability will only occur half the time).
        /// This function may return nothing if no result was chosen or found.
        /// </summary>
        /// <returns>true, if an outcome was chosen; otherwise false.</returns>
        
        private bool ChoosePossibleOutcome()
        {
            AiPossibleOutcome mostDesireable = possibleOutcomes[0];

            // get the probability value of executing this action based on its score
            // projected onto the probability curve.

            float probability = scoreProbabtilityCurve.Evaluate(mostDesireable.EvaluationScore / mostDesireable.OutcomeMaxScore);

            if (UnityEngine.Random.Range(0f, 1f) <= probability)
            {
                // callback to subclasses for any extra operations.

                OnPossibleOutcomeChosen(mostDesireable);

                // execute it if its within the probable range.

                OutcomeChosen?.Invoke(mostDesireable.Name);
                return true;
            }
            return false;
        }    

        /// <summary>
        /// A callback function to subclesses for any operations that need to be performed when choosing a possible outcome.
        /// </summary>
        /// <param name="chosenOutcome">The AiPossibleOutcome that was chosen in the evaluation process.</param>

        protected abstract void OnPossibleOutcomeChosen(in AiPossibleOutcome chosenOutcome);

        /// <summary>
        /// Halts the evaluation loop.
        /// </summary>

        public virtual void HaltEvaluationLoop()
        {
            EvaluationIntervalTimer.Halt();

            // halt a linked AiAgent if there is one.

            if(aiAgentLinkedToEvaluationLoopToggle != null)
            {
                aiAgentLinkedToEvaluationLoopToggle.HaltEvaluationLoop();
            }
        }        
        
        /// <summary>
        /// Begins the evaluation loop for choosing an desired action to take; per evaluation tick.
        /// </summary>

        public virtual void BeginEvaluationLoop()
        {
            EvaluationIntervalTimer.Begin();

            // begin a linked AiAgent if there is one.

            if(aiAgentLinkedToEvaluationLoopToggle != null)
            {
                aiAgentLinkedToEvaluationLoopToggle.BeginEvaluationLoop();                
            }
        }
        

        /// <summary>
        /// Linkage.
        /// </summary>


        protected virtual void LinkEvents()
        {
            LinkTimerEvents();
        }

        protected virtual void UnlinkEvents()
        {
            UnlinkTimerEvents();
        }

        protected virtual void LinkTimerEvents()
        {
            evaluationIntervalTimer.Timeout += OnEvaluationIntervalTimeout;
        }

        protected virtual void UnlinkTimerEvents()
        {
            evaluationIntervalTimer.Timeout -= OnEvaluationIntervalTimeout;
        }

        private void OnEvaluationIntervalTimeout()
        {
            Evaluate();
        }
    }    
}

