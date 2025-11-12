using Entropek.Combat;
using Entropek.EntityStats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour{
    
    [Header(nameof(HealthBar)+" Components")]
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI text;
    private Health health;


    /// 
    /// Base.
    /// 


    private void OnEnable(){
        if(health!=null){
            LinkEvents();
        }
    }

    private void OnDisable(){
        if(health!=null){
            UnlinkEvents();        
        }
    }


    /// 
    /// Functions.
    /// 


    public virtual void Activate(Health health){

        // unlink from the previous health if there was any.

        if(this.health!=null){
            UnlinkHealth();
        }

        this.health     = health;
        slider.maxValue = health.MaxValue;
        slider.value    = health.Value;

        // link to this new one.

        LinkHealth();

        gameObject.SetActive(true);
    }

    public virtual void Deactivate()
    {
        if (this.health != null)
        {
            UnlinkHealth();
        }

        gameObject.SetActive(false);                
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        LinkHealth();
    }

    private void UnlinkEvents(){
        UnlinkHealth();
    }

    private void LinkHealth(){
        health.Damaged += OnDamaged;
        health.Healed += OnHealed;
        health.Death += OnDeath;
        health.MaxValueSet += OnMaxValueSet;
    }

    private void UnlinkHealth(){
        health.Damaged -= OnDamaged;
        health.Healed -= OnHealed;
        health.Death -= OnDeath;
        health.MaxValueSet -= OnMaxValueSet;
    }

    private void OnHealed(int amount){
        slider.value = health.Value;
        UpdateHealthText();
    }

    private void OnDamaged(DamageContext damageContext){
        slider.value = health.Value;
        UpdateHealthText();
    }

    private void OnDeath()
    {
        UpdateHealthText();
        Deactivate();
    }

    private void OnMaxValueSet(int value)
    {
        slider.maxValue = health.MaxValue;
        slider.value = health.Value;

        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        text.text = $"{health.Value} / {health.MaxValue}";
    }
}

