using System;
using Entropek.Systems.Trails;
using Entropek.Time;
using Entropek.UnityUtils.AnimatorUtils;
using UnityEngine;

public class DashSkill : Skill, IAnimatedSkill, IMovementSkill, ITimedStateSkill
{


    ///
    /// Constants.
    /// 


    private const string DashSound = "Dodge";
    private const string AnimationName = "GroundDash";
    private const string AnimationCompletedEventName = "ExitDashState";
    private const float DashForce = 33.33f;
    private const float DashDecaySpeed = DashForce;    


    /// 
    /// Components.
    /// 


    [Header("Components")]
    [SerializeField] private SkinnedMeshTrailSystem arcGhost;
    [SerializeField] private DodgeTrailController dodgeTrail;


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
    
    float moveSpeedModifier = 1;
    float IMovementSkill.MoveSpeedModifier 
    { 
        get => moveSpeedModifier; 
        set => moveSpeedModifier = value; 
    }


    /// 
    /// ITimedStateSkill Field Overrides.
    /// 

    [SerializeField] OneShotTimer stateTimer;
    OneShotTimer ITimedStateSkill.StateTimer => stateTimer;


    ///
    /// Cached Interface References.
    /// 


    ITimedStateSkill ITimedStateSkill;
    IMovementSkill IMovementSkill;
    IAnimatedSkill IAnimatedSkill;


    /// 
    /// Base.
    /// 


    protected override void UseInternal()
    {
        inUse = true;

        stateTimer.Begin();

        Player.EnterIFrames();

        // move only in the direction of our dodge.

        Player.BlockMoveInput(); 
        Player.BlockJumpInput();

        Player.SnapToFaceMoveInput();
        Player.FaceMoveDirection();
        
        Player.CharacterControllerMovement.moveDirection = Vector3.zero;
        Player.CharacterControllerMovement.HaltMoveDirectionVelocity();
        Player.CharacterControllerMovement.ClearGravityVelocity();
        Player.JumpMovement.StopJumping();
        
        Player.ForceApplier.ImpulseRelativeToGround(transform.forward, IMovementSkill.ApplyMoveSpeedModifier(DashForce), IMovementSkill.ApplyMoveSpeedModifier(DashDecaySpeed));

        // Make player invulnerable.

        Player.EnterIFrames();

        // play animations and vfx.

        arcGhost.SpawnMeshes();
        dodgeTrail.EnableTrail();
        Player.AudioPlayer.PlaySound(DashSound, Player.gameObject);
        IAnimatedSkill.PlayAnimation();
    }

    public override bool CanUse()
    {
        return IAnimatedSkill.CanUseAnimatedSkill()
        && ITimedStateSkill.CanUseTimedStateSkill();
    }

    protected override void GetInterfaceTypes()
    {
        IAnimatedSkill = this;
        IMovementSkill = this;
        ITimedStateSkill = this;
    }


    /// 
    /// Linkage.
    /// 


    protected override void LinkEvents()
    {
        IAnimatedSkill.LinkAnimatedSkillEvents();
        IMovementSkill.LinkMovementSkillEvents();
        ITimedStateSkill.LinkTimedStateSkillEvents();
    }

    protected override void UnlinkEvents()
    {
        IAnimatedSkill.UnlinkAnimatedSkillEvents();
        IMovementSkill.UnlinkMovementSkillEvents();
        ITimedStateSkill.UnlinkTimedStateSkillEvents();
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

    void ITimedStateSkill.OnStateTimerTimeout()
    {
        inUse = false;

        Player.ExitIFrames();
        Player.EnterRestState();

        // re-enable move input.
        Player.UnblockMoveInput(); 
        Player.UnblockJumpInput();
    }
}
