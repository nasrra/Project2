using System;
using UnityEngine;

namespace Entropek.Systems{


[Serializable]
public enum HealthState : byte{
    None,       // none.
    Alive,    // below full and above zero health.
    Full,       // at max health.
    Dead,       // at zero health.
}


}

