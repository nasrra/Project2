using UnityEngine;

namespace Entropek.Physics
{
    public struct CharacterControllerMovementLinearImpulse
    {
        public Vector3 Direction{get; private set;}
        public float Force{get; private set;}
        public float DecaySpeed{get; private set;}

        public CharacterControllerMovementLinearImpulse(Vector3 direction, float force, float decaySpeed)
        {
            Direction = direction;
            Force = force;
            DecaySpeed = decaySpeed;
        }
    }    
}

