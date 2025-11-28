using System;
using System.Collections;
using UnityEngine;

namespace Entropek.Vfx
{
    public abstract class VfxPlayer : MonoBehaviour, IDeactivatable
    {
        public event Action Finished;
        public event Action Activated;
        public event Action Deactivated;

        /// <summary>
        /// Flag for whether or not Play() was called this frame;
        /// to ensure that checking if this vfx is finished playing doesnt
        /// incorrectly return true when just starting.
        /// </summary>

        private bool playedThisFrame = false;

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
            
            playedThisFrame = true;

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
        /// Plays a Vfx instance, moving this VfxPlayer to a world position at a specified rotation.
        /// </summary>
        /// <param name="worldSpacePosition">The world position to play at.</param>
        /// <param name="rotation">The rotation to be at when playing.</param>

        public virtual void Play(Vector3 worldSpacePosition, Quaternion rotation)
        {
            transform.rotation = rotation;
            Play(worldSpacePosition);
        }


        /// <summary>
        /// Stops a vfx instance.
        /// </summary>
        /// <param name="deactivateTime">The time - in seconds - to wait before deactivation; freeing this vfx player in the pooler.</param>

        public void Stop(float deactivateTime)
        {
            StopInternal();
            StartCoroutine(DefferedDeactivate(deactivateTime));
        }

        protected IEnumerator DefferedDeactivate(float deactivateTime)
        {
            yield return new WaitForSeconds(deactivateTime);
            Deactivate();
            yield break;
        }

        /// <summary>
        /// The internal implementation for stopping a vfx player.
        /// </summary>

        protected abstract void StopInternal();
        
        /// <summary>
        /// Checks whether or not the vfx instance has completed playing.
        /// Note:
        ///     Always check IsFinished in Update not LateUpdate as it will give incorrectly return true on the first frame when playing a vfx instance. 
        /// </summary>
        /// <returns>true, if completed; otherwise false.</returns>

        protected abstract bool IsFinished();

        private void Update()
        {
            if (playedThisFrame == false && IsFinished() == true)
            {
                Finished?.Invoke();

                // Deactivate to stop from checking is finished every frame
                // when this vfx has already stopped.

                Deactivate(); 
            }
            playedThisFrame = false;
        }
    }
    
}

