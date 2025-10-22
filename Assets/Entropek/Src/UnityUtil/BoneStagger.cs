using System;
using UnityEngine;

namespace Entropek.UnityUtils
{

    public class BoneStagger : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private UnityEngine.Transform[] affectedBones;
        [SerializeField] private Vector3 rotationOffset;
        [SerializeField] private float staggerDuration;
        float timer;
        private event Action staggerCallback;

        void LateUpdate()
        {
            staggerCallback?.Invoke();
        }

        public void TriggerStagger()
        {
            timer = staggerDuration;
            staggerCallback = StaggerFunction;
        }
        
        private void StaggerFunction()
        {
            float t = 1 - (timer / staggerDuration);
            timer -= UnityEngine.Time.deltaTime;

            if (timer <= 0)
            {
                staggerCallback = null;
            }
            for (int i = 0; i < affectedBones.Length; i++)
            {
                affectedBones[i].localRotation *= Quaternion.Euler(rotationOffset * Mathf.Sin(t * Mathf.PI));
            }
        }
    }
    
}

