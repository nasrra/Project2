using System;
using UnityEngine;

namespace Entropek.Vfx
{
    public abstract class VfxPlayer : MonoBehaviour, IDeactivatable
    {
        public event Action Finished;
        public event Action Activated;
        public event Action Deactivated;

        public void Activate()
        {
            gameObject.SetActive(true);
            Activated?.Invoke();
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
            Deactivated?.Invoke();
        }

        /// <summary>
        /// Plays the Vfx instance at the current world position and rotation of the attatched gameObject.
        /// </summary>

        public virtual void Play()
        {
            
            // Activate to re enable the update loop to check
            // if this vfx instance has finished.

            Activate();
        }
        
        /// <summary>
        /// Plays the Vfx instance and moves this VfxPlayer to the specfied world position.
        /// </summary>
        /// <param name="worldSpacePosition">The world position to play at.</param>

        public virtual void Play(Vector3 worldSpacePosition)
        {            
            transform.position = worldSpacePosition;
            Play();
        }
        
        /// <summary>
        /// Plays the Vfx instance, moving this VfxPlayer to a world position at a specified rotation.
        /// </summary>
        /// <param name="worldSpacePosition">The world position to play at.</param>
        /// <param name="rotationEuler">The rotation to be at when playing.</param>

        public virtual void Play(Vector3 worldSpacePosition, Vector3 rotationEuler)
        {
            transform.rotation = Quaternion.LookRotation(rotationEuler);
            Play(worldSpacePosition);
        }
        
        /// <summary>
        /// Checks whether or not the vfx instance has completed playing.
        /// </summary>
        /// <returns>true, if completed; otherwise false.</returns>

        protected abstract bool IsFinished();

        private void LateUpdate()
        {
            if (IsFinished() == true)
            {
                Finished?.Invoke();

                // Deactivate to stop from checking is finished every frame
                // when this vfx has already stopped.

                Deactivate(); 
            }
        }
    }
    
}

