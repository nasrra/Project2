using UnityEngine;

namespace Entropek.Audio
{

    public enum AudioInstanceType : byte
    {
        Global,     // The audio instance can be heard from everywhere.
        Positional, // The audio instance is emitted from a set position in world space.
        Attached    // the audio instance is attached to a gameobject.
    }
    
    
}

