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

        [SerializeField] private AnimationCurve scoreProbabtilityCurve = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(1f,1f)); // the probability at which a action is chosen based on the curve. 
        [SerializeField] private AiCombatAction[] aiCombatActions;
        private (AiCombatAction, float)[] possbileCombatActions;
        private AiCombatAction chosenCombatAction;

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

        public void Halt()
        {
            evaluationIntervalTimer.Halt();
        }

        public void Begin()
        {
            evaluationIntervalTimer.Begin();
        }


        private float GetDistanceToClosestObstacle()
        {
            return 0;
        }

        private bool ValidateEngagedOpponent(){
            return ValidateOpponent(opponentTransform, opponentHealth);
        }

        private bool ValidateOpponent(Transform opponentTransform, EntityStats.HealthSystem opponentHealth){
            return opponentTransform != null && opponentHealth != null;
        }

        public void Evaluate(){

            // validate that out opponent is still active and not destroyed.

            if(ValidateEngagedOpponent()==false){
                DisengageOpponent();
            }

            GetPossibleActions();        
            ChoosePossibleAction();
        }

        private void GetPossibleActions()
        {

            // get the data required for evaluation.

            float normalisedSelfHealthValue = selfHealth.GetNormalisedHealthValue();
            float normalisedOpponentHealthValue = opponentHealth.GetNormalisedHealthValue();
            float distanceToOpponent = (transform.position - opponentTransform.position).magnitude;
            float distanceToClosestObstacle = GetDistanceToClosestObstacle();

            // clear the previous evaluates options.

            Array.Clear(possbileCombatActions, 0, possbileCombatActions.Length);
            int possbileCombatActionsIndex = -1;

            for (int i = 0; i < aiCombatActions.Length; i++)
            {

                AiCombatAction currentEvaluation = aiCombatActions[i];

                // if the action is currently on cooldown.

                if (currentEvaluation.CooldownTimer.HasTimedOut == false)
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

        public (AiCombatAction, float)[] GetPossibleCombatActions(){
            return possbileCombatActions;
        }


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

