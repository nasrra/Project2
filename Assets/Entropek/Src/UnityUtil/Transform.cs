using UnityEngine;

namespace Entropek.UnityUtils{

public class TransformUtils{

    /// <summary>
    /// Rotates a Transform component around the y-axis (in world space) to face a given direction.
    /// </summary>
    /// <param name="transform">The transform component to rotate.</param>
    /// <param name="direction">The direction to look towards.</param>
    /// <param name="slerpAmount">the amount (0-1) to lerp the transform in when facing the direction.</param>

    public static void RotateYAxisToDirection(UnityEngine.Transform transform, Vector3 direction, float slerpAmount){
        Vector3 lookRotation = Quaternion.LookRotation(direction).eulerAngles;
        Quaternion yRotation = Quaternion.Euler(0,lookRotation.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, yRotation, slerpAmount);
    }
}

}

