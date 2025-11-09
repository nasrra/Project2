using System;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

public interface IBatchRechargeSkill
{
    Action<int> ChargesDepleted{get;set;}
    Action<int> ChargesRestored{get;set;}
    Action ChargesFullyDepleted{get;set;}
    Action ChargesFullyRestored{get;set;}
    [SerializeField] int MaxCharges{get;}
    [RuntimeField] int Charges{get;protected set;}

    public void RestoreCharges(int amount)
    {
        Charges += amount;
        if(Charges >= MaxCharges)
        {
            Charges = MaxCharges;
            OnChargesFullyRestored();
            ChargesFullyRestored?.Invoke();
        }
        else
        {
            ChargesRestored?.Invoke(amount);
        }
    }

    public void DepleteCharges(int amount)
    {
        Charges -= amount;
        if(Charges <= 0)
        {
            Charges = 0;
            OnChargesFullyDepleted();
            ChargesFullyDepleted?.Invoke();
        }
        else
        {
            ChargesDepleted?.Invoke(amount);
        }

    }

    void OnChargesFullyDepleted();
    void OnChargesFullyRestored();
}
