using Entropek.EntityStats;
using Entropek.Exceptions;
using Entropek.Physics;
using UnityEngine;

namespace Entropek.Combat
{
    // NOTE:
    //  Default execution order should be before any class that needs this singleton,
    //  as they need to get the singleton reference in Awake (such as enemies).
    
    [DefaultExecutionOrder(-1)]
    
    public class Opponent : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] HealthSystem healthSystem;
        public HealthSystem HealthSystem => healthSystem; 

        [SerializeField] NavAgentMovementTarget navAgentMovementTarget;
        public NavAgentMovementTarget NavAgentMovementTarget => navAgentMovementTarget; 

        public static Opponent Singleton;

        void Awake()
        {
            if(Singleton != null)
            {
                throw new SingletonException("There can only be on active Opponent in the scene.");
            }
            else
            {
                Singleton = this;
            }
        }

        void OnDestroy()
        {
            if(Singleton == this)
            {
                Singleton = null;
            }
        }
    }    
}

