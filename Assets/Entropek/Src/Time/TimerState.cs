using UnityEngine;

namespace Entropek.Time
{

    public enum TimerState : byte
    {
        Halted, // default state for any timer is halted.
        Paused,
        Active,
    }

}

