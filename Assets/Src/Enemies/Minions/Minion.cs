using Entropek.Time;
using UnityEngine;

public abstract class Minion : Enemy
{
    [Header(nameof(Minion)+" Components")]
    [SerializeField] protected OneShotTimer stunTimer;

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
        stunTimer.Timeout += OnStunTimeout;
    }

    protected virtual void UnlinkTimerEvents()
    {
        stunTimer.Timeout -= OnStunTimeout;
    }

    private void OnStunTimeout()
    {
        ExitStunState();
    }

    /// <summary>
    /// Stuns this Minion for an amount of time.
    /// </summary>
    /// <param name="time">The specified amount of time to remain in the stunned state.</param>

    public void EnterStunState(float time)
    {
        navAgentMovement.PausePath();
        combatAgent.HaltEvaluationLoop();
        stateQeueue.Halt();
        stunTimer.Begin(time);

        EnterStunStateInternal();    
    }

    protected abstract void EnterStunStateInternal();

    /// <summary>
    /// Forces this Minion to exit its stun state.
    /// </summary>

    private void ExitStunState()
    {
        navAgentMovement.ResumePath();
        navAgentMovement.RecalculatePath();
        combatAgent.BeginEvaluationLoop();
        stateQeueue.Begin();
        ExitStunStateInternal();
    }

    protected abstract void ExitStunStateInternal();
}
