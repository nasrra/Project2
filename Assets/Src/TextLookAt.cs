using Entropek.UnityUtils.Attributes;
using UnityEngine;

public class TextLookAt : MonoBehaviour
{
    [RuntimeField] public Transform LookAtTarget;

    private void LateUpdate()
    {

        // look at the target.

        Vector3 direction = transform.position - LookAtTarget.position;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
