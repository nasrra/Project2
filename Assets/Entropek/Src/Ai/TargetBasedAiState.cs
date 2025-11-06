using Entropek.Ai;
using Entropek.Ai.Contexts;
using UnityEngine;

namespace Entropek.Ai
{
    public class TargetBasedAiState : AiState
    {
        [Header("Curves")]
        [SerializeField] private AnimationCurve distanceToTargetCurve;
        public AnimationCurve DistanceToTargetCurve => distanceToTargetCurve;

        ITargetContext targetContext;

        public override float MaxScore => 1;

        protected override float Evaluate()
        {
            return distanceToTargetCurve.Evaluate(targetContext.DistanceToTarget);
        }

        protected override bool IsPossible()
        {
            return true;
        }

        protected override void RetrieveContextTypes(AiAgentContext context)
        {
            targetContext = context as ITargetContext;
        }
    }    
}

