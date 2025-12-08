using Entropek.Combat;
using UnityEngine;

public class TestMinion : Minion
{
    [Header(nameof(TestMinion)+" Components")]
    [SerializeField] Entropek.Projectiles.ProjectileSpawner projectileSpawner;
    [SerializeField] Entropek.Time.OneShotTimer attackStateTimer;
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
        aiActionAgent.BeginEvaluationLoop();
        navAgentMovement.ResumePath();
        navAgentMovement.StartPath(navAgentMovementTarget);
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

    public void Shoot(Transform target)
    {
        projectileSpawner.FireAtTarget(0, 0, target);
        attackStateTimer.Begin();
    }


    protected override bool OnCombatActionChosen(in string actionName)
    {
        switch (actionName)
        {
            case "Shoot":
                Shoot(target);
                return true;
            default:
                return false;
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

    protected override void OnHealthDamaged(DamageContext damageContext)
    {
        
        // if the damaging type is of stagger type.

        // if((damageContext.DamageType & StaggerDamageType) != 0)
        // {
        //     EnterStunState(0.64f);
        // }
    }

    protected override void EnterStunStateInternal()
    {
        // do nothing.        
    }

    protected override void ExitStunStateInternal()
    {
        // do nothing.
    }

}
