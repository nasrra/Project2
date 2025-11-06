using System;
using UnityEngine;

namespace Entropek.Combat
{
    [Flags]
    public enum DamageType : byte
    {
        None,
        Light,
        Heavy
    }    
}
