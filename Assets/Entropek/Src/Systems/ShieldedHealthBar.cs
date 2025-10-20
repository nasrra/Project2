using UnityEngine;
using UnityEngine.UI;

namespace Entropek.Systems{


public class ShieldedHealthBar : MonoBehaviour{


    [Header("Components")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider shieldSlider;
    private ShieldedHealth health;


    ///
    /// Base.
    /// 


    private void OnEnable(){
        LinkEvents();
    }

    private void OnDisable(){
        UnlinkEvents();
    }


    ///
    /// Functions.
    /// 


    public void DisplayShieldedHealth(ShieldedHealth shieldedHealth){
        
        // unlink from previous health if there was any.

        if(health!=null){
            UnlinkHealth();
        }

        health = shieldedHealth;
        healthSlider.maxValue   = shieldedHealth.GetMaxHealthValue();
        shieldSlider.maxValue   = shieldedHealth.GetMaxShieldValue();
        healthSlider.value      = shieldedHealth.GetHealthValue();
        shieldSlider.value      = shieldedHealth.GetShieldValue();

        // link to this new one.

        LinkHealth(); 
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        if(health != null){
            LinkHealth();
        }
    }

    private void UnlinkEvents(){
        if(health != null){
            UnlinkHealth();
        }
    }

    private void LinkHealth(){
        health.HealthDamaged        += OnHealthDamaged; 
        health.Healed               += OnHealthHealed;
        health.ShieldDamaged        += OnShieldDamaged;
        health.ShieldRestored       += OnShieldRestored;
        health.ShieldBroken         += OnShieldBroken;
        health.ShieldFull           += OnShieldFull;
        health.HealthFull           += OnHealthFull;
        health.Death                += OnDeath;
    }

    private void UnlinkHealth(){
        health.HealthDamaged        -= OnHealthDamaged; 
        health.Healed               -= OnHealthHealed;
        health.ShieldDamaged        -= OnShieldDamaged;
        health.ShieldRestored       -= OnShieldRestored;
        health.ShieldBroken         -= OnShieldBroken;
        health.ShieldFull           -= OnShieldFull;
        health.HealthFull           -= OnHealthFull;
        health.Death                -= OnDeath;
    }

    private void OnHealthDamaged(float amount){
        healthSlider.value = health.GetHealthValue();
    }

    private void OnHealthHealed(float amount){
        shieldSlider.value = health.GetHealthValue();
    }

    private void OnShieldDamaged(float amount){
        shieldSlider.value = health.GetShieldValue();
    }

    private void OnShieldRestored(float amount){
        shieldSlider.value = health.GetShieldValue();
    }

    private void OnShieldBroken(){
        shieldSlider.value = 0;
    }

    private void OnShieldFull(){
        shieldSlider.value = shieldSlider.maxValue;
    }

    private void OnHealthFull(){
        healthSlider.value = healthSlider.maxValue;
    }

    private void OnDeath(){
        shieldSlider.value = 0;
        healthSlider.value = 0;
    }

}


}

