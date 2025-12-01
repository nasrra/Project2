using Entropek.Ai.Contexts;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Ai
{    
    [System.Serializable]
    public class BasicCombatAiAction : AiAction
    {


        /// 
        /// Data.
        /// 


        [Header("Curves")]
        [SerializeField] private AnimationCurve distanceToOpponentCurve = new AnimationCurve(new Keyframe(0f,1f), new Keyframe(1f,0f));
        public AnimationCurve DistanceToOpponentCurve => distanceToOpponentCurve;


        ///
        /// Cache.
        /// 


        ITargetContext targetContext;


        /// 
        /// Base.
        /// 

        public override float MaxScore => 1;

        protected override bool IsPossible()
        {
            return 
                WithinFov(targetContext.DotDirectionToTarget) == true
                && IsOnCooldown() == false;
        }

        protected override float Evaluate()
        {
            return distanceToOpponentCurve.Evaluate(targetContext.DistanceToTarget);
        }

        protected override void RetrieveContextTypes(AiAgentContext context)
        {
            targetContext = context as ITargetContext;
        }
    }
}

