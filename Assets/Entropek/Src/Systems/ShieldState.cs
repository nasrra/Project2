using System;
using UnityEngine;

namespace Entropek.Systems{

[Serializable]
public enum ShieldState : byte{
    None,
    Full,       // shield value is equal to shield max value.
    Active,     // shield value is in between max value and zero.
    Depleted,   // shield value is zero.
    Broken      // shield value just hit zero.
}


}

