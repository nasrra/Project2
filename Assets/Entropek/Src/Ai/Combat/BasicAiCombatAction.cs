using UnityEngine;

namespace Entropek.Ai.Combat
{    
    [System.Serializable]
    public class BasicAiCombatAction : AiCombatAction
    {
        [Header("Curves")]
        private AnimationCurve distanceToOpponentCurve = new AnimationCurve(new Keyframe(0f,1f), new Keyframe(1f,0f));
        public AnimationCurve DistanceToOpponentCurve => distanceToOpponentCurve;

        public float Evaluate(float distanceToOpponent)
        {
            return distanceToOpponentCurve.Evaluate(distanceToOpponent);
        }

        public override float GetMaxWeight()
        {
            return 1;
        }
    }
}

