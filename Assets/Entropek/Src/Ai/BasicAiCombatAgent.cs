using UnityEngine;

namespace Entropek.Ai
{
    [RequireComponent(typeof(SphereCollider))]
    public class BasicAiActionAgent : AiActionAgent<BasicAiAction>
    {
        protected override void GeneratePossibleOutcomes(in AiActionAgentRelationToOpponentContext relationToOpponentContext)
        {
            for(int i = 0; i < aiActions.Length; i++)
            {
                ref BasicAiAction currentEvaluation = ref aiActions[i];

                // if the action is not enabled or currently on cooldown.

                if (currentEvaluation.Enabled == false || currentEvaluation.CooldownTimer.HasTimedOut == false)
                {
                    continue;
                }

                // check if the target is currently in view of the action.

                if (relationToOpponentContext.DotDirectionToOpponent < currentEvaluation.MinFov
                || relationToOpponentContext.DotDirectionToOpponent > currentEvaluation.MaxFov)
                {
                    continue;
                }

                float score = currentEvaluation.Evaluate(relationToOpponentContext.DistanceToOpponent);

                possibleOutcomes.Add(
                    new AiPossibleOutcome(
                        currentEvaluation.Name,
                        score,
                        currentEvaluation.GetMaxScore(),
                        i
                    )
                );
            }
        }
    }    
}

