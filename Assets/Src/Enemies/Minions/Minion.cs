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
        staggerTimer.Timeout += ExitStaggerState;
    }

    protected virtual void UnlinkTimerEvents()
    {
        staggerTimer.Timeout -= ExitStaggerState;        
    }

    protected abstract void EnterStaggerState(float time);
    protected abstract void ExitStaggerState();
}
