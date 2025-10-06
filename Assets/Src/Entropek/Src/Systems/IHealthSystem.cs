using System;
using UnityEngine;

namespace Entropek.Systems{

[Serializable]
public abstract class HealthSystem : MonoBehaviour{
    
    public event Action<float> Healed;
    public event Action<float> HealthDamaged;
    public event Action Death;
    public event Action HealthFull;

    public abstract float GetHealthValue();
    public abstract float GetMaxHealthValue();
    public abstract float GetNormalisedHealthValue();

    public abstract bool Damage(float amount);
    public abstract void Heal(float amount);

    [Header(nameof(HealthSystem))]
    public bool Vulnerable = true; 

    protected void InvokeDeath(){
        Death?.Invoke();
    }
    
    protected void InvokeHealthDamaged(float amount){
        HealthDamaged?.Invoke(amount);
    }

    protected void InvokeHealthFull(){
        HealthFull?.Invoke();
    }

    protected void InvokeHealed(float amount){
        Healed?.Invoke(amount);
    }

}


}

