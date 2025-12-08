using System;
using Entropek.Time;
using UnityEngine;

public abstract class Minion : Enemy
{


    /// 
    /// Constants & Statics.
    /// 


    private const int DeathCurrencyAwardMin = 10;
    private const int DeathCurrencyAwardMax = 17;
    public static Currency deathCurrency;


    /// 
    /// Callbacks.
    /// 


    public static event Action<Currency, int> AwardDeathCurrency;


    /// 
    /// Components. 
    /// 


    [Header(nameof(Minion)+" Components")]
    [SerializeField] protected OneShotTimer stunTimer;


    /// 
    /// Base.
    /// 


    public override void Kill()
    {
        // gameObject.SetActive(false);
        int amount = UnityEngine.Random.Range(DeathCurrencyAwardMin, DeathCurrencyAwardMax + 1); // add one as its exclusive.
        AwardDeathCurrency?.Invoke(deathCurrency, amount);
        Destroy(gameObject);
    }

    /// <summary>
    /// Stuns this Minion for an amount of time.
    /// </summary>
    /// <param name="time">The specified amount of time to remain in the stunned state.</param>

    public void EnterStunState(float time)
    {
        navAgentMovement.PausePath();
        aiActionAgent.HaltEvaluationLoop();
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
        aiActionAgent.BeginEvaluationLoop();
        stateQeueue.Begin();
        ExitStunStateInternal();
    }

    protected abstract void ExitStunStateInternal();


    /// 
    /// Event Linkage.
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

    private static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initalise()
        {
            deathCurrency = Resources.Load<Currency>("ScriptableObject/Currency/ArcStone");
        }
    }
}
