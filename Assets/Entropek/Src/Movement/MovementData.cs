using System;
using UnityEngine;

namespace Entropek.Physics
{
    [Serializable]
    public struct MovementData{
        [SerializeField] private float acceleration;
        public float Acceleration => acceleration;

        [SerializeField] private float deceleration;
        public float Deceleration => deceleration;

        [SerializeField] private float maxSpeed;
        public float MaxSpeed => maxSpeed;

        [SerializeField] private float gravityModifier;
        public float GravityModifier => gravityModifier; 

        public MovementData(
            float acceleration, 
            float deceleration,
            float maxSpeed,
            float gravityModifier
        ){
            this.acceleration       = acceleration;
            this.deceleration       = deceleration;
            this.maxSpeed           = maxSpeed;
            this.gravityModifier    = gravityModifier;
        }
    }
    
}


