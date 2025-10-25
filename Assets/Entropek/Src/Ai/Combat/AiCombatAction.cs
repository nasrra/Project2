using System;
using UnityEngine;

namespace Entropek.Ai.Combat{


    [Serializable]
    public class AiCombatAction{

        public const float MaxWeight = 5;

        [Header("Action Name")]
        [SerializeField] private string name;
        public string Name => name; 

        // the cooldown of the action.

        [Header("Cooldown")]
        [SerializeField] private Time.OneShotTimer cooldownTimer;
        public Time.OneShotTimer CooldownTimer => cooldownTimer;


        [Header("Curves")]

        // The amount of health lost in a given time interval.

        [SerializeField] private AnimationCurve damageTakenIntervalCurve = new AnimationCurve(new Keyframe(0f,1f), new Keyframe(1f,0f));
        public AnimationCurve DamageTakenIntervalCurve => damageTakenIntervalCurve;

        // The Distance to the closest significant obstacle [walls, etc.]

        [SerializeField] private AnimationCurve distanceToObstacleCurve = new AnimationCurve(new Keyframe(0f,1f), new Keyframe(1f,0f));
        public AnimationCurve DistanceToObstacleCurve => distanceToObstacleCurve;

        // The distance from the Ai to the opponent.

        [SerializeField] private AnimationCurve distanceToOpponentCurve = new AnimationCurve(new Keyframe(0f,1f), new Keyframe(1f,0f));
        public AnimationCurve DistanceToOpponentCurve => distanceToOpponentCurve;

        // The amount of health of the opponent.

        [SerializeField] private AnimationCurve normalisedOpponentHealthCurve = new AnimationCurve(new Keyframe(0f,1f), new Keyframe(1f,0f));
        public AnimationCurve NormalisedOpponentHealthCurve => normalisedOpponentHealthCurve;

        // The amount of health this Ai currently has.

        [SerializeField] private AnimationCurve normalisedSelfHealthCurve = new AnimationCurve(new Keyframe(0f,1f), new Keyframe(1f,0f));
        public AnimationCurve NormalisedSelfHealthCurve => normalisedSelfHealthCurve;



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

        [Header("Fov")]
        [Tooltip("Fov is measured by dot product value (-1 - 1), not by angle")]
        [SerializeField][Range(-1,1)] private float maxFov;
        public float MaxFov => maxFov;
        [Tooltip("Fov is measured by dot product value (-1 - 1), not by angle")]
        [SerializeField][Range(-1, 1)] private float minFov;
        public float MinFov => minFov;

        [Header("Parameters")]
        // whether or not to turn towards the target before commiting to this action.
        [SerializeField] private bool turnToTarget;
        public bool TurnToTarget => turnToTarget;
        // whether or not this action is enabled and shoule be chosen by a combat agent.
        [SerializeField] private bool enabled = true;
        public bool Enabled => enabled;



        public float Evaluate(float damageTakenInterval, float distanceToOpponent, float distanceToObstacle, float normalisedOpponentHealth, float noramlisedSelfHealth)
        {

            float selfHealthScore
                = normalisedSelfHealthWeight > 0
                ? normalisedSelfHealthCurve.Evaluate(noramlisedSelfHealth) * normalisedSelfHealthWeight
                : 0;

            float distanceToObstacleScore
                = distanceToObstacleWeight > 0
                ? distanceToObstacleCurve.Evaluate(distanceToObstacle) * distanceToObstacleWeight
                : 0;

            float distanceToOpponentScore
                = distanceToOpponentWeight > 0
                ? distanceToOpponentCurve.Evaluate(distanceToOpponent) * distanceToOpponentWeight
                : 0;

            float opponentHealthScore
                = normalisedOpponentHealthWeight > 0
                ? normalisedOpponentHealthCurve.Evaluate(normalisedOpponentHealth) * normalisedOpponentHealthWeight
                : 0;

            float damageTakenIntervalScore
                = damageTakenIntervalWeight > 0
                ? damageTakenIntervalCurve.Evaluate(damageTakenInterval) * damageTakenIntervalWeight
                : 0;

            return selfHealthScore + distanceToObstacleScore + distanceToOpponentScore + opponentHealthScore + damageTakenIntervalScore;
        }

#if UNITY_EDITOR

        private float editorLastValidateMinFov;
        private float editorLastValidateMaxFov;

        /// <summary>
        /// Calls OnValidate for the Unity Editor. This should never be called outside of a UNITY_EDITOR 'if' macro.
        /// </summary>

        public void OnValidate()
        {
            EditorClampTotalWeightToMaxWeight();
            EditorRegulateFovValues();
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
        
        /// <summary>
        /// Ensures that min fov is never greater than max fov and max fov is never less than min fov.
        /// </summary>

        private void EditorRegulateFovValues()
        {

            /// Ensures that min fov is never greater than max fov.

            if (editorLastValidateMinFov != minFov)
            {                
                if (minFov > maxFov)
                {
                    // add 0.1f for margin of error, and clamp value between -1 and 1 (min and max of dot product values).

                    minFov = Mathf.Clamp((maxFov + 0.01f), -1, 1);
                }
            }

            // ensure that max fov is never less than min fov.

            if (editorLastValidateMaxFov != maxFov)
            {
                if (maxFov < minFov)
                {

                    // subtract 0.1f for margin of error, and clamp value between -1 and 1 (min and max of dot product values).

                    maxFov = Mathf.Clamp((minFov - 0.01f), -1, 1);
                }
            }

            editorLastValidateMaxFov = maxFov;
            editorLastValidateMinFov = minFov;
        }

#endif

    }


}
