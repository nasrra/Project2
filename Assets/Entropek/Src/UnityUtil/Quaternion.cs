using UnityEngine;

namespace Entropek.UnityUtils
{
    public static class QuaternionUtils
    {
        /// <summary>
        /// projects a directional vector - relativate to a rotation - onto a target directional vector.
        /// Note:
        ///     Example use case: Aligning the up vector of a gameobject to a up direction of another gameobject.
        /// </summary>
        /// <param name="currentRotation">The current rotation of the transform to align.</param>
        /// <param name="currentDirectionalVector">The direction - relatative to the transform - that will be set to the target direction.</param>
        /// <param name="targetDirectionalVector">The target direction for the transform to rotate to.</param>
        /// <returns></returns>

        public static Quaternion AlignRotationToVector(Quaternion currentRotation, Vector3 currentDirectionalVector, Vector3 targetDirectionalVector)
        {

            // Get the rotation needed to align up with the ground.

            Quaternion alignToGround = Quaternion.FromToRotation(currentDirectionalVector, targetDirectionalVector) * currentRotation;

            // Extract Euler Angles.

            Vector3 originalEuler = currentRotation.eulerAngles;
            Vector3 targetEuler = alignToGround.eulerAngles;

            Vector3 finalEuler = new Vector3(targetEuler.x, originalEuler.y, targetEuler.z);
            return Quaternion.Euler(finalEuler);
        }    
    }    
}

