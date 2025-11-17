using System;
using Entropek.Combat;
using Entropek.Time;
using Entropek.UnityUtils.AnimatorUtils;
using UnityEngine;

public class AttackSkill : Skill, IAnimatedSkill
{


    /// 
    /// Constants.
    /// 


    public const string IdleAnimation = "HoldingIdle";
    public const string AttackFrameEventName = "SwordAttackFrame";
    private const string AnimationName = "SwordOutwardSlash";
    private const string AnimationCompletedEventName = "ExitSwordAttackState";
    private const string AttackFrameSound = "MeleeSwing";
    private const string HitSound = "MeleeHit";
    private const float LungeForce = 4.44f;
    private const float LungeDecaySpeed = LungeForce * 3f;
    private const float HitCameraShakeForce = 4f;
    private const float HitCameraShakeTime = 0.167f;
    private const float HitLensDistortionIntensity = 0.24f;
    private const float HitLensDistortionDuration = 0.16f;
    private const float HitMotionBlurDuration = 0.33f;
    private const float HitMotionBlurIntensity = 1f;
    private const int HealthRestorationAmount = 5;
    private const int AnimationLayer = 1;
    private const int LeftSlashVfxId = 0;
    private const int RightSlashVfxId = 1;
    private const int HitVfxId = 2;


    /// 
    /// Components
    /// 


    [Header("Components")]
    [SerializeField] private TimedSingleHitbox slashLeftHitBox;
    [SerializeField] private TimedSingleHitbox slashRightHitBox;
    [SerializeField] private OneShotTimer onHitGravityDisableTimer;



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

    int IAnimatedSkill.AnimationLayer => AnimationLayer;

    bool IAnimatedSkill.AllowsAnimationCancel => true;
    bool IAnimatedSkill.CausesAnimationCancel => false;

    Skill IAnimatedSkill.Skill => this;

    Coroutine animationLayerWeightTransitionCoroutine;
    Coroutine IAnimatedSkill.AnimationLayerWeightTransitionCoroutine 
    { 
        get => animationLayerWeightTransitionCoroutine; 
        set => animationLayerWeightTransitionCoroutine = value; 
    }

    public bool AnimationCancel => false;


    /// 
    /// Cached Interface Types.
    /// 


    IAnimatedSkill IAnimatedSkill;


    /// 
    /// Base.
    /// 

    protected override void GetInterfaceTypes()
    {
        IAnimatedSkill = this;
    }

    public override bool CanUse()
    {
        return IAnimatedSkill.CanUseAnimatedSkill();
    }

    protected override void UseInternal()
    {
        // swap slashes for next time.
        inUse = true;

        slashFlag = !slashFlag;

        // face in the attack direction.

        Player.FaceAttackDirection();
        
        IAnimatedSkill.StartAnimationLayerWeightTransition(IAnimatedSkill.MaxAnimationLayerWeight, 100);
        IAnimatedSkill.PlayAnimation();

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
        IAnimatedSkill.StartAnimationLayerWeightTransition(IAnimatedSkill.MinAnimationLayerWeight, 100);
        inUse = false;
    }

    void IAnimatedSkill.Cancel()
    {
        IAnimatedSkill.StartAnimationLayerWeightTransition(IAnimatedSkill.MinAnimationLayerWeight, 100);
        
        // instantly swap back to idle on the attack animation layer;
        // so that the animation doesnt follow through when cancelled.
        
        Player.Animator.Play(IdleAnimation, AnimationLayer, 0);
        Player.Animator.Update(0);
        
        inUse = false;
    }

    /// 
    /// Functions.
    /// 


    private void AttackFrame()
    {
        Player.CharacterControllerMovement.ImpulseRelativeToGround(transform.forward, LungeForce, LungeDecaySpeed);
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
        onHitGravityDisableTimer.Timeout += OnHitGravityDiableTimeout;
    }

    protected override void UnlinkEvents()
    {
        UnlinkHitBoxEvents();
        IAnimatedSkill.UnlinkAnimatedSkillEvents();
        onHitGravityDisableTimer.Timeout -= OnHitGravityDiableTimeout;
    }

    private void OnHitGravityDiableTimeout()
    {
        Player.CharacterControllerMovement.UseGravity = true;
    }


    /// 
    /// Hitbox Linkage.
    /// 


    private void OnAttackHit(GameObject other, Vector3 hitPoint)
    {
        Player.CharacterControllerMovement.UseGravity = false;
        Player.CharacterControllerMovement.ClearGravityVelocity();
        onHitGravityDisableTimer.Begin();

        Player.Health.Heal(HealthRestorationAmount);
        Player.CameraController.StartShaking(HitCameraShakeForce, HitCameraShakeTime);

        Player.CameraPostProcessingController.PulseLensDistortionIntensity(
            HitLensDistortionDuration,
            HitLensDistortionIntensity
        );

        Player.CameraPostProcessingController.PulseMotionBlurIntensity(
            HitMotionBlurDuration,
            HitMotionBlurIntensity
        );
        
        Player.VfxPlayerSpawner.PlayVfx(HitVfxId, hitPoint, transform.forward);
        Player.AudioPlayer.PlaySound(HitSound, hitPoint);
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
