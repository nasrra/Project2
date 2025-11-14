using UnityEngine;

namespace Entropek.Physics
{

    public struct DynamicForceVelocity
    {
        public Vector3 Direction { get; private set; }
        public AnimationCurve Force { get; private set; }
        public float ElapsedTime {get; private set;}
        public float MaxTime {get; private set;}

        public DynamicForceVelocity(Vector3 direction, AnimationCurve force, float maxTime)
        {
            Direction = direction;
            Force = force;
            ElapsedTime = 0;
            MaxTime = maxTime;
        }

        /// <summary>
        /// Calculates the velocity to be applied at the timestep equal to this struct's ElapsedTime field.
        /// </summary>
        /// <returns>The velocity to be applied.</returns>

        public Vector3 GetVelocity()
        {
            return GetVelocity(ElapsedTime);
        }

        /// <summary>
        /// Calculates the velocity to be applied at the specified elapsedTime parameter.
        /// </summary>
        /// <param name="elapsedTime">The specified elapsed time.</param>
        /// <returns>The velocity to be applied.</returns>

        public Vector3 GetVelocity(float elapsedTime)
        {
            return Direction * Force.Evaluate(elapsedTime);
        }

        /// <summary>
        /// Increments this struct's ElapsedTime by the specified timestep parameter.
        /// </summary>
        /// <param name="timeStep">The specified amount of time to add to ElapsedTime</param>

        public void IncrementElapsedTime(float timeStep)
        {
            ElapsedTime += timeStep;
        }

        /// <summary>
        /// Checks whether the internal ElapsedTime is greater than the set MaxTime.
        /// </summary>
        /// <returns>true, if this ElapsedTime is greater than this struct's Maxtime; otherwise false.</returns>
        
        public bool IsCompleted()
        {
            return ElapsedTime >= MaxTime;
        }
    }

}

