using Entropek.Combat;
using Entropek.Vfx;
using UnityEngine;

public abstract class MushroomMinion : Minion
{
    private const string FootStepSfx = "FootstepGrassMedium";
    private const string FootStepAnimationEvent = "Footstep";
    private const string StunAnimation = "SA_Mushroom_Damage";
    private const string HeadbuttAnimation = "SA_Mushroom_Headbutt";
    private const string HeadbuttAttackFrameAnimationEvent = "HeadbuttAttackFrame";

    private const string FleeStateAgentOutcome = "Flee";
    private const string ChaseStateAgentOutcome = "Chase";
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
        navAgentMovement.PausePath();
        stateAgent.HaltEvaluationLoop();
    }

    public override void IdleState()
    {
    }

    public override void IdleState(float time)
    {
        IdleState();
        stateQeueue.Enqueue(combatAgent.BeginEvaluationLoop);
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

    public override void Kill()
    {
        Destroy(gameObject);
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

    protected override void AttackEndedState()
    {
        base.AttackEndedState();
        
        // go back to the chosen state when the attack has finished.

        stateQeueue.Enqueue(
            () =>
            {
                OnStateAgentOutcomeChosen(stateAgent.ChosenState.Name);
                stateAgent.BeginEvaluationLoop();
            }
        );
    }


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
        audioPlayer.PlaySound(FootStepSfx, gameObject);
    }

    private void OnHeadbuttAttackFrameAnimationEvent()
    {
        headbuttVfx.Play();
        headbuttHitbox.Enable();
    }


    ///
    /// State Agent Outcomes. 
    ///


    private void OnStateAgentOutcomeChosen(string outcomeName)
    {
        switch (outcomeName)
        {
            case ChaseStateAgentOutcome:
                ChaseState();
                break;
            case FleeStateAgentOutcome:
                FleeState();
                break;
            default:
                Debug.LogError($"Mushroom Minion does not implement state: {outcomeName}");
                break;
        }
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


    ///
    /// Event Linkage Override.
    /// 


    protected override void LinkEvents()
    {
        base.LinkEvents();
        LinkStateAgentEvents();
    }

    protected override void UnlinkEvents()
    {
        base.UnlinkEvents();
        UnlinkStateAgentEvents();
    }

    protected override void LinkTimerEvents()
    {
        base.LinkTimerEvents();
    }

    protected override void UnlinkTimerEvents()
    {
        base.UnlinkTimerEvents();
    }

    protected void LinkStateAgentEvents()
    {
        stateAgent.OutcomeChosen += OnStateAgentOutcomeChosen;
    }

    protected void UnlinkStateAgentEvents()
    {
        stateAgent.OutcomeChosen -= OnStateAgentOutcomeChosen;        
    }

}
