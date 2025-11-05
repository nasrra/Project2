using UnityEngine;

namespace Entropek.Ai
{
    public class BasicAiStateAgent : AiStateAgent<BasicAiState>
    {
        protected override void GeneratePossibleOutcomes(in AiActionAgentRelationToOpponentContext context)
        {
            for(int i = 0; i < aiStates.Length; i++)
            {
                BasicAiState evaluation = aiStates[i];
                
                if(evaluation.Enabled == false)
                {
                    return;
                }

                float score = evaluation.Evaluate(context.DistanceToOpponent);
                
                // modify the evaluation score by the lifetime of the state, so that the state isntimmediately switched.
                
                score *= ChosenState.StateSwitchChanceOverLifetime.Evaluate(ChosenStateCurrentLifetime);

                possibleOutcomes.Add(
                    new AiPossibleOutcome(
                        evaluation.Name,
                        score,
                        evaluation.GetMaxScore(),
                        i
                    )
                );
            }
        }
    }
}

