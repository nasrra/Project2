using System;
using UnityEngine;
using FMOD.Studio;
using UnityEngine.UIElements;

namespace Entropek.Audio
{

    /// <summary>
    /// This struct is a wrapper of the fmod event instance struct
    /// so that the name of the event can be cached.
    /// this helps instead of going through the lengthy process of locating the name by event description
    /// in the audio player class when stopping a sound.
    /// </summary>

    public struct AudioInstance
    {
        public FMOD.Studio.EventInstance EventInstance { get; private set; }
        public string Name { get; private set; }
        public AudioInstanceType Type { get; private set; }
        public Transform AttatchedTransform;


        public AudioInstance(FMOD.Studio.EventInstance eventInstance, string name, AudioInstanceType type, Transform attatchedTransform = null)
        {
            EventInstance = eventInstance;
            Name = name;
            Type = type;
            AttatchedTransform = attatchedTransform;
        }

        // override the audio instance to check if the event instances are the same.        
        
        public static bool operator ==(AudioInstance a, AudioInstance b)
        {
            return a.EventInstance.handle == b.EventInstance.handle;
        }

        public static bool operator !=(AudioInstance a, AudioInstance b)
        {
            return a.EventInstance.handle != b.EventInstance.handle;            
        }

        public override bool Equals(object obj)
        {
            if(obj is AudioInstance other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return EventInstance.handle.GetHashCode();
        }
    }

}

