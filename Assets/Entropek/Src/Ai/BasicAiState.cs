using UnityEngine;

namespace Entropek.Ai
{
    public class BasicAiState : AiState
    {
        [Header("Curves")]
        [SerializeField] AnimationCurve distanceToOpponent;
        public AnimationCurve DistanceToOpponent => distanceToOpponent;

        public float Evaluate(float distanceToOpponent)
        {
            return DistanceToOpponent.Evaluate(distanceToOpponent);
        }

#if UNITY_EDITOR
        public override void OnValidate()
        {
            base.OnValidate();
            UnityUtils.AnimationCurve.Clamp01AnimationCurveKeyValues(distanceToOpponent);
        }
#endif
    }
}

