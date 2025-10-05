using System;
using UnityEngine;

namespace Entropek.Systems{


public class Health : MonoBehaviour{
    
    public event Action<float> Healed;
    public event Action<float> Damaged;
    public event Action Death;
    public event Action FullyHealed;

    [Header("Data")]
    [SerializeField] private float value;
    public float Value => value;
    [SerializeField] private float maxValue;
    public float MaxValue => maxValue;
    public float NormalisedValue => value/maxValue;

    public void Damage(float amount){
        value-=amount;
        if(value<=0){
            value=0;
            Death?.Invoke();
        }
        else{
            Damaged?.Invoke(amount);
        }
    }

    public void Heal(float amount){
        value+=amount;
        if(value>=maxValue){
            value=maxValue;
            FullyHealed?.Invoke();
        }
        else{
            Healed?.Invoke(amount);
        }
    }
}


}

