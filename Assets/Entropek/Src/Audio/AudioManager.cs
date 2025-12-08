using System;
using System.Collections.Generic;
using System.IO;
using Entropek.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Entropek.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Singleton { get; private set; }

        private HashSet<string> availableBanks = new();

        private Dictionary<string, EventReference> loadedEventReferences = new();

        private Dictionary<BusHandle, Bus> audioBuses = new();


        private const string MasterBusName = "bus:/";
        private const string MusicBusName = "bus:/Music";
        private const string VoiceBusName = "bus:/Voice";
        private const string MenuBusName = "bus:/Menu";
        private const string SfxBusName = "bus:/Sfx";


        /// 
        /// Base.
        /// 


        private void OnDestroy()
        {
            UninitialiseFmod();
        }

        private void UninitialiseFmod()
        {
            // NOTE:
            //  here to avoid memory leak when quitting built versions of the game.

            // RuntimeManager.CoreSystem.mixerSuspend();
            // // stop all sounds
            RuntimeManager.StudioSystem.flushCommands();
            // unload all banks
            RuntimeManager.StudioSystem.unloadAll();
            // Release FMOD Studio System.
            RuntimeManager.StudioSystem.release();
        }


        ///
        /// Sound Instance Functions.
        /// 


        /// <summary>
        /// Plays a sound event at a given position in world space.
        /// </summary>
        /// <param name="eventName">The name of the loaded event reference (sound) to play.</param>
        /// <param name="position">The position in world space to play the sound at.</param>
        /// <param name="release">Whether or not to immediately release the event instance - upon ending - for garbage collection.</param>
        /// <returns>An audio instance of the playing sound.</returns>

        public AudioInstance PlayEvent(string eventName, Vector3 position, bool release = true)
        {
            EventInstance instance = RuntimeManager.CreateInstance(loadedEventReferences[eventName]);

            FMOD.ATTRIBUTES_3D attributes = new FMOD.ATTRIBUTES_3D
            {
                position    = AudioManager.Singleton.UnityToFmodVector(position),
                velocity    = new FMOD.VECTOR { x = 0, y = 0, z = 0 },
                forward     = new FMOD.VECTOR { x = 0, y = 0, z = 1},
                up          = new FMOD.VECTOR { x = 0, y = 1, z = 0}
            };


            instance.set3DAttributes(attributes);
            instance.start();

            if (release == true)
            {
                // Immediately release it, so when the sound has finished,
                // FMOD studio can garbage collect it.

                instance.release();
            }

            RuntimeManager.StudioSystem.update();

            return new AudioInstance(instance, eventName, AudioInstanceType.Positional);;
        }

        /// <summary>
        /// Plays a sound event at an attached gameobjects position in world space; the sound following the gameobjects position.
        /// </summary>
        /// <param name="eventName">The name of the loaded event reference (sound) to play.</param>
        /// <param name="attachedTransform">The transform to attach this sound event to.</param>
        /// <param name="release">Whether or not to immeditely release the event instanc - upon ending - for garbage collection.</param>
        /// <returns>An audio instance of the playing sound.</returns>

        public AudioInstance PlayEvent(string eventName, GameObject attachedGameObject, bool release = true)
        {
            EventInstance instance = RuntimeManager.CreateInstance(loadedEventReferences[eventName]);

            FMOD.ATTRIBUTES_3D attributes = new FMOD.ATTRIBUTES_3D
            {
                position    = AudioManager.Singleton.UnityToFmodVector(attachedGameObject.transform.position),
                velocity    = new FMOD.VECTOR { x = 0, y = 0, z = 0 },
                forward     = new FMOD.VECTOR { x = 0, y = 0, z = 1},
                up          = new FMOD.VECTOR { x = 0, y = 1, z = 0}
            };


            instance.set3DAttributes(attributes);

            RuntimeManager.AttachInstanceToGameObject(instance, attachedGameObject);

            instance.start();

            if (release == true)
            {
                // Immediately release it, so when the sound has finished,
                // FMOD studio can garbage collect it.

                instance.release();
            }

            return new AudioInstance(instance, eventName, AudioInstanceType.Attached);
        }

        /// <summary>
        /// Plays a global sound event - heard from anywhere in world space.
        /// </summary>
        /// <param name="eventName">THe name of the loaded reference (sound) to play.</param>
        /// <param name="release">Whether or not to immediately release the event instance - upon ending - for garbage collection.</param>
        /// <returns>An audio instance of the palying sound.</returns>

        public AudioInstance PlayEvent(string eventName, bool release = true)
        {
            
            EventInstance instance = RuntimeManager.CreateInstance(loadedEventReferences[eventName]);
            instance.start();

            if (release == true)
            {
                // Immediately release it, so when the sound has finished,
                // FMOD studio can garbage collect it.


                instance.release();
            }

            return new AudioInstance(instance, eventName, AudioInstanceType.Global);
        }


        ///
        /// Bus Management.
        /// 


        /// <summary>
        /// Retrieves all buses from the FMOD Project and internally stores a refence to them.
        /// </summary>

        private void RetrieveBuses()
        {
            audioBuses.Add(BusHandle.Master, RuntimeManager.GetBus(MasterBusName));
            audioBuses.Add(BusHandle.Music, RuntimeManager.GetBus(MusicBusName));
            audioBuses.Add(BusHandle.Voice, RuntimeManager.GetBus(VoiceBusName));
            audioBuses.Add(BusHandle.Menu, RuntimeManager.GetBus(MenuBusName));
            audioBuses.Add(BusHandle.Sfx, RuntimeManager.GetBus(SfxBusName));
        }

        /// <summary>
        /// Gets the current volume of an audio bus.
        /// </summary>
        /// <param name="busHandle">The specified audio bus id</param>
        /// <returns></returns>

        public float GetBusVolume(BusHandle busHandle)
        {
            audioBuses[busHandle].getVolume(out float volume);
            return volume;
        }

        /// <summary>
        /// Sets the current volume of an audio bus.
        /// </summary>
        /// <param name="busHandle">The specified audio bus id.</param>
        /// <param name="volume">The desired volume.</param>

        public void SetBusVolume(BusHandle busHandle, float volume)
        {
            audioBuses[busHandle].setVolume(volume);
        }

        /// <summary>
        /// Mutes an audio bus from playing any sound.
        /// </summary>
        /// <param name="busHandle">The specified audio bus id.</param>

        public void MuteBus(BusHandle busHandle)
        {
            audioBuses[busHandle].setMute(true);
        }

        /// <summary>
        /// Unmutes an audio bus, allowing sound to be played.
        /// </summary>
        /// <param name="busHandle">The specified audio bus id.</param>

        public void UnmuteBus(BusHandle busHandle)
        {
            audioBuses[busHandle].setMute(false);
        }

        /// <summary>
        /// Gets whether an audio bus is muted.
        /// </summary>
        /// <param name="busHandle">The specified audio bus id.</param>
        /// <returns>true, if muted and false if unmuted.</returns>

        public bool IsBusMuted(BusHandle busHandle)
        {
            audioBuses[busHandle].getMute(out bool muted);
            return muted;
        }


        ///
        /// Bank management.
        /// 


        /// <summary>
        /// Retrieves and caches the file names of available built FMOD banks in the streaming assets folder.
        /// </summary>

        private void GetAvailableBanks()
        {
            string[] bankFiles = Directory.GetFiles(Application.streamingAssetsPath + "/FMOD Banks/Desktop/", "*.bank");
            foreach (string filePath in bankFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                availableBanks.Add(fileName);
                // UnityEngine.Debug.Log("found bank: " + fileName);
            }
        }

        /// <summary>
        /// Load a built FMOD bank and its event references from the Streaming Assets Fmod Banks folder.
        /// </summary>
        /// <param name="bankName"></param>
        /// <returns>true, if the bank was successfully loaded.</returns>

        public bool LoadBank(string bankName)
        {
            if (availableBanks.Contains(bankName) == false)
            {
                return false;
            }

            if (RuntimeManager.HasBankLoaded(bankName))
            {
                UnityEngine.Debug.Log("Bank " + bankName + " has already been loaded!");
                return false;
            }

            RuntimeManager.LoadBank(bankName);
            Bank bank;
            FMOD.RESULT result = RuntimeManager.StudioSystem.getBank("bank:/" + bankName, out bank);

            if (result != FMOD.RESULT.OK)
            {
                UnityEngine.Debug.LogError($"Failed to load {bankName} bank: {result}");
                return false;
            }

            LoadBankEvents(bank);

            return true;
        }

        /// <summary>
        /// Unload a built FMOD bank and its event references.
        /// </summary>
        /// <param name="bankName"></param>
        /// <returns>true, if the bank was unloaded successfully.</returns>

        public bool UnloadBank(string bankName)
        {
            if (availableBanks.Contains(bankName) == false)
            {
                return false;
            }

            if (RuntimeManager.HasBankLoaded(bankName) == false)
            {
                UnityEngine.Debug.LogError("Bank " + bankName + " cannot be unloaded as it has not been loaded!");
                return false;
            }

            Bank bank;
            RuntimeManager.StudioSystem.getBank("bank:/" + bankName, out bank);
            UnloadBankEvents(bank);
            RuntimeManager.UnloadBank(bankName);

            return true;
        }

        /// <summary>
        /// Loads all event references from a built FMOD bank and caches it by event name.
        /// </summary>
        /// <param name="bank"></param>

        private void LoadBankEvents(Bank bank)
        {
            bank.getEventList(out EventDescription[] descriptions);

            for (int i = 0; i < descriptions.Length; i++)
            {

                // get the events parameters within the fmod studio project.

                ref EventDescription description = ref descriptions[i];

                // trim the event file path in the fmod studio project to just its event/file name
                // (Not in the internal file system of FMOD studio but the event description file system.)

                description.getPath(out string path);
                EventReference eventReference = RuntimeManager.PathToEventReference(path);
                path = Path.GetFileNameWithoutExtension(path);

                // cache a reference to the event reference by event name.

                loadedEventReferences.Add(path, eventReference);
            }

        }

        /// <summary>
        /// Unloads all loaded event references in a given FMOD bank.
        /// </summary>
        /// <param name="bank"></param>

        private void UnloadBankEvents(Bank bank)
        {
            bank.getEventList(out EventDescription[] descriptions);

            for (int i = 0; i < descriptions.Length; i++)
            {

                // get the events parameters within the fmod studio project.

                ref EventDescription description = ref descriptions[i];

                // trim the event file path in the fmod studio project to just its event/file name
                // (Not in the internal file system of FMOD studio but the event description file system.)

                description.getPath(out string path);
                path = Path.GetFileNameWithoutExtension(path);

                // remove the cached reference.

                loadedEventReferences.Remove(path);
            }

        }


        ///
        /// Utility Functions.
        /// 


        /// <summary>
        /// Converts a Unity vector into an FMOD vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>

        public FMOD.VECTOR UnityToFmodVector(Vector3 vector)
        {
            return new FMOD.VECTOR
            {
                x = vector.x,
                y = vector.y,
                z = vector.z
            };
        }

        /// <summary>
        /// Converts a Unity vector into an FMOD vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>

        private FMOD.VECTOR UnityToFmodVector(Vector2 vector)
        {
            return new FMOD.VECTOR
            {
                x = vector.x,
                y = vector.y,
                z = 0
            };
        }


        /// 
        /// BootStrap
        /// 


        internal static class Bootstrap
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            internal static void Initialise()
            {

                // Singleton Object creation.

                GameObject main = new()
                {
                    name = "Audio Manager"
                };

                main.AddComponent<AudioManager>();
                DontDestroyOnLoad(main);
                Singleton = main.GetComponent<AudioManager>();

                Singleton.GetAvailableBanks();
                RuntimeManager.LoadBank("Master");
                RuntimeManager.LoadBank("Master.strings");
                Singleton.LoadBank("Globals");

                Singleton.RetrieveBuses();

            }
        }
    }
    
}

