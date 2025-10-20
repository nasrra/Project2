using System;

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

        public AudioInstance(FMOD.Studio.EventInstance eventInstance, string name, AudioInstanceType type)
        {
            EventInstance = eventInstance;
            Name = name;
            Type = type;
        }
    }

}

