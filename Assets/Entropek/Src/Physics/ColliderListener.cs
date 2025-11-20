using System;
using UnityEngine;

namespace Entropek.Physics
{
    /// <summary>
    /// This is a wrapper class over the base UnityEngine Collider.
    /// Attatch this to a gameobject with a collider to gain access to its specific callbacks.
    /// </summary>

    public class ColliderListener : MonoBehaviour
    {
        public event Action<Collider> TriggerEnter;
        public event Action<Collider> TriggerExit;

        private void OnTriggerEnter(Collider other)
        {
            TriggerEnter?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            TriggerExit?.Invoke(other);
        }
    }    
}

