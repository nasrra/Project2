using System;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Ai.Combat{


    [RequireComponent(typeof(SphereCollider))]
    public class ComplexAiCombatAgent : AiCombatAgent<ComplexAiCombatAction>
    {

        
        /// 
        /// Components.
        /// 


        [Header("Components")]
        [SerializeField] private EntityStats.HealthSystem selfHealth;
        [SerializeField] private Time.LoopedTimer damageTakenIntervalTimer;
        private EntityStats.HealthSystem opponentHealth;


        /// 
        /// Data.
        /// 


        [Header("Data")]
        [SerializeField] LayerMask obstacleLayer;
        [RuntimeField] private float damageTakenInCurrentInterval;
        public float DamageTakenInCurrentInterval => damageTakenInCurrentInterval; 


        /// 
        /// Base.
        /// 
        
        private void OnEnable()
        {
            LinkEvents();
        }

        private void OnDisable()
        {
            UnlinkEvents();
        }


        /// 
        /// Functions.
        /// 


        public override void HaltEvaluationLoop()
        {
            base.HaltEvaluationLoop();
            damageTakenIntervalTimer.Halt();
        }

        public override void BeginEvaluationLoop()
        {
            base.BeginEvaluationLoop();
            damageTakenIntervalTimer.Begin();

            // reset to ensure the value does not pass between intervals;
            // when Evaluate () successfully chooses an action.

            damageTakenInCurrentInterval = 0;
        }

        private float GetDistanceToClosestObstacle()
        {
            // TODO:
            // IMPLEMENT THIS WHEN NEEDED!

            return 0;
        }

        protected override void GeneratePossibleActions()
        {

            CalculateRelationToEngagedOpponent(out Vector3 vectorDistanceToOpponent, out float distanceToOpponent, out float dotDirectionToOpponent);

            float normalisedSelfHealthValue = selfHealth.GetNormalisedHealthValue();
            float normalisedOpponentHealthValue = opponentHealth.GetNormalisedHealthValue();
            float distanceToClosestObstacle = GetDistanceToClosestObstacle();

            // clear the previous evaluates options.
            
            possibleCombatActions.Clear();

            for (int i = 0; i < aiCombatActions.Length; i++)
            {

                ComplexAiCombatAction currentEvaluation = aiCombatActions[i];

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
                    damageTakenInCurrentInterval,
                    distanceToOpponent,
                    distanceToClosestObstacle,
                    normalisedOpponentHealthValue,
                    normalisedSelfHealthValue
                );

                possibleCombatActions.Add((currentEvaluation, score));
            }

            // sort in descenging order.

            possibleCombatActions.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        }

        public override void EngageOpponent(Transform opponentTransform)
        {
            base.EngageOpponent(opponentTransform);
            opponentHealth = opponentTransform.GetComponent<EntityStats.Health>();
        }

        public override void DisengageOpponent()
        {
            base.DisengageOpponent();
            opponentHealth = null;
        }


        ///
        /// Linkage Overrides.
        /// 


        protected override void LinkEvents()
        {
            base.LinkEvents();
            LinkSelfHealthEvents();
        }

        protected override void UnlinkEvents()
        {
            base.UnlinkEvents();
            UnlinkSelfHealthEvents();
        }


        ///
        /// Timer Linkage.
        ///


        protected override void LinkTimerEvents()
        {
            base.LinkTimerEvents();
            damageTakenIntervalTimer.Timeout += OnDamgeTakenIntevalTimeout;
        }

        protected override void UnlinkTimerEvents()
        {
            base.UnlinkTimerEvents();
            damageTakenIntervalTimer.Timeout -= OnDamgeTakenIntevalTimeout;
        }

        /// <summary>
        /// Resets the damage counter for the next interval.
        /// </summary>

        private void OnDamgeTakenIntevalTimeout()
        {

            damageTakenInCurrentInterval = 0;
        }


        ///
        /// Self health linkage.
        /// 


        private void LinkSelfHealthEvents()
        {
            selfHealth.HealthDamaged += OnHealthDamaged; 
        }

        private void UnlinkSelfHealthEvents()
        {
            selfHealth.HealthDamaged -= OnHealthDamaged; 
        }

        /// <summary>
        /// Increments the counter for damage taken this interval by a specified amount.
        /// </summary>
        /// <param name="amount">The amount to increment by.</param>

        private void OnHealthDamaged(float amount)
        {
            damageTakenInCurrentInterval += amount;
        }

    }


}

