using System;
using System.Collections.Generic;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Ai.Combat
{    
    public abstract class AiCombatAgent<T> : MonoBehaviour where T : AiCombatAction
    {
        public event Action<AiCombatAction> ActionChosen;
        public Action<Transform> EngagedOpponent;
        
        [Header("Components")]
        [SerializeField] protected Time.LoopedTimer evaluationIntervalTimer;
        private AiCombatAction chosenCombatAction;
        public AiCombatAction ChosenCombatAction => chosenCombatAction;
        protected Transform opponentTransform;


        [Header("Data")]
        [Tooltip("The probability at which a action is chosen based on the curve.")]
        [SerializeField] protected AnimationCurve scoreProbabtilityCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        [SerializeField] protected T[] aiCombatActions;
        public T[] AiCombatActions => aiCombatActions;
        protected List<(T, float)> possibleCombatActions = new();
        public List<(T, float)> PossibleCombatActions => possibleCombatActions;
        [SerializeField] protected LayerMask opponentLayer;
        [RuntimeField] protected bool isEngaged; // whether or not this agent has been agro'd


        /// 
        /// Base.
        /// 


        private void Awake()
        {
            LinkEvents();
        }

        private void OnDestroy()
        {
            UnlinkEvents();
        }


        /// <summary>
        /// Halts the evaluation loop.
        /// </summary>

        public virtual void HaltEvaluationLoop()
        {
            evaluationIntervalTimer.Halt();
        }

        /// <summary>
        /// Begins the evaluation loop for choosing an desired action to take; per evaluation tick.
        /// </summary>

        public virtual void BeginEvaluationLoop()
        {
            evaluationIntervalTimer.Begin();
        }

        /// <summary>
        /// Evaluates the agent in relation to its opponent and invokes the ActionChosen callback if an desireable action was found; possibly not invoking at all.
        /// </summary>

        public void Evaluate()
        {

            // validate that out opponent is still active and not destroyed.

            if (ValidateEngagedOpponent() == false)
            {
                DisengageOpponent();
            }

            GeneratePossibleActions();
            ChoosePossibleAction();
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
        /// Used to validate if the engaged opponent hasnt been destroyed in a given frame.
        /// </summary>
        /// <returns>true, if the objct is still allocated in memory.</returns>

        protected bool ValidateEngagedOpponent()
        {
            return opponentTransform != null;
        }

        /// <summary>
        /// Updates the possible actions array stored by this agent to contain a new set of desireable actions that can be chosen; sorted in descending order from most to least desireable.
        /// </summary>

        protected abstract void GeneratePossibleActions();
                    
        /// <summary>
        /// Begins the cooldown timer for the chosen combat action, removing it as a possibility from the
        /// action choice pool until the timer has finished.
        /// </summary>

        public void BeginChosenCombatActionCooldown()
        {
            chosenCombatAction.CooldownTimer.Begin();
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
        /// Executes a random possible action, with probability of an action depending on how desirable it is.
        /// (eg. 100% desirability will allways occur, 50% desirability will only occur half the time).
        /// This function may return nothing if no action was chosen or found, link to the ActionChosen event to 
        /// recieve callbacks. 
        /// </summary>

        private void ChoosePossibleAction()
        {

            // short-circuit if there are currently no possible actions.

            if(possibleCombatActions.Count == 0)
            {
                return;
            }

            (T, float) bestAction = possibleCombatActions[0];

            // get the probability value of executing this action based on its score
            // projected onto the probability curve.

            float probability = scoreProbabtilityCurve.Evaluate(bestAction.Item2 / bestAction.Item1.GetMaxWeight());

            if (UnityEngine.Random.Range(0f, 1f) <= probability)
            {

                // cache the chosen action so the cooldown timer can be called
                // by the Ai entity when ready (e.g. after completing an attack animation).

                chosenCombatAction = bestAction.Item1;

                // stop from evaluating any more

                HaltEvaluationLoop();

                // execute it if its within the probable range.

                ActionChosen?.Invoke(bestAction.Item1);
            }
        }    

        /// <summary>
        /// Gets the relative data for this frame between this agent and the engaged opponent.
        /// </summary>
        /// <param name="vectorDistanceToOpponent">The vector distance between this agent and the engaged opponent.</param>
        /// <param name="distanceToOpponent">The float distance between this agent and the engaged opponent.</param>
        /// <param name="dotDirectionToOpponent">The dot product representing if the agent is facing (1) or looking away (-1) from the engaged opponent.</param>

        protected void CalculateRelationToEngagedOpponent(out Vector3 vectorDistanceToOpponent, out float distanceToOpponent, out float dotDirectionToOpponent)
        {
            // get the data required for evaluation.

            vectorDistanceToOpponent = transform.position - opponentTransform.position;
            distanceToOpponent = vectorDistanceToOpponent.magnitude;

            // dot needs to be negated because vector distance is reveresed for correct distance calculations, not dot product similarity.
            // vector distance could be negated instead but doing one negation on a float is faster then 3.

            dotDirectionToOpponent = -Vector3.Dot(vectorDistanceToOpponent.normalized, transform.forward);
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

#if UNITY_EDITOR
        /// <summary>
        /// Calls the OnValidate inspector function for AiCombatActions in the unity editor.
        /// Note:
        ///     This should only be called withn an '#if UNITY_EDITOR' macro.
        /// </summary>
        protected virtual void OnValidate()
        {
            for (int i = 0; i < aiCombatActions.Length; i++)
            {
                // call on validate for each action as they are not MonoBehaviour. 

                aiCombatActions[i].OnValidate();
            }

        }
#endif
        
    }
}
