using UnityEngine;

namespace Entropek.Ai
{
    /// <summary>
    /// A possible outcome that an AiAgent can choose during the evluation process;
    /// Outcome types include AiAction and AiState.
    /// </summary>

    public struct AiPossibleOutcome
    {
        /// <summary>
        /// Gets the name of this outcome.
        /// </summary>
        
        public string Name {get; private set;} 

        /// <summary>
        /// Gets the score given to the outcome after an evaluation.
        /// </summary>

        public float EvaluationScore {get; private set;}

        /// <summary>
        /// Gets the max possible score of an outcome when being evaluated.
        /// </summary>

        public float OutcomeMaxScore {get; private set;}

        /// <summary>
        /// Gets the index of the outcome in the collection that stores the outcome.
        /// Note:
        ///     Outcomes should always be in a collection where ordering does not change (array or list).
        /// </summary>

        public int OutcomeIndex {get; private set;}

        /// <summary>
        /// Creates a new AiPossibleOutcome for an AiAgent to consider when choosing an outcome.
        /// </summary>
        /// <param name="name">The name of the outcome.</param>
        /// <param name="evaluationScore">The score given to the outcome after an evaluation.</param>
        /// <param name="outcomeMaxScore">The max possible score of an outcome when being evaluated.</param>
        /// <param name="outcomeIndex">the index of the outcome in the collection that stores the outcome.
        /// Note:
        ///     Outcomes should always be in a collection where ordering does not change (array or list).
        /// </param>

        public AiPossibleOutcome(string name, float evaluationScore, float outcomeMaxScore, int outcomeIndex)
        {
            Name = name;
            EvaluationScore = evaluationScore;
            OutcomeMaxScore = outcomeMaxScore;
            OutcomeIndex = outcomeIndex;
        }
    }    
}

