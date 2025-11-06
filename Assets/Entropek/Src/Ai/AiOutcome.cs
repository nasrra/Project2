using Entropek.Ai.Contexts;
using UnityEngine;

namespace Entropek.Ai
{
    public abstract class AiOutcome
    {
        [Header("Parameters")]
        [SerializeField] private string name;
        public string Name => name;
        [SerializeField] public bool Enabled = true;

        /// <summary>
        /// Internal check if this AiOutcome has retrieved its context types;
        /// Stops needing to check if all retrieved contex types are null in each RetriveContextTypes call.
        /// </summary>

        private bool retrievedContextTypes = false;

        public abstract float MaxScore{get;}

        /// <summary>
        /// Checks whether or not this outcome is possible within the given context; 
        /// and be considered for evluation in the decision making process.
        /// </summary>
        /// <param name="context">The AiContext to check against.</param>
        /// <returns>true, if this outcome is possible; otherwise false.</returns>

        public bool IsPossible(AiAgentContext context)
        {
            RetrieveContextTypesInternal(context);
            return IsPossible();
        }


        /// <summary>
        /// Checks whether or not this outcome is possible within the given context; 
        /// and be considered for evluation in the decision making process.
        /// </summary>
        /// <returns>true, if this outcome is possible; otherwise false.</returns>

        protected abstract bool IsPossible();

        /// <summary>
        /// Returns a score between 0 and 1 for how desirable this outcome is.
        /// (eg. 100% desirability will allways occur, 50% desirability will only occur half the time).
        /// </summary>
        /// <param name="context">The AiAgentContext to evluate against.</param>
        /// <returns></returns>

        public float Evaluate(AiAgentContext context)
        {
            RetrieveContextTypesInternal(context);
            return Evaluate();
        }

        /// <summary>
        /// Returns a score between 0 and 1 for how desirable this outcome is.
        /// (eg. 100% desirability will allways occur, 50% desirability will only occur half the time).
        /// </summary>
        /// <returns></returns>

        protected abstract float Evaluate();

        /// <summary>
        /// If not done so already, retrieve and caches the context interface types implmented in this AiOutcome for later reusage.
        /// </summary>
        /// <param name="context"></param>

        private void RetrieveContextTypesInternal(AiAgentContext context)
        {
            if(retrievedContextTypes == false)
            {
                retrievedContextTypes = true;
                RetrieveContextTypes(context);
            }
        }

        /// <summary>
        /// Retrieves and caches the context interface types implmented in this AiOutcome for later reusage.
        /// </summary>
        /// <param name="context"></param>

        protected abstract void RetrieveContextTypes(AiAgentContext context);
    }    
}
