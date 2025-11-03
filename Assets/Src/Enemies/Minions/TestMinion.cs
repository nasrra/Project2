using UnityEngine;

public class TestMinion : Enemy
{
    [Header(nameof(TestMinion)+" Components")]
    [SerializeField] Entropek.Projectiles.ProjectileSpawner projectileSpawner;
    [SerializeField] Entropek.Time.OneShotTimer attackStateTimer;

    void OnEnable()
    {
        ChaseState();
    }

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

    public override void IdleState()
    {
        Debug.Log("Idle");
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
        Debug.Log("Shoot");
        projectileSpawner.Fire(0, 0, target);
        attackStateTimer.Begin();
    }


    protected override void OnCombatActionChosen(Entropek.Ai.Combat.AiCombatAction action)
    {
        switch (action.Name)
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
    }

    protected override void UnlinkEvents()
    {
        base.UnlinkEvents();
        UnlinkTimerEvents();
    }

    private void LinkTimerEvents()
    {
        attackStateTimer.Timeout += AttackEndedState;
    }

    private void UnlinkTimerEvents()
    {
        attackStateTimer.Timeout -= AttackEndedState;        
    }
}
