using Entropek.Ai.Contexts;
using Entropek.EntityStats;
using Entropek.Time;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

[System.Serializable]
public class SlinkAiAgentContext : AiAgentContext,
IDamageIntervalContext, IOpponentContext
{


    /// 
    /// Opponent Context.
    /// 


    [Header("Runtime Data")]
    [RuntimeField] protected Health opponentHealth;
    Health IOpponentContext.Health 
    { 
        get => opponentHealth; 
        set => opponentHealth =  value;
    }
    
    [RuntimeField] protected Transform target; 
    Transform ITargetContext.Target 
    { 
        get => target;
        set => target = value; 
    }
    
    [RuntimeField] protected Vector3 vectorDistanceToTarget;
    Vector3 ITargetContext.VectorDistanceToTarget 
    { 
        get => vectorDistanceToTarget; 
        set => vectorDistanceToTarget = value; 
    }

    [RuntimeField] protected float distanceToTarget;
    float ITargetContext.DistanceToTarget 
    { 
        get => distanceToTarget; 
        set => distanceToTarget = value; 
    }
    
    [RuntimeField] protected float dotDirectionToTarget;
    float ITargetContext.DotDirectionToTarget 
    { 
        get => dotDirectionToTarget; 
        set => dotDirectionToTarget = value; 
    }


    /// 
    /// Damage Interval Context.
    /// 

    
    [Header("Components")]
    [SerializeField] protected LoopedTimer damgeTakenIntervalTimer;
    LoopedTimer IDamageIntervalContext.DamageTakenIntervalTimer 
    { 
        get => damgeTakenIntervalTimer; 
        set => damgeTakenIntervalTimer = value; 
    }

    [SerializeField] protected Health selfHealth;
    Health IDamageIntervalContext.SelfHealth 
    { 
        get => selfHealth; 
        set => selfHealth = value; 
    }

    [Header("Runtime Data")]
    [RuntimeField] protected float damageTakenInCurrentInterval;
    float IDamageIntervalContext.DamageTakenInCurrentInterval 
    { 
        get => damageTakenInCurrentInterval; 
        set => damageTakenInCurrentInterval = value; 
    }

    /// 
    /// Interface caching.
    /// 


    private IDamageIntervalContext damageIntervalContext;
    private IOpponentContext opponentContext;

    /// 
    /// Base.
    /// 


    public override void BeginEvaluationLoop()
    {
        damageIntervalContext.BeginEvaluationLoop();
    }

    public override void HaltEvaluationLoop()
    {
        damageIntervalContext.HaltEvaluationLoop();
    }

    public override void Evaluate()
    {
        opponentContext.CalculateRelativeData(transform);
    }

    protected override void RetrieveContextTypes()
    {
        damageIntervalContext = this;
        opponentContext = this;
    }
}
