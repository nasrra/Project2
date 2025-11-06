using UnityEngine;

public class TestMinion : Enemy
{
    [Header(nameof(TestMinion)+" Components")]
    [SerializeField] Entropek.Projectiles.ProjectileSpawner projectileSpawner;
    [SerializeField] Entropek.Time.OneShotTimer attackStateTimer;
    [SerializeField] protected Entropek.Ai.AiStateAgent stateAgent;


    public override void AttackState()
    {
        throw new System.NotImplementedException();
    }

    public override void ChaseState()
    {
        combatAgent.BeginEvaluationLoop();
        movement.ResumePath();
        movement.StartPath(target);     
    }

    public void FleeState()
    {
        combatAgent.BeginEvaluationLoop();
        movement.ResumePath();
        movement.MoveAway(target, 24);        
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
        projectileSpawner.Fire(0, 0, target);
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
        LinkTimerEvents();
        LinkStateAgentEvents();
    }

    protected override void UnlinkEvents()
    {
        base.UnlinkEvents();
        UnlinkTimerEvents();
        UnlinkStateAgentEvents();
    }

    private void LinkTimerEvents()
    {
        attackStateTimer.Timeout += AttackEndedState;
    }

    private void UnlinkTimerEvents()
    {
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
}
