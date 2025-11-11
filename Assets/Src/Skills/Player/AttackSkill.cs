using System;
using Entropek.UnityUtils.AnimatorUtils;
using UnityEngine;

public class AttackSkill : Skill, IAnimatedSkill
{


    /// 
    /// Constants.
    /// 


    public const string AttackFrameEventName = "SwordAttackFrame";
    private const string AnimationName = "Rig_Sword_Attack";
    private const string AnimationCompletedEventName = "ExitSwordAttackState";
    private const string AttackFrameSound = "MeleeSwing";
    private const string AttackHitSound = "MeleeHit";
    private const float AttackLungeForce = 4.44f;
    private const float AttackLungeDecaySpeed = AttackLungeForce * 3f;
    private const float AttackHitCameraShakeForce = 4f;
    private const float AttackHitCameraShakeTime = 0.167f;
    private const float AttackHitLensDistortionIntensity = 0.24f;
    private const float AttackHitLensDistortionDuration = 0.16f;
    private const float AttackHitMotionBlurDuration = 0.33f;
    private const float AttackHitMotionBlurIntensity = 1f;
    private const int AttackShieldRestorationAmount = 5;
    private const int LeftSlashVfxId = 0;
    private const int RightSlashVfxId = 1;
    private const int AttackHitVfxId = 2;


    /// 
    /// Components
    /// 


    [Header("Components")]
    [SerializeField] private Entropek.Combat.TimedSingleHitbox slashLeftHitBox;
    [SerializeField] private Entropek.Combat.TimedSingleHitbox slashRightHitBox;


    /// 
    /// Data.
    /// 

    
    [Header("Data")]
    private bool slashFlag = false;


    /// 
    /// IAnimatedSkill Field Overrides.
    /// 


    Player IAnimatedSkill.Player => Player;

    Animator IAnimatedSkill.Animator => Player.Animator; 
    
    AnimationEventReciever IAnimatedSkill.AnimationEventReciever => Player.AnimationEventReciever;
        
    string IAnimatedSkill.AnimationName => AnimationName; 
    
    string IAnimatedSkill.AnimationCompletedEventName => AnimationCompletedEventName;
    
    event Action animationCompleted;
    Action IAnimatedSkill.AnimationCompleted 
    { 
        get => animationCompleted; 
        set => animationCompleted = value; 
    }

    int IAnimatedSkill.AnimationLayer => 1;

    Skill IAnimatedSkill.Skill => this;

    Coroutine animationLayerWeightTransitionCoroutine;
    Coroutine IAnimatedSkill.AnimationLayerWeightTransitionCoroutine 
    { 
        get => animationLayerWeightTransitionCoroutine; 
        set => animationLayerWeightTransitionCoroutine = value; 
    }


    /// 
    /// Cached Interface Types.
    /// 


    IAnimatedSkill IAnimatedSkill;


    /// 
    /// Base.
    /// 


    public override bool Use()
    {
        // dont execute if an animated skill is already being used.

        if (Player.SkillCollection.AnimatedSkillIsInUse())
        {
            return false;
        }

        // swap slashes for next time.

        slashFlag = !slashFlag;

        // face in the attack direction.

        Player.FaceAttackDirection();
        
        IAnimatedSkill.StarAnimationLayerWeightTransition(IAnimatedSkill.MaxAnimationLayerWeight, 100);
        IAnimatedSkill.PlayAnimation();

        inUse = true;

        return true;
    }

    protected override void GetInterfaceTypes()
    {
        IAnimatedSkill = this;
    }

    void IAnimatedSkill.OnAnimationEventTriggered(string eventName)
    {
        switch (eventName)
        {            
            case AttackFrameEventName: AttackFrame(); break;
        }
    }

    void IAnimatedSkill.OnAnimationCompleted()
    {
        IAnimatedSkill.StarAnimationLayerWeightTransition(IAnimatedSkill.MinAnimationLayerWeight, 100);
        inUse = false;
    }

    /// 
    /// Functions.
    /// 


    private void AttackFrame()
    {
        Player.ForceApplier.ImpulseRelativeToGround(transform.forward, AttackLungeForce, AttackLungeDecaySpeed);
        Player.AudioPlayer.PlaySound(AttackFrameSound, Player.transform.position);
        if (slashFlag == true)
        {
            slashRightHitBox.Activate();
            Player.VfxPlayerSpawner.PlayVfx(LeftSlashVfxId, transform.position + (transform.forward * 1.1f), transform.forward);
        }
        else
        {
            slashLeftHitBox.Activate();
            Player.VfxPlayerSpawner.PlayVfx(RightSlashVfxId, transform.position + (transform.forward * 1.1f), transform.forward);
        }
    }



    ///
    /// Linkage.
    /// 


    protected override void LinkEvents()
    {
        LinkHitBoxEvents();
        IAnimatedSkill.LinkAnimatedSkillEvents();
    }

    protected override void UnlinkEvents()
    {
        UnlinkHitBoxEvents();
        IAnimatedSkill.UnlinkAnimatedSkillEvents();
    }


    /// 
    /// Hitbox Linkage.
    /// 


    private void OnAttackHit(GameObject other, Vector3 hitPoint)
    {
        Player.VfxPlayerSpawner.PlayVfx(AttackHitVfxId, hitPoint, transform.forward);
        Player.Health.Heal(AttackShieldRestorationAmount);
        Player.CameraController.StartShaking(AttackHitCameraShakeForce, AttackHitCameraShakeTime);

        Player.CameraPostProcessingController.PulseLensDistortionIntensity(
            AttackHitLensDistortionDuration,
            AttackHitLensDistortionIntensity
        );

        Player.CameraPostProcessingController.PulseMotionBlurIntensity(
            AttackHitMotionBlurDuration,
            AttackHitMotionBlurIntensity
        );
        
        Player.AudioPlayer.PlaySound(AttackHitSound, hitPoint);
    }

    private void LinkHitBoxEvents()
    {
        slashLeftHitBox.HitHealth += OnAttackHit;
        slashRightHitBox.HitHealth += OnAttackHit;
    }

    private void UnlinkHitBoxEvents()
    {
        slashLeftHitBox.HitHealth -= OnAttackHit;
        slashRightHitBox.HitHealth -= OnAttackHit;
    }

}
