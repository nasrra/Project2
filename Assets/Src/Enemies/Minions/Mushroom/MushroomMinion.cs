using Entropek.Combat;
using UnityEngine;

public abstract class MushroomMinion : Minion
{
    private const string FootStepSfx = "FootstepGrassMedium";
    private const string FootStepAnimationEvent = "Footstep";
    private const string StunAnimationName = "SA_Mushroom_Damage";

    private const string ShootActionAgentOutcome = "Shoot";

    private const string FleeStateAgentOutcome = "Flee";
    private const string ChaseStateAgentOutcome = "Chase";

    [Header(nameof(BatMinion)+" Components")]
    [SerializeField] protected Entropek.Ai.AiStateAgent stateAgent;

    protected virtual void FixedUpdate()
    {
        FaceMoveDirection();
    }

    public override void AttackState()
    {
        throw new System.NotImplementedException();
    }

    public override void ChaseState()
    {
        combatAgent.BeginEvaluationLoop();
        navAgentMovement.ResumePath();
        navAgentMovement.StartPath(navAgentMovementTarget);
    }

    public void FleeState()
    {
        combatAgent.BeginEvaluationLoop();
        navAgentMovement.ResumePath();
        navAgentMovement.MoveAway(navAgentMovementTarget, 24);
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

    public override void Kill()
    {
        Destroy(gameObject);
    }

    protected override void OnHealthDeath()
    {
        Kill();
    }

    protected override void OnOpponentEngaged(Transform opponent)
    {
        target = opponent;
    }

    ///
    /// Linkage.
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

    protected override bool OnAnimationEventTriggered(string eventName)
    {
        if (base.OnAnimationEventTriggered(eventName) == true)
        {
            return true;
        }
        switch (eventName)
        {
            case FootStepAnimationEvent:
                audioPlayer.PlaySound(FootStepSfx, gameObject);
                return true;
            default: 
                return false;
        }
    }

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

    protected override void OnCombatActionChosen(in string actionName)
    {
        switch (actionName)
        {
            case ShootActionAgentOutcome:
                OnShootActionAgentOutome();
                break;
            default:
                Debug.LogError($"Mushroom Minion does not implement action: {actionName}");
                break;
        }
    }

    private void OnShootActionAgentOutome()
    {
        Debug.Log("shoot!");
    }

    protected override void OnHealthDamaged(DamageContext damageContext)
    {
        
        // if the damaging type is of stagger type.

        EnterStunState(0.64f);
    }

    protected override void EnterStunStateInternal()
    {
        animator.Play(StunAnimationName);
    }

    protected override void ExitStunStateInternal()
    {
        // do nothing.
    }

}
