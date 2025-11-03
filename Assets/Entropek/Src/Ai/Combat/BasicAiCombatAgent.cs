using UnityEngine;

namespace Entropek.Ai.Combat
{
    [RequireComponent(typeof(SphereCollider))]
    public class BasicAiCombatAgent : AiCombatAgent<BasicAiCombatAction>
    {
        protected override void GeneratePossibleActions()
        {
            CalculateRelationToEngagedOpponent(
                out Vector3 vectorDistanceToOpponent, 
                out float distanceToOpponent, 
                out float dotDirectionToOpponent
            );

            // clear the previous evaluateion call options.
            
            possibleCombatActions.Clear();

            for(int i = 0; i < aiCombatActions.Length; i++)
            {
                BasicAiCombatAction currentEvaluation = aiCombatActions[i];

                // if the action is not enabled or currently on cooldown.

                if (currentEvaluation.Enabled == false || currentEvaluation.CooldownTimer.HasTimedOut == false)
                {
                    continue;
                }

                // check if the target is currently in view of the action.

                if (dotDirectionToOpponent < currentEvaluation.MinFov
                || dotDirectionToOpponent > currentEvaluation.MaxFov)
                {
                    continue;
                }

                float score = currentEvaluation.Evaluate(distanceToOpponent);

                possibleCombatActions.Add((currentEvaluation, score));
            }

            // sort in descending order.

            possibleCombatActions.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        }
    }    
}

