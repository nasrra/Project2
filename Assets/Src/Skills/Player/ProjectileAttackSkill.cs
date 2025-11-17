using System;
using Entropek.Projectiles;
using Entropek.Time;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

public class ProjectileAttackSkill : Skill, ICooldownSkill, IBatchRechargeSkill
{


    ///
    /// Constants. 
    /// 

    
    private const int ProjectilePrefabId = 0;
    private const int ProjectileSpawnPointId = 0;


    /// 
    /// Components.
    /// 


    [Header("Components")]
    [SerializeField] ProjectileSpawner projectileSpawner;


    /// 
    /// ICooldownSkill Field Overrides.
    /// 


    [SerializeField] OneShotTimer cooldownTimer;
    OneShotTimer ICooldownSkill.CooldownTimer => cooldownTimer;
    

    /// 
    /// IBatchRechargeSkill Field Overrides.
    /// 


    private Action<int> ChargesDepleted;
    Action<int> IBatchRechargeSkill.ChargesDepleted 
    { 
        get => ChargesDepleted; 
        set => ChargesDepleted = value; 
    }

    private Action<int> ChargesRestored;
    Action<int> IBatchRechargeSkill.ChargesRestored 
    { 
        get => ChargesRestored; 
        set => ChargesRestored = value; 
    }
    
    private Action ChargesFullyDepleted;
    Action IBatchRechargeSkill.ChargesFullyDepleted 
    { 
        get => ChargesFullyDepleted; 
        set => ChargesFullyDepleted = value; 
    }
    
    private Action ChargesFullyRestored;
    Action IBatchRechargeSkill.ChargesFullyRestored 
    { 
        get => ChargesFullyRestored; 
        set => ChargesFullyRestored = value; 
    }

    [SerializeField] private int maxCharges;
    int IBatchRechargeSkill.MaxCharges => maxCharges;

    [RuntimeField] private int charges = 0;
    int IBatchRechargeSkill.Charges 
    { 
        get => charges; 
        set => charges = value; 
    }


    ///
    /// Chached Interfaces.
    /// 


    ICooldownSkill ICooldownSkill;
    IBatchRechargeSkill IBatchRechargeSkill;


    /// 
    /// Base.
    /// 


    public override bool CanUse()
    {
        return ICooldownSkill.CanUseCooldownSkill();
    }

    protected override void UseInternal()
    {
        // restore all charges to fire all 

        IBatchRechargeSkill.RestoreCharges(maxCharges);
    }

    protected override void GetInterfaceTypes()
    {
        ICooldownSkill = this;
        IBatchRechargeSkill = this;
    }


    /// 
    /// Linkage.
    /// 


    protected override void LinkEvents()
    {
        ICooldownSkill.LinkCooldownSkillEvents();
        Player.AnimationEventReciever.AnimationEventTriggered += OnAnimationEventTriggered;
    }

    protected override void UnlinkEvents()
    {
        ICooldownSkill.UnlinkCooldownSkillEvents();
        Player.AnimationEventReciever.AnimationEventTriggered -= OnAnimationEventTriggered;
    }

    protected void OnAnimationEventTriggered(string eventName)
    {
        // short circuit if there are no charges.

        if(charges == 0)
        {
            return;
        }


        // fire a projectile only when the player attacks.

        switch (eventName)
        {
            case AttackSkill.AttackFrameEventName:
                projectileSpawner.FireAtPosition(
                    (maxCharges - charges) % 3, 
                    ProjectileSpawnPointId, 
                    Player.CameraAimTarget.transform.position);

                IBatchRechargeSkill.DepleteCharges(1);
            break;
        }
    }


    /// 
    /// ICooldownSkill Function Overrides.
    /// 


    void ICooldownSkill.OnCooldownTimeout()
    {
        // do nothing.
    }


    /// 
    /// IBatchRechargeSkill Function Overrides.
    /// 


    void IBatchRechargeSkill.OnChargesFullyDepleted()
    {
        cooldownTimer.Begin();
    }

    void IBatchRechargeSkill.OnChargesFullyRestored()
    {
        // do nothing.
    }
}
