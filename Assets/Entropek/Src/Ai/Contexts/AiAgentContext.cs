using UnityEngine;

namespace Entropek.Ai.Contexts
{    
    [System.Serializable]
    public abstract class AiAgentContext : MonoBehaviour
    {
        public abstract void HaltEvaluationLoop();
        public abstract void BeginEvaluationLoop();   

        /// <summary>
        /// Evaluates the context based on this gameobjects transform.
        /// </summary>

        public abstract void Evaluate();
        protected abstract void RetrieveContextTypes();
        
        protected virtual void Awake()
        {
            RetrieveContextTypes();
        }
    }
}
