using System;
using UnityEngine;


namespace Entropek.Ai.Combat
{
    /// <summary>
    /// This base class serves as an entry point for an entity that wants to interact
    /// with any type of Ai Combat agent; such as an enemy base class.
    /// </summary>
    
    [Serializable]
    public abstract class AiCombatAgentBase : MonoBehaviour
    {
        public abstract event Action<AiCombatAction> ActionChosen;
        public abstract event Action<Transform> EngagedOpponent;
        public abstract AiCombatAction ChosenCombatAction{get;}

        public abstract void Evaluate();
        public abstract void DisengageOpponent();
        public abstract void EngageOpponent(Transform opponentTransform);
        public abstract void HaltEvaluationLoop();
        public abstract void BeginEvaluationLoop();
        public abstract void BeginChosenCombatActionCooldown();
    }
}
