using Entropek.Combat;
using UnityEngine;

public class TestMinion : Minion
{
    [Header(nameof(TestMinion)+" Components")]
    [SerializeField] Entropek.Projectiles.ProjectileSpawner projectileSpawner;
    [SerializeField] Entropek.Time.OneShotTimer attackStateTimer;
    [SerializeField] protected Entropek.Ai.AiStateAgent stateAgent;

    const DamageType StaggerDamageType = DamageType.Light | DamageType.Heavy;

    public override void AttackState()
    {
        throw new System.NotImplementedException();
    }

    public override void ChaseState()
    {
        combatAgent.BeginEvaluationLoop();
        movement.ResumePath();
        movement.StartPath(navAgentMovementTarget);
    }

    public void FleeState()
    {
        combatAgent.BeginEvaluationLoop();
        movement.ResumePath();
        movement.MoveAway(navAgentMovementTarget, 24);
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

    public void Shoot(Transform target)
    {
        projectileSpawner.FireAtTarget(0, 0, target);
        attackStateTimer.Begin();
    }


    protected override void OnCombatActionChosen(in string actionName)
    {
        switch (actionName)
        {
            case "Shoot":
                Shoot(target);
                break;
            default:
                break;
        }
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
        attackStateTimer.Timeout += AttackEndedState;
    }

    protected override void UnlinkTimerEvents()
    {
        base.UnlinkTimerEvents();
        attackStateTimer.Timeout -= AttackEndedState;        
    }

    protected void LinkStateAgentEvents()
    {
        stateAgent.OutcomeChosen += OnStateAgentOutcomeChosen;
    }

    protected void UnlinkStateAgentEvents()
    {
        stateAgent.OutcomeChosen -= OnStateAgentOutcomeChosen;        
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
                Debug.LogError($"Test Minion does not implment state: {outcomeName}");
                break;
        }
    }

    protected override void OnHealthDamaged(DamageContext damageContext)
    {
        
        // if the damaging type is of stagger type.

        if((damageContext.DamageType & StaggerDamageType) != 0)
        {
            EnterStaggerState(0.64f);
        }
    }

    protected override void EnterStaggerState(float time)
    {
        combatAgent.HaltEvaluationLoop();
        stateAgent.HaltEvaluationLoop();
        movement.PausePath();
        staggerTimer.Begin(time);
    }

    protected override void ExitStaggerState()
    {
        combatAgent.BeginEvaluationLoop();
        stateAgent.BeginEvaluationLoop();
        movement.ResumePath();
    }
}
