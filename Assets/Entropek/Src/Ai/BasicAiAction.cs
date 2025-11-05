using UnityEngine;

namespace Entropek.Ai
{    
    [System.Serializable]
    public class BasicAiAction : AiAction
    {
        [Header("Curves")]
        
        [SerializeField] private AnimationCurve distanceToOpponentCurve = new AnimationCurve(new Keyframe(0f,1f), new Keyframe(1f,0f));
        public AnimationCurve DistanceToOpponentCurve => distanceToOpponentCurve;

        public float Evaluate(float distanceToOpponent)
        {
            return distanceToOpponentCurve.Evaluate(distanceToOpponent);
        }

        public override float GetMaxScore()
        {
            return 1;
        }
    }
}

