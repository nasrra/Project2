using System;
using Entropek.Combat;
using Entropek.Systems.Trails;
using Entropek.Time;
using Entropek.UnityUtils.AnimatorUtils;
using Entropek.UnityUtils.Attributes;
using UnityEditor.EditorTools;
using UnityEngine;

public class DashSkill : Skill, IAnimatedSkill, IMovementSkill, ITimedStateSkill, ICooldownSkill
{


    ///
    /// Constants.
    /// 


    private const string DashSound = "Dodge";
    private const string AnimationName = "AirDash";
    private const string AnimationCompletedEventName = "ExitDashState";
    private const int MaxDashChain = 2; // chain up to 2 additional dashes.
    private const string HitSound = "MeleeHit";
    private const float HitCameraShakeForce = 4f;
    private const float HitCameraShakeTime = 0.167f;
    private const float HitLensDistortionIntensity = -0.48f;
    private const float HitLensDistortionDuration = 0.16f;
    private const float HitMotionBlurDuration = 0.33f;
    private const float HitMotionBlurIntensity = 1f;
    private const int HitVfxId = 2;


    /// 
    /// Components.
    /// 


    [Header("Components")]
    [Tooltip("The amount (from the start of a dash) to disable gravity for; enabling it again on timeout.")]
    [SerializeField] private OneShotTimer gravityDisableTimer;
    [Tooltip("The window for how long a dash chain can be executed after successfully hitting a health component.")]
    [SerializeField] private OneShotTimer dashChainWindowTimer;
    [SerializeField] private SkinnedMeshTrailSystem arcGhost;
    [SerializeField] private DodgeTrailController dodgeTrail;
    [SerializeField] private AnimationCurve forceCurve;
    [SerializeField] private SingleHitbox hitbox;


    ///
    /// Unique Data.
    /// 

    [Header("Data")]
    [Tooltip("The layers to exclude when dashing")]
    [SerializeField] LayerMask dashStateExcludeLayers;
    LayerMask previousStateExcludeLayers; // the layer mask for the state before (idle state) dashing.
    [RuntimeField] private bool hitHealthObjectThisDash = false;
    [RuntimeField] private int dashChainCount = 0;

    /// 
    /// IAnimated Skill Field Overrides.
    /// 


    private event Action animationCompleted;

    Action IAnimatedSkill.AnimationCompleted 
    { 
        get => animationCompleted; 
        set => animationCompleted = value; 
    }

    Animator IAnimatedSkill.Animator => Player.Animator;

    AnimationEventReciever IAnimatedSkill.AnimationEventReciever => Player.AnimationEventReciever;

    string IAnimatedSkill.AnimationName => AnimationName;

    string IAnimatedSkill.AnimationCompletedEventName => AnimationCompletedEventName;

    Player IAnimatedSkill.Player => Player;

    int IAnimatedSkill.AnimationLayer => 0;

    Skill IAnimatedSkill.Skill => this;

    Coroutine animationLayerWeightTransitionCoroutine;
    Coroutine IAnimatedSkill.AnimationLayerWeightTransitionCoroutine 
    { 
        get => animationLayerWeightTransitionCoroutine;
        set => animationLayerWeightTransitionCoroutine = value; 
    }

    public bool AnimationCancel => true;

    /// 
    /// IMovementSkill Field Overrides.
    /// 


    PlayerStats IMovementSkill.PlayerStats => Player.PlayerStats; 
    

    /// 
    /// ITimedStateSkill Field Overrides.
    /// 


    [SerializeField] OneShotTimer stateTimer;
    OneShotTimer ITimedStateSkill.StateTimer => stateTimer;


    /// 
    /// ICooldownSkill Field Overrides.
    /// 


    [SerializeField] OneShotTimer cooldownTimer;
    OneShotTimer ICooldownSkill.CooldownTimer => cooldownTimer;

    bool IAnimatedSkill.AllowsAnimationCancel => false;
    bool IAnimatedSkill.CausesAnimationCancel => true;


    ///
    /// Cached Interface References.
    /// 


    ITimedStateSkill ITimedStateSkill;
    IMovementSkill IMovementSkill;
    IAnimatedSkill IAnimatedSkill;
    ICooldownSkill ICooldownSkill;


    /// 
    /// Base.
    /// 


    protected override void UseInternal()
    {
        
        IAnimatedSkill.UseAnimatedSkill();

        inUse = true;

        stateTimer.Begin();
        gravityDisableTimer.Begin();
        dashChainWindowTimer.Halt();

        // Apply the dash force relative to the camera's rotation.

        Player.CharacterControllerMovement.Impulse(Player.CameraController.transform.forward, forceCurve);
        Player.CharacterControllerMovement.UseGravity = false;

        // move only in the direction of our dodge.

        Player.BlockMoveInput(); 
        Player.BlockJumpInput();

        Player.SnapToFaceMoveInput();
        Player.FaceMoveDirection();
        
        Player.CharacterControllerMovement.moveDirection = Vector3.zero;
        Player.CharacterControllerMovement.HaltMoveDirectionVelocity();
        Player.CharacterControllerMovement.ClearGravityVelocity();
        Player.JumpMovement.StopJumping();

        // Make player invulnerable and dash through enemies.

        Player.EnterIFrames();
        previousStateExcludeLayers = Player.CharacterControllerMovement.CharacterController.excludeLayers;
        Player.CharacterControllerMovement.CharacterController.excludeLayers = dashStateExcludeLayers;

        // play animations and vfx.
        
        arcGhost.SpawnMeshes();
        dodgeTrail.EnableTrail();
        Player.AudioPlayer.PlaySound(DashSound, Player.gameObject);

        // enable the dash hitbox.

        hitbox.Enable();
    }

    public override bool CanUse()
    {
        return
            IAnimatedSkill.CanUseAnimatedSkill()
            && ITimedStateSkill.CanUseTimedStateSkill()
            && ICooldownSkill.CanUseCooldownSkill();
    }

    protected override void GetInterfaceTypes()
    {
        IAnimatedSkill = this;
        IMovementSkill = this;
        ITimedStateSkill = this;
        ICooldownSkill = this;
    }


    /// 
    /// Linkage.
    /// 


    protected override void LinkEvents()
    {
        gravityDisableTimer.Timeout += OnAerialGravityDisableTimeout;
        IAnimatedSkill.LinkAnimatedSkillEvents();
        IMovementSkill.LinkMovementSkillEvents();
        ITimedStateSkill.LinkTimedStateSkillEvents();
        hitbox.HitHealth += OnHitHealth;
        dashChainWindowTimer.Timeout += OnDashChainWindowTimeout;
    }

    protected override void UnlinkEvents()
    {
        gravityDisableTimer.Timeout -= OnAerialGravityDisableTimeout;
        IAnimatedSkill.UnlinkAnimatedSkillEvents();
        IMovementSkill.UnlinkMovementSkillEvents();
        ITimedStateSkill.UnlinkTimedStateSkillEvents();
        dashChainWindowTimer.Timeout -= OnDashChainWindowTimeout;
    }

    private void OnAerialGravityDisableTimeout()
    {
        Player.CharacterControllerMovement.UseGravity = true;
    }

    private void OnHitHealth(GameObject other, Vector3 hitPoint)
    {

        // queue another dash in the chain on the first hit of a health component.
        // and we have not exceeded our maximum dash amount.

        if(hitHealthObjectThisDash == false && dashChainCount < MaxDashChain)
        {
            dashChainCount++;
            hitHealthObjectThisDash = true;
        }

        Player.VfxPlayerSpawner.PlayVfx(HitVfxId, hitPoint, transform.forward);
        Player.CameraController.StartShaking(HitCameraShakeForce, HitCameraShakeTime);

        Player.CameraPostProcessingController.PulseLensDistortionIntensity(
            HitLensDistortionDuration,
            HitLensDistortionIntensity
        );

        Player.CameraPostProcessingController.PulseMotionBlurIntensity(
            HitMotionBlurDuration,
            HitMotionBlurIntensity
        );
        
        Player.AudioPlayer.PlaySound(HitSound, hitPoint);
    }

    private void OnDashChainWindowTimeout()
    {
        // complete our dash chain if the player does not execute a
        // dash within the given window of time.

        DashChainCompleted();
    }


    /// 
    /// IAnimatedSkill function overrides.
    /// 


    void IAnimatedSkill.OnAnimationEventTriggered(string eventName)
    {
        // Do nothing as this is a Timed Skill;
        // The dash shouldnt have any animation events as it is a looped animation.
    }

    void IAnimatedSkill.OnAnimationCompleted()
    {
        // Do nothing as this is a Timed Skill.
        // The animation is looped so this will never be called.
        
    }
    void IAnimatedSkill.Cancel()
    {
        throw new NotImplementedException();
    }


    /// 
    /// ITimedStateSkill function overrides.
    /// 


    void ITimedStateSkill.OnStateTimerTimeout()
    {
        inUse = false;

        Player.CharacterControllerMovement.CharacterController.excludeLayers = previousStateExcludeLayers;
        Player.ExitIFrames();

        // re-enable move input.

        Player.UnblockMoveInput(); 
        Player.UnblockJumpInput();
    
        // deacivate the hitbox.

        hitbox.Disable();

        if(hitHealthObjectThisDash == false)
        {
            DashChainCompleted();
        }
        else
        {
            dashChainWindowTimer.Begin();
            hitHealthObjectThisDash = false;            
        }

        Player.EnterRestState();
    }

    private void DashChainCompleted()
    {        
        cooldownTimer.Begin();
        dashChainCount = 0;
        hitHealthObjectThisDash = false;
    }


    /// 
    /// ICooldownSkill function overrides.
    /// 


    public void OnCooldownTimeout()
    {
        // do nothing.
    }


    /// 
    /// IMovementSkill function overrides.
    /// 


    void IMovementSkill.OnCalculatedScaledMoveSpeedModifier(float value)
    {
        Entropek.UnityUtils.AnimationCurve.MultiplyKeyValues(forceCurve, value); 
    }
}
