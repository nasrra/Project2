using UnityEngine;

namespace Entropek.Physics
{
    public struct NavAgentPathCornerContext
    {
        public Vector3 VectorDistanceToCorner {get; private set;}
        public Vector3 DirectionToCorner {get; private set;}
        public float DistanceToCorner {get; private set;}

        public NavAgentPathCornerContext(Vector3 vectorDistanceCorner, Vector3 directionToCorner, float distanceToCorner)
        {
            VectorDistanceToCorner = vectorDistanceCorner;
            DirectionToCorner = directionToCorner;
            DistanceToCorner = distanceToCorner;
        }
    }    
}

