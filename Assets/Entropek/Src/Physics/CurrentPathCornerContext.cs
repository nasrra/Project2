using UnityEngine;

public struct PathCornerContext
{
    public Vector3 VectorDistanceToCorner {get; private set;}
    public Vector3 DirectionToCorner {get; private set;}
    public float DistanceToCorner {get; private set;}

    public PathCornerContext(Vector3 vectorDistanceToCorner, Vector3 directionToCorner, float distanceToCorner)
    {
        VectorDistanceToCorner = vectorDistanceToCorner;
        DirectionToCorner = directionToCorner;
        DistanceToCorner = distanceToCorner;
    }
}
