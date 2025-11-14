using System;
using Entropek.Systems.Trails;
using Entropek.UnityUtils.AnimatorUtils;
using UnityEngine;

public class DodgeSkill : Skill, IAnimatedSkill, IMovementSkill
{


    ///
    /// Constants.
    /// 


    private const string DodgeSound = "Dodge";
    private const string AnimationName = "Rig_Roll";
    private const string AnimationCompletedEventName = "ExitDodgeState";
    private const float BaseDodgeForce = 23.33f;
    private const float BaseDodgeDecaySpeed = BaseDodgeForce * 2.66f;


    /// 
    /// Components.
    /// 


    [Header("Components")]
    [SerializeField] private SkinnedMeshTrailSystem arcGhost;
    [SerializeField] private DodgeTrailController dodgeTrail;


    ///
    /// Data.
    /// 


    private float dodgeForce = BaseDodgeForce;
    private float dodgeDecaySpeed = BaseDodgeDecaySpeed;


    /// 
    /// IAnimated Skill Field Overrides.
    /// 


    IAnimatedSkill IAnimatedSkill;

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

    IMovementSkill IMovementSkill;


    PlayerStats IMovementSkill.PlayerStats => Player.PlayerStats; 
    
    bool IAnimatedSkill.AnimationCancel => throw new NotImplementedException();



    /// 
    /// Base.
    /// 

    protected override void UseInternal()
    {
        inUse = true;

        // move only in the direction of our dodge.

        Player.BlockMoveInput(); 
        Player.BlockJumpInput();

        Player.SnapToFaceMoveInput();
        Player.FaceMoveDirection();
        
        Player.CharacterControllerMovement.moveDirection = Vector3.zero;
        Player.CharacterControllerMovement.HaltMoveDirectionVelocity();
        Player.CharacterControllerMovement.ClearGravityVelocity();
        Player.JumpMovement.StopJumping();
        
        Player.CharacterControllerMovement.ImpulseRelativeToGround(transform.forward, dodgeForce, dodgeDecaySpeed);

        // Make player invulnerable.

        Player.EnterIFrames();

        // play animations and vfx.

        arcGhost.SpawnMeshes();
        dodgeTrail.EnableTrail();
        Player.AudioPlayer.PlaySound(DodgeSound, Player.gameObject);
        IAnimatedSkill.PlayAnimation();
    }

    public override bool CanUse()
    {
        return IAnimatedSkill.CanUseAnimatedSkill();
    }

    protected override void GetInterfaceTypes()
    {
        IAnimatedSkill = this;
        IMovementSkill = this;
    }


    /// 
    /// Linkage.
    /// 


    protected override void LinkEvents()
    {
        IAnimatedSkill.LinkAnimatedSkillEvents();
        IMovementSkill.LinkMovementSkillEvents();
    }

    protected override void UnlinkEvents()
    {
        IAnimatedSkill.UnlinkAnimatedSkillEvents();
        IMovementSkill.UnlinkMovementSkillEvents();
    }


    /// 
    /// IAnimatedSkill function overrides.
    /// 


    void IAnimatedSkill.OnAnimationEventTriggered(string eventName)
    {
        switch (eventName)
        {            
            case "EnterIFrames": Player.EnterIFrames(); break;
            case "ExitIFrames" : Player.ExitIFrames(); break;
        }
    }

    void IAnimatedSkill.OnAnimationCompleted()
    {
        inUse = false;

        // re-enable move input.
        Player.UnblockMoveInput(); 
        Player.UnblockJumpInput();
    }

    void IAnimatedSkill.Cancel()
    {
        throw new NotImplementedException();
    }

    void IMovementSkill.OnCalculatedScaledMoveSpeedModifier(float value)
    {
        dodgeForce = BaseDodgeForce * value;
        dodgeDecaySpeed = BaseDodgeDecaySpeed * value;
    }
}
