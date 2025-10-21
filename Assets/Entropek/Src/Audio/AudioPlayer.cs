using System;
using System.Collections.Generic;
using Entropek.Collections;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.UIElements;

namespace Entropek.Audio
{

    public class AudioPlayer : MonoBehaviour
    {
        public List<AudioInstance> ActiveAudioInstances { get; private set; } = new();
        public Dictionary<string, SwapbackList<AudioInstance>> FreePooledAudioInstances {get; private set;} = new();
        
        // callbacks mus be stored here so they are not garbage collected as binding the 
        // callback wont prevent it from garbage collection; as callbacks are delegate types.
        
        public SwapbackList<EVENT_CALLBACK> OneShotCallbacks   {get; private set;}= new();
        public SwapbackList<EVENT_CALLBACK> PooledCallbacks    {get; private set;}= new();

        private enum PlaySoundEvaluationResult : byte
        {
            Reuse,      // reuse an already allocated pooled audio instance.
            Allocate,   // allocate and play a new pooled audio instance.
            OneShot,    // play a new one shot audio instance.
        }

        /// 
        /// Base.
        /// 

        void OnDestroy()
        {
            Clear();
        }

        /// <summary>
        /// Emit a global sound event from this audio player.
        /// </summary>
        /// <param name="eventName">The name of the event reference.</param>
        /// <param name="pooled">true, to pool the audio instance for later reuse. false, for one shot audio (e.g. music).</param>

        public void PlaySound(string eventName, bool pooled = true)
        {

            // release the instance immediately if the sound isn't pooled.

            bool release = !pooled;

            switch (EvaluatePlaySound(eventName, pooled, AudioInstanceType.Global, out int reuseIndex))
            {

                // reuse a free audio instance.

                case PlaySoundEvaluationResult.Reuse:
                    FreePooledAudioInstances[eventName][reuseIndex].EventInstance.start();
                    break;
                
                // allocate a new audio instance to pool.
                
                case PlaySoundEvaluationResult.Allocate:
                    ManagePooledEventInstanceLifetime(AudioManager.Singleton.PlayEvent(eventName, release));
                    break;
                
                // allocate a new one shot audio instance.
                
                case PlaySoundEvaluationResult.OneShot:
                    ManageOneShotEventInstanceLifetime(AudioManager.Singleton.PlayEvent(eventName, release));
                    break;
            }
        }

        /// <summary>
        /// Emit a positional sound event from this audio player.
        /// </summary>
        /// <param name="eventName">The name of the event reference.</param>
        /// <param name="postion">The position - in world space - to play this sound at.</param>
        /// <param name="pooled">true, to pool the audio instance for later reuse. false, for one shot audio (e.g. music).</param>

        public void PlaySound(string eventName, Vector3 position, bool pooled = true)
        {

            // release the instance immediately if the sound isn't pooled.

            bool release = !pooled;

            switch (EvaluatePlaySound(eventName, pooled, AudioInstanceType.Positional, out int reuseIndex))
            {

                // reuse a free audio instance.

                case PlaySoundEvaluationResult.Reuse:
                    
                    // set the instance position.
                    FMOD.ATTRIBUTES_3D attributes = new FMOD.ATTRIBUTES_3D
                    {
                        position    = AudioManager.Singleton.UnityToFmodVector(position),
                        velocity    = new FMOD.VECTOR { x = 0, y = 0, z = 0 },
                        forward     = new FMOD.VECTOR { x = 0, y = 0, z = 1},
                        up          = new FMOD.VECTOR { x = 0, y = 1, z = 0}
                    };
                    FreePooledAudioInstances[eventName][reuseIndex].EventInstance.set3DAttributes(attributes);
                    FreePooledAudioInstances[eventName][reuseIndex].EventInstance.start();
                    break;

                // allocate a new audio instance to pool.

                case PlaySoundEvaluationResult.Allocate:
                    ManagePooledEventInstanceLifetime(AudioManager.Singleton.PlayEvent(eventName, position, release));
                    break;

                // allocate a new one shot audio instance.

                case PlaySoundEvaluationResult.OneShot:
                    ManageOneShotEventInstanceLifetime(AudioManager.Singleton.PlayEvent(eventName, position, release));
                    break;
            }
        }

        /// <summary>
        /// Emit a sound event that is attatched to a gameobject's position; following it.
        /// </summary>
        /// <param name="eventName">The name of the event reference.</param>
        /// <param name="attatchedGameObject">The gameobject to be attatched to.</param>
        /// <param name="pooled">true, to pool the audio instance for later reuse. false, for one shot audio (e.g. music).</param>

        public void PlaySound(string eventName, GameObject attachedGameObject, bool pooled = true)
        {

            // release the instance immediately if the sound isn't pooled.

            bool release = !pooled;

            switch (EvaluatePlaySound(eventName, pooled, AudioInstanceType.Global, out int reuseIndex))
            {

                // reuse a free audio instance.

                case PlaySoundEvaluationResult.Reuse:

                    // bind to the new gameobject.
                    FMODUnity.RuntimeManager.AttachInstanceToGameObject(FreePooledAudioInstances[eventName][reuseIndex].EventInstance, attachedGameObject);
                    FreePooledAudioInstances[eventName][reuseIndex].EventInstance.start();
                    break;

                // allocate a new audio instance to pool.

                case PlaySoundEvaluationResult.Allocate:
                    ManagePooledEventInstanceLifetime(AudioManager.Singleton.PlayEvent(eventName, attachedGameObject, release));
                    break;

                // allocate a new one shot audio instance.

                case PlaySoundEvaluationResult.OneShot:
                    ManageOneShotEventInstanceLifetime(AudioManager.Singleton.PlayEvent(eventName, attachedGameObject, release));
                    break;
            }
        }

        /// <summary>
        /// Evaluates whether or not a PlaySound() function call should reuse or allocate an audio instance to play the sound.
        /// </summary>
        /// <param name="eventName">The name of the event reference.</param>
        /// <param name="pooled">true, to pool the audio instance for later reuse. false, for one shot audio (e.g. music).</param>
        /// <param name="audioInstanceType">The audio instance type to compare reusage against.</param>
        /// <param name="reuseIndex">The index in the stored freeAudioInstance list of an audio instance that can be reused.</param>
        /// <returns></returns>

        private PlaySoundEvaluationResult EvaluatePlaySound(string eventName, bool pooled, AudioInstanceType audioInstanceType, out int reuseIndex)
        {
            reuseIndex = -1;

            if (pooled == true)
            {
                // create a free list if there isn't one already.

                if (FreePooledAudioInstances.ContainsKey(eventName) == false)
                {
                    FreePooledAudioInstances.Add(eventName, new());
                }

                // if there is an free audio instance able to be played.

                if (FreePooledAudioInstances[eventName].Count > 0)
                {
                    SwapbackList<AudioInstance> freePooledInstances = FreePooledAudioInstances[eventName];

                    for (int i = 0; i < freePooledInstances.Count; i++)
                    {

                        // reuse the audio instance that shares the same instance type as the play sound call.
                        // This is done so that audio instances that dont share the same behaviours will never overlap with eachother
                        // when reusing an instance to play a sound.

                        if (freePooledInstances[i].Type == audioInstanceType)
                        {
                            reuseIndex = i;
                            return PlaySoundEvaluationResult.Reuse;
                        }
                    }
                }

                // allocate and play a new audio instance if there aren't any.

                return PlaySoundEvaluationResult.Allocate;
            }

            // the audio instance is a one shot if it isnt pooled.

            return PlaySoundEvaluationResult.OneShot;
        }

        /// <summary>
        /// Pauses all sounds that are currently emitted by this audio player.
        /// </summary>

        public void PauseAllSounds()
        {
            for (int i = 0; i < ActiveAudioInstances.Count; i++)
            {
                AudioInstance instance = ActiveAudioInstances[i];
                instance.EventInstance.setPaused(true);
            }
        }

        /// <summary>
        /// Resumes all sounds that were previously being emitted by this audio player.
        /// </summary>

        public void ResumeAllSounds()
        {
            for (int i = 0; i < ActiveAudioInstances.Count; i++)
            {
                AudioInstance instance = ActiveAudioInstances[i];
                instance.EventInstance.setPaused(false);
            }
        }

        /// <summary>
        /// Stops all sounds that are currently emitted by this audio player.
        /// </summary>
        /// <param name="immediate">true, to abruptly stop sound. false, to allow fade out.</param>

        public void StopAllSounds(bool immediate)
        {
            for (int i = 0; i < ActiveAudioInstances.Count; i++)
            {
                AudioInstance instance = ActiveAudioInstances[i];
                instance.EventInstance.stop(immediate == true ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
            }
        }

        /// <summary>
        /// stops the latest played instance of a sound.
        /// </summary>
        /// <param name="eventName">The name of the event reference.</param>
        /// <param name="immediate">true, to abruptly stop sound. false, to allow fade out.</param>

        public void StopSound(string eventName, bool immediate)
        {
            for (int i = ActiveAudioInstances.Count - 1; i >= 0; i--)
            {
                if (ActiveAudioInstances[i].Name == eventName)
                {
                    ActiveAudioInstances[i].EventInstance.stop(immediate == true ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
                    break;
                }
            }
        }


        ///
        /// Lifetime Callbacks.
        /// 


        /// <summary>
        /// Internally caches and tracks the audio instance, ensuring that id deallocated itself when stopped.
        /// </summary>
        /// <param name="audioInstance">The specified audio instance.</param>


        private void ManageOneShotEventInstanceLifetime(AudioInstance audioInstance)
        {

            // track the playing audio instance.

            ActiveAudioInstances.Add(audioInstance);

            // create a callback to stop tracking the lifetime of the audio instance
            // once it has stopped playing.

            EVENT_CALLBACK callback = null;
            callback = (EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr paramPtr) =>
            {

                // when the event instance has stopped playing.

                if (type == EVENT_CALLBACK_TYPE.STOPPED)
                {
                    // get the currently playing event instance that has just stopped.

                    EventInstance stoppedInstance = new EventInstance(instancePtr);

                    // remove the audio instance that equality matches to the stopped instance.

                    ActiveAudioInstances.Remove(audioInstance);

                    // release the actual playing event instance that has stopped.
                    // NOTE:
                    //  this is just a sanity check as the instance has already been released when first starting to play.

                    stoppedInstance.release();

                    // allow this callback to be garbage collected once completed.

                    OneShotCallbacks.Remove(callback);
                }


                return FMOD.RESULT.OK;
            };

            // bind the callback to the audio instance.

            audioInstance.EventInstance.setCallback(callback, EVENT_CALLBACK_TYPE.STOPPED);

            // cache the callback to stop it from being garbage collected.

            OneShotCallbacks.Add(callback);
        }

        /// <summary>
        /// Internally caches and trackes the audio instance, placing it in a callback loop to ensure reusage of allocated instances.
        /// </summary>
        /// <param name="audioInstance">The specified audio instance.</param>

        private void ManagePooledEventInstanceLifetime(AudioInstance audioInstance)
        {
            EVENT_CALLBACK callback = null;
            callback = (EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr paramPtr) =>
            {
                // when the instance has started playing.

                if (type == EVENT_CALLBACK_TYPE.STARTED)
                {
                    // remove the audio instance the equality matches to the started instance
                    // from the free instance list.

                    FreePooledAudioInstances[audioInstance.Name].Remove(audioInstance);

                    // add the audio instance back into the active list.

                    ActiveAudioInstances.Add(audioInstance);
                    return FMOD.RESULT.OK;
                }
                else if (type == EVENT_CALLBACK_TYPE.STOPPED)
                {

                    // remove the audio instance that equality matches to the stopped instance
                    // from the active instance list.

                    ActiveAudioInstances.Remove(audioInstance);

                    // add the audio instace back to the free list for reuse.

                    FreePooledAudioInstances[audioInstance.Name].Add(audioInstance);
                    return FMOD.RESULT.OK;
                }
                else
                {
                    return FMOD.RESULT.ERR_INVALID_PARAM;
                }
            };

            // bind the callback to the audio instance.

            audioInstance.EventInstance.setCallback(callback, EVENT_CALLBACK_TYPE.STOPPED | EVENT_CALLBACK_TYPE.STARTED);

            // cache the callback to stop it from being garbage collected.

            PooledCallbacks.Add(callback);
        }

        /// <summary>
        /// Stops and deallocates all sound events, clearing all internal sound lifetime chaches.
        /// </summary>

        public void Clear()
        {

            for (int i = 0; i < ActiveAudioInstances.Count; i++)
            {
                // release all active pooled audio instances.
                ActiveAudioInstances[i].EventInstance.setCallback(null); // remove the callbacks.
                ActiveAudioInstances[i].EventInstance.stop(STOP_MODE.IMMEDIATE);
                ActiveAudioInstances[i].EventInstance.release();
            }

            // clear the list of cached audio instances.

            ActiveAudioInstances.Clear();

            // iterate through each free pooled list and release each audio instance, deallocating them. 

            foreach (SwapbackList<AudioInstance> audioInstances in FreePooledAudioInstances.Values)
            {
                for (int i = 0; i < audioInstances.Count; i++)
                {
                    audioInstances[i].EventInstance.setCallback(null); // remove the callbacks.
                    audioInstances[i].EventInstance.stop(STOP_MODE.IMMEDIATE);    
                    audioInstances[i].EventInstance.release();
                }
            }

            // clear the cached free lists.

            FreePooledAudioInstances.Clear();

            OneShotCallbacks.Clear();
            PooledCallbacks.Clear();
        }
    }

}

