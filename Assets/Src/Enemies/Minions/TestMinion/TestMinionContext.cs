using UnityEngine;
using Entropek.Ai.Contexts;
using Entropek.UnityUtils.Attributes;

public class TestMinionContext : AiAgentContext, ITargetContext
{
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

    ITargetContext targetContext;

    public override void BeginEvaluationLoop()
    {
        // do nothing.
    }

    public override void Evaluate()
    {
        targetContext.CalculateRelativeData(transform);
    }

    public override void HaltEvaluationLoop()
    {
        // do nothing.
    }

    protected override void RetrieveContextTypes()
    {
        targetContext = this;
    }
}
