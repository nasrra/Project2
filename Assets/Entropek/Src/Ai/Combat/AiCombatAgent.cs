using System;
using UnityEngine;

namespace Entropek.Ai.Combat{


    [RequireComponent(typeof(SphereCollider))]
    public class AiCombatAgent : MonoBehaviour{
        

        /// 
        /// Callbacks.
        /// 

        public Action<AiCombatAction> ActionChosen;
        public Action<Transform> EngagedOpponent;


        /// 
        /// Components.
        /// 


        [Header("Components")]
        [SerializeField] private EntityStats.HealthSystem selfHealth;
        [SerializeField] private Time.LoopedTimer damageTakenIntervalTimer;
        [SerializeField] private Time.LoopedTimer evaluationIntervalTimer;
        private Transform opponentTransform;
        private EntityStats.HealthSystem opponentHealth;


        /// 
        /// Data.
        /// 


        [Header("Data")]

        [Tooltip("The probability at which a action is chosen based on the curve.")]
        [SerializeField] private AnimationCurve scoreProbabtilityCurve = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(1f,1f));
        [SerializeField] private AiCombatAction[] aiCombatActions;
        public AiCombatAction[] AiCombatActions => aiCombatActions;
        private (AiCombatAction, float)[] possbileCombatActions;
        private AiCombatAction chosenCombatAction;
        public AiCombatAction ChosenCombatAction => chosenCombatAction;

        [SerializeField] LayerMask opponentLayer;
        [SerializeField] LayerMask obstacleLayer;

        private float damageTakenInCurrentInteval;

        bool isEngaged; // whether or not this agent has been agro'd


        /// 
        /// Base.
        /// 

        void Start(){
            possbileCombatActions = new (AiCombatAction, float)[aiCombatActions.Length];
        }

        private void OnEnable(){
            LinkEvents();
        }

        private void OnDisable(){
            UnlinkEvents();
        }

        private void OnTriggerEnter(Collider other){
            
            // Get the other's bitwise layer mask.
            // Get our bitwise layer mask for opponents. 

            int otherLayerValue = 1 << other.gameObject.layer;
            int opponentLayerValue = opponentLayer.value;
            
            // evaluate if the other is an opponent.

            if((otherLayerValue & opponentLayerValue) != 0){
                
                Transform otherTransform = other.transform;
                EntityStats.HealthSystem otherHealth = other.GetComponent<EntityStats.HealthSystem>();
                
                if(ValidateOpponent(otherTransform, otherHealth) == true){
                    EngageOpponent(otherTransform, otherHealth);
                }
            }
        }


        /// 
        /// Functions.
        /// 


        /// <summary>
        /// Halts the evaluation loop.
        /// </summary>

        public void Halt()
        {
            evaluationIntervalTimer.Halt();
        }

        /// <summary>
        /// Begins the evaluation loop.
        /// </summary>

        public void Begin()
        {
            evaluationIntervalTimer.Begin();
        }

        private float GetDistanceToClosestObstacle()
        {
            // TODO:
            // IMPLEMENT THIS WHEN NEEDED!

            return 0;
        }

        /// <summary>
        /// Used to validate if the engaged opponent hasnt been destroyed in a given frame.
        /// </summary>
        /// <returns>true, if the objct is still allocated in memory.</returns>

        private bool ValidateEngagedOpponent()
        {
            return ValidateOpponent(opponentTransform, opponentHealth);
        }

        /// <summary>
        /// Used to validate if an opponent hasnt been destroyed in the given frame.
        /// </summary>
        /// <param name="opponentTransform"></param>
        /// <param name="opponentHealth"></param>
        /// <returns>true, if they are still allocated in memory.</returns>

        private bool ValidateOpponent(Transform opponentTransform, EntityStats.HealthSystem opponentHealth)
        {
            return opponentTransform != null && opponentHealth != null;
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
        /// Updates the possible actions array stored by this agent to contain a new set of desireable actions that can be chosen; sorted in descending order from most to least desireable.
        /// </summary>

        private void GeneratePossibleActions()
        {

            // get the data required for evaluation.

            Vector3 vectorDistanceToOpponent = transform.position - opponentTransform.position;
            float distanceToOpponent = vectorDistanceToOpponent.magnitude;

            // dot needs to be negated because vector distance is reveresed for correct distance calculations, not dot product similarity.
            // vector distance could be negated instead but doing one negation on a float is faster then 3.

            float dotDirectionToOpponent = -Vector3.Dot(vectorDistanceToOpponent.normalized, transform.forward);

            float normalisedSelfHealthValue = selfHealth.GetNormalisedHealthValue();
            float normalisedOpponentHealthValue = opponentHealth.GetNormalisedHealthValue();
            float distanceToClosestObstacle = GetDistanceToClosestObstacle();

            // clear the previous evaluates options.

            Array.Clear(possbileCombatActions, 0, possbileCombatActions.Length);
            int possbileCombatActionsIndex = -1;

            for (int i = 0; i < aiCombatActions.Length; i++)
            {

                AiCombatAction currentEvaluation = aiCombatActions[i];

                // if the action is not enabled or currently on cooldown.

                if (currentEvaluation.Enabled == false || currentEvaluation.CooldownTimer.HasTimedOut == false)
                {
                    continue;
                }

                // check if the target is currently in view of the action.

                if (dotDirectionToOpponent < currentEvaluation.MinFov
                || dotDirectionToOpponent > currentEvaluation.MaxFov)
                {
                    continue;
                }

                float score = currentEvaluation.Evaluate(
                    damageTakenInCurrentInteval,
                    distanceToOpponent,
                    distanceToClosestObstacle,
                    normalisedOpponentHealthValue,
                    normalisedSelfHealthValue
                );

                possbileCombatActions[++possbileCombatActionsIndex] = (currentEvaluation, score);
            }

            // sort in descenging order.

            Array.Sort(possbileCombatActions, (a, b) => b.Item2.CompareTo(a.Item2));
        }

        /// <summary>
        /// Executes a random possible action, with probability of an action depending on how desirable it is.
        /// (eg. 100% desirability will allways occur, 50% desirability will only occur half the time).
        /// This function may return nothing if no action was chosen or found, link to the ActionChosen event to 
        /// recieve callbacks. 
        /// </summary>

        private void ChoosePossibleAction()
        {

            (AiCombatAction, float) bestAction = possbileCombatActions[0];

            // get the probability value of executing this action based on its score
            // projected onto the probability curve.

            float probability = scoreProbabtilityCurve.Evaluate(bestAction.Item2 / AiCombatAction.MaxWeight);

            if (UnityEngine.Random.Range(0f, 1f) <= probability)
            {

                // cache the chosen action so the cooldown timer can be called
                // by the Ai entity when ready (e.g. after completing an attack animation).

                chosenCombatAction = bestAction.Item1;

                // stop from evaluating any more

                Halt();

                // execute it if its within the probable range.

                ActionChosen?.Invoke(bestAction.Item1);
            }
        }

        /// <summary>
        /// Begins the cooldown timer for the chosen combat action, removing it as a possibility from the
        /// action choice pool until the timer has finished.
        /// </summary>

        public void BeginChosenCombatActionCooldown()
        {
            chosenCombatAction.CooldownTimer.Begin();
        }

        public void EngageOpponent(Transform opponentTransform, EntityStats.HealthSystem opponentHealth)
        {
            isEngaged = true;
            this.opponentTransform = opponentTransform;
            this.opponentHealth = opponentHealth;
            evaluationIntervalTimer.Begin();
            EngagedOpponent?.Invoke(opponentTransform);
        }

        public void DisengageOpponent(){
            isEngaged           = false;
            opponentTransform   = null;
            opponentHealth      = null;
            evaluationIntervalTimer.Halt();
        }

        public (AiCombatAction, float)[] GetPossibleCombatActions()
        {
            return possbileCombatActions;
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < aiCombatActions.Length; i++)
            {
                // call on validate for each action as they are not MonoBehaviour. 
                
                aiCombatActions[i].OnValidate();
            }
        }
#endif


        /// 
        /// Linkage.
        /// 


        private void LinkEvents(){
            LinkTimerEvents();
        }

        private void UnlinkEvents(){
            UnlinkTimerEvents();
        }

        private void LinkTimerEvents(){
            evaluationIntervalTimer.Timeout += OnEvaluationIntervalTimeout;
        }

        private void UnlinkTimerEvents(){
            evaluationIntervalTimer.Timeout -= OnEvaluationIntervalTimeout;
        }

        private void OnEvaluationIntervalTimeout(){
            if(isEngaged==true){
                Evaluate();
            }
        }
    }


}

