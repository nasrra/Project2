using System;
using Entropek.Systems.Trails;
using Entropek.UnityUtils.AnimatorUtils;
using UnityEngine;

public class DodgeSkill : Skill, IAnimatedSkill
{


    ///
    /// Constants.
    /// 


    private const string DodgeSound = "Dodge";
    private const string AnimationName = "Rig_Roll";
    private const string AnimationCompletedEventName = "ExitDodgeState";
    private const float DodgeForce = 23.33f;
    private const float DodgeDecaySpeed = DodgeForce * 2.66f;    


    /// 
    /// Components.
    /// 


    [Header("Components")]
    [SerializeField] private SkinnedMeshTrailSystem arcGhost;
    [SerializeField] private DodgeTrailController dodgeTrail;


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


    /// 
    /// Base.
    /// 


    public override bool Use()
    {
        // enter state.

        Player.SnapToFaceMoveInput();
        Player.FaceMoveDirection();

        // move only in the direction of our dodge.

        Player.CharacterControllerMovement.moveDirection = Vector3.zero;
        Player.CharacterControllerMovement.HaltMoveDirectionVelocity();

        Player.CharacterControllerMovement.ClearGravityVelocity();

        Player.JumpMovement.StopJumping();

        Player.ForceApplier.ImpulseRelativeToGround(transform.forward, DodgeForce, DodgeDecaySpeed);

        // play animations and vfx.

        arcGhost.SpawnMeshes();
        dodgeTrail.EnableTrail();
        Player.AudioPlayer.PlaySound(DodgeSound, Player.gameObject);


        Player.EnterIFrames();

        IAnimatedSkill.PlayAnimation();
        return true;
    }

    protected override void GetInterfaceTypes()
    {
        IAnimatedSkill = this;
    }


    /// 
    /// Linkage.
    /// 


    protected override void LinkEvents()
    {
        IAnimatedSkill.LinkAnimatedSkillEvents();
    }

    protected override void UnlinkEvents()
    {
        IAnimatedSkill.UnlinkAnimatedSkillEvents();
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

}
