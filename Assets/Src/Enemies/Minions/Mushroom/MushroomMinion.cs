using Entropek.Combat;
using Entropek.Vfx;
using UnityEngine;

public abstract class MushroomMinion : Minion
{
    private const string FootStepAnimationEvent = "Footstep";
    private const string StunAnimation = "SA_Mushroom_Damage";
    private const string HeadbuttAnimation = "SA_Mushroom_Headbutt";
    private const string HeadbuttAttackFrameAnimationEvent = "HeadbuttAttackFrame";

    private const string HeadbuttActionAgentOutcome = "Headbutt";

    [Header(nameof(MushroomMinion)+" Components")]
    [SerializeField] protected Entropek.Ai.AiStateAgent stateAgent;
    [SerializeField] private SingleVfxPlayer headbuttVfx;
    [SerializeField] private TimedSingleHitbox headbuttHitbox;


    /// 
    /// Base.
    /// 


    protected virtual void FixedUpdate()
    {
        FaceMoveDirection();
    }


    /// 
    /// State Machine.
    /// 


    public override void AttackState()
    {
        base.AttackState();
        navAgentMovement.PausePath();
    }

    public override void IdleState()
    {
    }

    public override void IdleState(float time)
    {
        IdleState();
        stateQeueue.Enqueue(aiActionAgent.BeginEvaluationLoop);
        stateQeueue.Begin(time);
    }

    protected override void EnterStunStateInternal()
    {
        animator.Play(StunAnimation);
    }

    protected override void ExitStunStateInternal()
    {
        // do nothing.
    }

    protected override void OnHealthDeath()
    {
        Kill();
    }

    protected override void OnHealthDamaged(DamageContext damageContext)
    {
        
        // if the damaging type is of stagger type.

        EnterStunState(0.64f);
    }

    protected override void OnOpponentEngaged(Transform opponent)
    {
        target = opponent;
    }

    ///
    /// Unique Functions.
    /// 

    
    protected abstract string GetFootstepSfx();


    /// 
    /// Animation Events.
    /// 


    protected override bool OnAnimationEventTriggered(string eventName)
    {
        if (base.OnAnimationEventTriggered(eventName) == true)
        {
            return true;
        }
        switch (eventName)
        {
            case FootStepAnimationEvent:
                OnFootstepAnimationEvent();
                return true;
            case HeadbuttAttackFrameAnimationEvent:
                OnHeadbuttAttackFrameAnimationEvent();
                return true;
            default: 
                return false;
        }
    }

    private void OnFootstepAnimationEvent()
    {
        audioPlayer.PlaySound(GetFootstepSfx(), gameObject);
    }

    private void OnHeadbuttAttackFrameAnimationEvent()
    {
        headbuttVfx.Play();
        headbuttHitbox.Enable();
    }


    /// 
    /// Action Agent Outcomes
    /// 


    protected override bool OnCombatActionChosen(in string actionName)
    {
        switch (actionName)
        {
            case HeadbuttActionAgentOutcome:
                OnHeadbuttActionAgentOutcome();
                return true;
            default:
                return false;
        }
    }


    private void OnHeadbuttActionAgentOutcome()
    {
        animator.Play(HeadbuttAnimation);
        AttackState();
    }
}
