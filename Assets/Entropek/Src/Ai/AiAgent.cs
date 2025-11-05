using System;
using System.Collections.Generic;
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
        public event Action<Transform> EngagedOpponent;

        [Header("Components")]
        [Tooltip("An AiAgent that halts and begins its evaluation loop when this AiAgent is halted or began.")]
        [SerializeField] private AiAgent aiAgentLinkedToEvaluationLoopToggle;
        public AiAgent AiAgentLinkedToEvaluationLoopToggle;

        [Header("Data")]

        [Tooltip("The probability at which a action is chosen based on the curve.")]
        [SerializeField] protected AnimationCurve scoreProbabtilityCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        protected List<AiPossibleOutcome> possibleOutcomes = new();
        public List<AiPossibleOutcome> PossibleOutcomes => possibleOutcomes;

        [SerializeField] private Transform opponentTransform;
        public Transform OpponentTransform => opponentTransform;

        [SerializeField] private Time.LoopedTimer evaluationIntervalTimer;
        public Time.LoopedTimer EvaluationIntervalTimer => evaluationIntervalTimer;
        
        [SerializeField] protected LayerMask opponentLayer;
        
        [RuntimeField] protected bool isEngaged = false;


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

        public virtual void Evaluate()
        {

            // validate that out opponent is still active and not destroyed.

            if (ValidateEngagedOpponent() == false)
            {
                DisengageOpponent();
            }

            CalculateRelationToEngagedOpponent(out AiActionAgentRelationToOpponentContext context);
                        
            possibleOutcomes.Clear();
            GeneratePossibleOutcomes(in context);

            // short-circuit if there are no possible outcomes.

            if(possibleOutcomes.Count == 0)
            {
                return;
            }

            // sort in descending order.

            possibleOutcomes.Sort((a,b) => b.EvaluationScore.CompareTo(a.EvaluationScore));

            ChoosePossibleOutcome(in context);
        }

        /// <summary>
        /// Updates the possible results array stored by this agent to contain a new set of desireable results that can be chosen; sorted in descending order from most to least desireable.
        /// </summary>
        /// <param name="context">The relational information between this agent and its engaged opponent.</param>
        /// <param name ="outcomeIndex">The index of the outcome in the collection that stores the outcome.
        /// Note:
        ///     Outcomes should always be in a collection where ordering does not change (array or list).
        /// </param>

        protected abstract void GeneratePossibleOutcomes(in AiActionAgentRelationToOpponentContext context);

        /// <summary>
        /// Executes a random possible result, with probability of an result depending on how desirable it is.
        /// (eg. 100% desirability will allways occur, 50% desirability will only occur half the time).
        /// This function may return nothing if no result was chosen or found.
        /// </summary>
        /// <param name="context">The relational information between this agent and its engaged opponent.</param>

        private void ChoosePossibleOutcome(in AiActionAgentRelationToOpponentContext context)
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
            }
        }    

        /// <summary>
        /// A callback function to subclesses for any operations that need to be performed when choosing a possible outcome.
        /// </summary>
        /// <param name="chosenOutcome">The AiPossibleOutcome that was chosen in the evaluation process.</param>

        protected abstract void OnPossibleOutcomeChosen(in AiPossibleOutcome chosenOutcome);

        /// <summary>
        /// Used to validate if the engaged opponent hasnt been destroyed in a given frame.
        /// </summary>
        /// <returns>true, if the objct is still allocated in memory.</returns>

        protected bool ValidateEngagedOpponent()
        {
            return opponentTransform != null;
        }


        /// <summary>
        /// Gets the relative data for this frame between this agent and the engaged opponent.
        /// </summary>
        /// <param name="vectorDistanceToOpponent">The vector distance from this agent to the engaged opponent.</param>
        /// <param name="distanceToOpponent">The float distance from this agent to the engaged opponent.</param>
        /// <param name="dotDirectionToOpponent">The dot product that represents if the agent is facing (1) or looking away (-1) from the engaged opponent.</param>

        protected void CalculateRelationToEngagedOpponent(out AiActionAgentRelationToOpponentContext relationToOpponentContext)
        {
            // Notes:
            //  dotDirectionToOpponent needs to be negated because vector distance is reveresed for correct distance calculations, not dot product similarity.
            //  vector distance could be negated instead but doing one negation on a float is faster then 3.

            Vector3 vectorDistanceToOpponent = opponentTransform.position - transform.position;
            float distanceToOpponent = vectorDistanceToOpponent.magnitude;
            float dotDirectionToOpponent = Vector3.Dot(vectorDistanceToOpponent.normalized, transform.forward);

            relationToOpponentContext = new(
                vectorDistanceToOpponent,
                distanceToOpponent,
                dotDirectionToOpponent 
            );
        }

        /// <summary>
        /// Clears the currently engaged opponent and halts the evaluation loop.
        /// </summary>

        public virtual void DisengageOpponent()
        {
            isEngaged = false;
            opponentTransform = null;
            HaltEvaluationLoop();
        }

        /// <summary>
        /// Sets the target of this agent to the transform of an opponent and starts the evaluation loop.
        /// </summary>
        /// <param name="opponentTransform">The specified transform.</param>

        public virtual void EngageOpponent(Transform opponentTransform)
        {
            isEngaged = true;
            this.opponentTransform = opponentTransform;
            BeginEvaluationLoop();
            EngagedOpponent?.Invoke(opponentTransform);
        }        
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

        private void OnTriggerEnter(Collider other)
        {

            // Get the other's bitwise layer mask.
            // Get our bitwise layer mask for opponents. 

            int otherLayerValue = 1 << other.gameObject.layer;
            int opponentLayerValue = opponentLayer.value;

            // evaluate if the other is an opponent.

            if ((otherLayerValue & opponentLayerValue) != 0)
            {

                Transform otherTransform = other.transform;

                // if (ValidateEngagedOpponent() == true)
                // {
                EngageOpponent(otherTransform);
                // }
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
            if (isEngaged == true)
            {
                Evaluate();
            }
        }
    }    
}

