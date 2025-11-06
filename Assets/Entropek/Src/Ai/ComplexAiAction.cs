using System;
using Entropek.Ai.Contexts;
using Entropek.Combat;
using Entropek.EntityStats;
using Entropek.Time;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Ai{


    [Serializable]
    public class ComplexCombatAiAction : AiAction{


        /// 
        /// Curves.
        /// 


        [Header("Curves")]

        // The amount of health lost in a given time interval.

        [SerializeField] private AnimationCurve damageTakenIntervalCurve;
        public AnimationCurve DamageTakenIntervalCurve => damageTakenIntervalCurve;

        // The Distance to the closest significant obstacle [walls, etc.]

        [SerializeField] private AnimationCurve distanceToObstacleCurve;
        public AnimationCurve DistanceToObstacleCurve => distanceToObstacleCurve;

        // The distance from the Ai to the opponent.

        [SerializeField] private AnimationCurve distanceToOpponentCurve;
        public AnimationCurve DistanceToOpponentCurve => distanceToOpponentCurve;

        // The amount of health of the opponent.

        [SerializeField] private AnimationCurve normalisedOpponentHealthCurve;
        public AnimationCurve NormalisedOpponentHealthCurve => normalisedOpponentHealthCurve;

        // The amount of health this Ai currently has.

        [SerializeField] private AnimationCurve normalisedSelfHealthCurve;
        public AnimationCurve NormalisedSelfHealthCurve => normalisedSelfHealthCurve;


        ///
        /// Weights.
        ///

        public const float MaxWeight = 5;

        [Header("Weigths")]
        [Range(0,5f)][SerializeField] private float damageTakenIntervalWeight = 0;
        public float DamageTakenIntervalWeight => damageTakenIntervalWeight;

        [Range(0,5f)][SerializeField] private float distanceToObstacleWeight = 0;
        public float DistanceToObstacleWeight => distanceToObstacleWeight;

        [Range(0,5f)][SerializeField] private float distanceToOpponentWeight = 0;
        public float DistanceToOpponentWeight => distanceToOpponentWeight;

        [Range(0,5f)][SerializeField] private float normalisedOpponentHealthWeight = 0;
        public float NormalisedOpponentHealthWeight => normalisedOpponentHealthWeight;

        [Range(0,5f)][SerializeField] private float normalisedSelfHealthWeight = 0;
        public float NormalisedSelfHealthWeight => normalisedSelfHealthWeight;

        ///
        /// Cache.
        /// 


        IOpponentContext opponentContext;
        IDamageIntervalContext damageIntervalContext;



        /// 
        /// Base.
        /// 

        public override float MaxScore => MaxWeight;

        protected override bool IsPossible()
        {
            return 
            WithinFov(opponentContext.DotDirectionToTarget) == true
            && IsOnCooldown() == false;
        }

        protected override void RetrieveContextTypes(AiAgentContext context)
        {
            opponentContext = context as IOpponentContext;
            damageIntervalContext = context as IDamageIntervalContext;
        }

        protected override float Evaluate()
        {
            float selfHealthScore
                = normalisedSelfHealthWeight > 0
                ? normalisedSelfHealthCurve.Evaluate(damageIntervalContext.SelfHealth.GetNormalisedHealthValue()) * normalisedSelfHealthWeight
                : 0;

            float distanceToObstacleScore
                = distanceToObstacleWeight > 0
                ? distanceToObstacleCurve.Evaluate(0) * distanceToObstacleWeight
                : 0;

            float distanceToOpponentScore
                = distanceToOpponentWeight > 0
                ? distanceToOpponentCurve.Evaluate(opponentContext.DistanceToTarget) * distanceToOpponentWeight
                : 0;

            float opponentHealthScore
                = normalisedOpponentHealthWeight > 0
                ? normalisedOpponentHealthCurve.Evaluate(opponentContext.HealthSystem.GetNormalisedHealthValue()) * normalisedOpponentHealthWeight
                : 0;

            float damageTakenIntervalScore
                = damageTakenIntervalWeight > 0
                ? damageTakenIntervalCurve.Evaluate(damageIntervalContext.DamageTakenInCurrentInterval) * damageTakenIntervalWeight
                : 0;

            return 
                selfHealthScore 
                + distanceToObstacleScore 
                + distanceToOpponentScore 
                + opponentHealthScore 
                + damageTakenIntervalScore;            
        }

#if UNITY_EDITOR

        /// <summary>
        /// Calls OnValidate for the Unity Editor. This should never be called outside of a UNITY_EDITOR 'if' macro.
        /// </summary>

        public override void OnValidate()
        {
            base.OnValidate();
            EditorClampTotalWeightToMaxWeight();
        }

        /// <summary>
        /// Ensures that total weight is never higher than max weight.
        /// </summary>

        private void EditorClampTotalWeightToMaxWeight()
        {
            // Calculate sum
            float sum = normalisedSelfHealthWeight + distanceToObstacleWeight + distanceToOpponentWeight + normalisedOpponentHealthWeight + damageTakenIntervalWeight;

            // Scale values if sum exceeds the max
            if (sum > MaxWeight)
            {
                float scale = MaxWeight / sum;
                normalisedSelfHealthWeight *= scale;

                if (normalisedSelfHealthWeight < 0.1f)
                {
                    normalisedSelfHealthWeight = 0;
                }

                distanceToObstacleWeight *= scale;

                if (distanceToObstacleWeight < 0.1f)
                {
                    distanceToObstacleWeight = 0;
                }

                distanceToOpponentWeight *= scale;

                if (distanceToOpponentWeight < 0.1f)
                {
                    distanceToOpponentWeight = 0;
                }

                normalisedOpponentHealthWeight *= scale;

                if (normalisedOpponentHealthWeight < 0.1f)
                {
                    normalisedOpponentHealthWeight = 0;
                }

                damageTakenIntervalWeight *= scale;

                if (damageTakenIntervalWeight < 0.1f)
                {
                    damageTakenIntervalWeight = 0;
                }
            }
        }
#endif
    }


}
