using UnityEngine;

namespace Entropek.Physics
{

    public struct LinearForceVelocity
    {
        public Vector3 Velocity;
        public float DecaySpeed { get; private set; }

        public LinearForceVelocity(Vector3 direction, float force, float decaySpeed)
        {
            Velocity = direction * force;
            DecaySpeed = decaySpeed;
        }

        public LinearForceVelocity(Vector3 velocity, float decaySpeed)
        {
            Velocity = velocity;
            DecaySpeed = decaySpeed;
        }
    }

}

