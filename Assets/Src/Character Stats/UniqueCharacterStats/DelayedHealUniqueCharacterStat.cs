using Entropek.Combat;
using Entropek.EntityStats;
using Entropek.Time;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

public class DelayedHealUniqueCharacterStat : UniqueCharacterStat
{

    [Header(nameof(DelayedHealUniqueCharacterStat)+" Components")]
    [SerializeField] private OneShotTimer timer;
    [Tooltip("The value of the stat correlates to a percentage health value. E.g 0.1 = 10% of MaxHealth")]
    [SerializeField] private CharacterStatFloat stat;
    [RuntimeField] private Health health;


    /// 
    /// Base.
    /// 

    private void Start()
    {
        LinkEvents();
    }

    private void OnDestroy()
    {
        UnlinkEvents();
    }

    public override void Initialise(PlayerStats playerStats)
    {
        health = playerStats.GetComponent<Health>();
    }

    public override void AddModifier(int amount)
    {
        stat.AddLinearModifier(amount);
    }

    public override void RemoveModifier(int amount)
    {
        stat.RemoveLinearModifier(amount);
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents()
    {
        LinkTimerEvents();
        LinkHealthEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkTimerEvents();
        UnlinkHealthEvents();
    }


    /// 
    /// Timer Event Linkage.
    /// 


    private void LinkTimerEvents()
    {
        timer.Timeout += OnTimerTimeout;
    }

    private void UnlinkTimerEvents()
    {
        timer.Timeout -= OnTimerTimeout;
    }

    private void OnTimerTimeout()
    {
        Debug.Log(Mathf.CeilToInt(health.MaxValue * stat.ScaledValue));
        health.Heal(Mathf.CeilToInt(health.MaxValue * stat.ScaledValue));
    }


    ///
    /// Health Event Linkage.
    /// 


    private void LinkHealthEvents()
    {
        health.Damaged += OnDamaged;
        health.Death += OnDeath;
    }

    private void UnlinkHealthEvents()
    {
        health.Damaged -= OnDamaged;        
        health.Death -= OnDeath;
    }

    private void OnDamaged(DamageContext damageContext)
    {
        timer.Begin();
    }

    private void OnDeath()
    {
        timer.Halt();
    }
}
