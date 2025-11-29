using Entropek.Combat;
using UnityEngine;

public abstract class BatMinion : Minion
{
    private const string WingsFlapSfx = "SmallWingsFlap";
    private const string WingsFlapAnimationEvent = "WingsFlap";
    private const string StunAnimationName = "SA_Bat_Damage";

    [Header(nameof(BatMinion)+" Components")]
    [SerializeField] protected Entropek.Ai.AiStateAgent stateAgent;

    const DamageType StaggerDamageType = DamageType.Light | DamageType.Heavy;

    private void FixedUpdate()
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

    public abstract void Shoot(Transform target);

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
            case WingsFlapAnimationEvent:
                audioPlayer.PlaySound(WingsFlapSfx, gameObject);
                return true;
            default: 
                return false;
        }
    }

    private void OnStateAgentOutcomeChosen(string outcomeName)
    {
        switch (outcomeName)
        {
            case "Chase":
                ChaseState();
                break;
            case "Flee":
                FleeState();
                break;
            default:
                Debug.LogError($"Test Minion does not implement state: {outcomeName}");
                break;
        }
    }

    protected override void OnHealthDamaged(DamageContext damageContext)
    {
        
        // if the damaging type is of stagger type.

        // if((damageContext.DamageType & StaggerDamageType) != 0)
        // {
            EnterStunState(0.64f);
        // }
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
