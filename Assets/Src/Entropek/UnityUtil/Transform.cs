using UnityEngine;

namespace Entropek.UnityUtils{

public class Transform{
    public static void RotateYAxisToDirection(UnityEngine.Transform transform, Vector3 direction, float speed){
        Vector3 lookRotation = Quaternion.LookRotation(direction).eulerAngles;
        Quaternion yRotation = Quaternion.Euler(0,lookRotation.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, yRotation, speed);
    }
}

}

