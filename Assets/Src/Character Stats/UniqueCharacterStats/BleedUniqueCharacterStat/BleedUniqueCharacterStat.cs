using Entropek.Combat;
using Entropek.EntityStats;
using Entropek.UnityUtils.Attributes;
using Unity.VisualScripting;
using UnityEngine;

public class BleedUniqueCharacterStat : UniqueCharacterStat
{
    [SerializeField] CharacterStatFloat bleedStackChance = new();
    [RuntimeField] private HitboxManager hitboxManager;

    private void Start()
    {
        LinkEvents();
    }

    private void OnDestroy()
    {
        UnlinkEvents();
    }

    public override void AddModifier(int amount)
    {
        bleedStackChance.AddLinearModifier(amount);
    }

    public override void Initialise(PlayerStats playerStats)
    {   
        hitboxManager = playerStats.gameObject.GetComponent<HitboxManager>();
    }

    public override void RemoveModifier(int amount)
    {
        bleedStackChance.RemoveLinearModifier(amount);        
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents()
    {
        LinkHitboxEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkHitboxEvents();
    }

    private void LinkHitboxEvents()
    {
        hitboxManager.HealthHit += OnHealthHit;
    }

    private void UnlinkHitboxEvents()
    {
        hitboxManager.HealthHit -= OnHealthHit;
    }

    private void OnHealthHit(HitboxHitContext context)
    {
        if(context.HitGameObject.TryGetComponent(out Health health))
        {
            health.ApplyBleedStacks(1);
            Debug.Log("Apply Bleed");
        }
    }
}
