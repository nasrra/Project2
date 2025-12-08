using Entropek.Combat;
using UnityEngine;

public abstract class BatMinion : Minion
{
    private const string WingsFlapSfx = "SmallWingsFlap";
    private const string WingsFlapAnimationEvent = "WingsFlap";
    private const string StunAnimationName = "SA_Bat_Damage";

    const DamageType StaggerDamageType = DamageType.Light | DamageType.Heavy;

    void Start()
    {
        ChaseState();
    }

    protected virtual void FixedUpdate()
    {
        FaceMoveDirection();
    }

    public override void AttackState()
    {
        throw new System.NotImplementedException();
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

    /// <summary>
    /// shoot at a position in world space.
    /// </summary>
    /// <param name="position">The specified position in world space.</param>

    public abstract void Shoot(Vector3 position);

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


    protected override bool OnAnimationEventTriggered(string eventName)
    {
        if (base.OnAnimationEventTriggered(eventName) == true)
        {
            return true;
        }
        switch (eventName)
        {
            case WingsFlapAnimationEvent:
                audioPlayer.PlaySound(WingsFlapSfx, transform);
                return true;
            default: 
                return false;
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
