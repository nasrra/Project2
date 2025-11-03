using UnityEngine;

namespace Entropek.Ai.Combat
{
    
    public struct AiCombatAgentRelationToOpponentContext
    {
        /// <summary>
        /// The vector distance from this agent to the engaged opponent.
        /// </summary>

        public Vector3 VectorDistanceToOpponent {get; private set;}

        /// <summary>
        /// The float distance from this agent to the engaged opponent.
        /// </summary>

        public float DistanceToOpponent {get; private set;}   
    
        /// <summary>
        /// The dot product that represents if the agent is facing (1) or looking away (-1) from the engaged opponent.
        /// </summary>
            
        public float DotDirectionToOpponent {get; private set;}


        /// <summary>
        /// Creates a new AiCombatActionChosenCallbackContext
        /// </summary>
        /// <param name="VectorDistanceToOpponent">The vector distance from this agent and the engaged opponent.</param>
        /// <param name="DistanceToOpponent">The float distance from this agent and the engaged opponent.</param>
        /// <param name="DotDirectionToOpponent">The dot product that represents if the agent is facing (1) or looking away (-1) from the engaged opponent.</param>

        public AiCombatAgentRelationToOpponentContext
        (
            Vector3 VectorDistanceToOpponent,
            float DistanceToOpponent,
            float DotDirectionToOpponent
        )
        {
            this.VectorDistanceToOpponent = VectorDistanceToOpponent;
            this.DistanceToOpponent = DistanceToOpponent;
            this.DotDirectionToOpponent = DotDirectionToOpponent;
        }
    }

}

