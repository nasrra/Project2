using System;
using Entropek.Collections;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

[Serializable]
public class CharacterStatInt
{
    public event Action<int> ScaledValueCalculated;

    [SerializeField] private int baseValue;
    public int BaseValue => baseValue;

    private int scaledValue;
    public int ScaledValue
    {
        get
        {
            if(isDirty == true)
            {
                CalculateScaledValue();
            }            
            return scaledValue;
        }
        set
        {
            scaledValue = value;
        }
    }

    [SerializeField, RuntimeField] SwapbackList<float> flatModifiers = new();
    [SerializeField, RuntimeField] SwapbackList<float> linearModifiers = new();
    [SerializeField, RuntimeField] SwapbackList<float> hyperbolicModifiers = new();

    // NOTE:
    //  Is dirty checks are needed as unity cant guarantee 
    //  reliably intialising scaled value properly through constructors.

    private bool isDirty = true;

    public void AddFlatModifier(float value)
    {
        flatModifiers.Add(value);
        isDirty = true;
    }

    public void RemoveFlatModifier(float value)
    {
        flatModifiers.Remove(value);
        isDirty = true;
    }

    public void AddLinearModifier(float value)
    {
        linearModifiers.Add(value);
        isDirty = true;
    }

    public void RemoveLinearModifier(float value)
    {
        linearModifiers.Remove(value);
        isDirty = true;
    }

    public void AddHyperbolicModifier(float value)
    {
        hyperbolicModifiers.Add(value);
        isDirty = true;
    }

    public void RemoveHyperbolicModifier(float value)
    {
        hyperbolicModifiers.Remove(value);
        isDirty = true;
    }

    public void CalculateScaledValue()
    {
        isDirty = false;
        scaledValue = Mathf.RoundToInt(CalculateFlatModifierValue() + CalculateLinearModifierValue() + CalculateHyperbolicModifier());
        ScaledValueCalculated?.Invoke(scaledValue);
    }

    private float CalculateFlatModifierValue()
    {
        float value = 0;
        
        for(int i = 0; i < flatModifiers.Count; i++)
        {
            value += flatModifiers[i];
        }

        return value;   
    }

    private float CalculateLinearModifierValue()
    {
        float linearModifierTotal = 1;

        for(int i = 0; i < linearModifiers.Count; i++)
        {
            linearModifierTotal += linearModifiers[i];
        }

        return baseValue * linearModifierTotal;
    }

    private float CalculateHyperbolicModifier()
    {
        float value = 0;

        float hyperSum = 0f;
        for (int i = 0; i < hyperbolicModifiers.Count; i++)
        {
            hyperSum += hyperbolicModifiers[i];
        }

        if (hyperSum != 0f)
        {
            value += (baseValue * hyperSum) / (1f + baseValue * hyperSum);
        }

        return value;
    }
}
