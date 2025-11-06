using UnityEngine;

namespace Entropek.Ai.Contexts
{    
    public interface IOpponentContext : ITargetContext
    {

        /// <summary>
        /// The opponents health system.
        /// </summary>

        EntityStats.HealthSystem HealthSystem{get; set;}
    }
}

