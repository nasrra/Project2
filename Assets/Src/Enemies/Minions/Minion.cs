using Entropek.Time;
using UnityEngine;

public abstract class Minion : Enemy
{
    [Header(nameof(Minion)+" Components")]
    [SerializeField] protected OneShotTimer staggerTimer;

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

    protected virtual void LinkTimerEvents()
    {
        staggerTimer.Timeout += OnStaggerTimeout;
    }

    protected virtual void UnlinkTimerEvents()
    {
        staggerTimer.Timeout -= OnStaggerTimeout;
    }

    private void OnStaggerTimeout()
    {
        ExitStaggerState();
    }

    public void EnterStaggerState(float time)
    {
        navAgentMovement.PausePath();
        combatAgent.HaltEvaluationLoop();
        stateQeueue.Halt();
        staggerTimer.Begin(time);

        EnterStaggerStateInternal();    
    }

    protected abstract void EnterStaggerStateInternal();

    private void ExitStaggerState()
    {
        navAgentMovement.ResumePath();
        combatAgent.BeginEvaluationLoop();
        stateQeueue.Begin();
        ExitStaggerStateInternal();
    }

    protected abstract void ExitStaggerStateInternal();
}
