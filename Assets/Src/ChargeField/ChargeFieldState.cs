using UnityEngine;

public enum ChargeFieldState : byte
{
    Depleted,   // default as depleted.
    Charging,   // when a charger object has entered the field.
    Standy,     // when a charger has no chargers inside the field but has been charged. 
    Charged,    // the field is fully charged.
}
