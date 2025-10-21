using System;
using Entropek.Time;
using UnityEngine;

namespace Entropek.Systems.Ai.Combat{


[Serializable]
public class AiCombatAction{

    public const float MaxWeight = 5;

    [Header("Action Name")]
    [SerializeField] private string actionName;
    public string ActionName => actionName; 

    // the cooldown of the action.

    [Header("Timers")]

    [SerializeField] private Timer cooldownTimer;
    public Timer CooldownTimer => cooldownTimer;


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



    public float Evaluate(float damageTakenInterval, float distanceToOpponent, float distanceToObstacle, float normalisedOpponentHealth, float noramlisedSelfHealth){
        
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

    private void OnValidate(){
        // Calculate sum
        float sum = normalisedSelfHealthWeight + distanceToObstacleWeight + distanceToOpponentWeight + normalisedOpponentHealthWeight + damageTakenIntervalWeight;

        // Scale values if sum exceeds the max
        if (sum > MaxWeight){
            float scale                 = MaxWeight / sum;
            normalisedSelfHealthWeight  *= scale;
            
            if(normalisedSelfHealthWeight < 0.1f){
                normalisedSelfHealthWeight = 0;
            }
            
            distanceToObstacleWeight    *= scale;

            if(distanceToObstacleWeight < 0.1f){
                distanceToObstacleWeight = 0;
            }

            distanceToOpponentWeight    *= scale;

            if(distanceToOpponentWeight < 0.1f){
                distanceToOpponentWeight = 0;
            }

            normalisedOpponentHealthWeight *= scale;

            if(normalisedOpponentHealthWeight < 0.1f){
                normalisedOpponentHealthWeight = 0;
            }

            damageTakenIntervalWeight   *= scale;
            
            if(damageTakenIntervalWeight < 0.1f){
                damageTakenIntervalWeight = 0;
            }
        }
    }
}


}
