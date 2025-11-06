using Entropek.EntityStats;
using Entropek.Exceptions;
using UnityEngine;

namespace Entropek.Combat
{
    public class Opponent : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] HealthSystem healthSystem;
        public HealthSystem HealthSystem => healthSystem; 

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

