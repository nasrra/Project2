using UnityEngine;
using Entropek.Combat;
using Entropek.Physics;
using Entropek.Time;
using Entropek.EntityStats;
using Entropek.Audio;
using Entropek.UnityUtils.AnimatorUtils;
using Entropek.Ai;
using Entropek.Ai.Contexts;

public abstract class Enemy : MonoBehaviour 
{

    /// 
    /// Constants.
    /// 


    private const string FleeStateAgentOutcome = "Flee";
    private const string ChaseStateAgentOutcome = "Chase";


    /// 
    /// Required Components.
    /// 


    [Header(nameof(Enemy) + " Required Components")]
    [SerializeField] protected Transform graphicsObject; // gameobject that holds the enemy mesh, vfx, etc.
    public Transform GraphicsObject => graphicsObject;
    
    [SerializeField] protected NavAgentMovementTarget navAgentMovementTarget;
    public NavAgentMovementTarget NavAgentMovementTarget => navAgentMovementTarget;

    [SerializeField] protected Transform target;
    public Transform Target => target;
    
    [SerializeField] protected Health health;
    public Health Health => health;

    [SerializeField] protected Animator animator;
    public Animator Animator => animator;

    [SerializeField] protected AnimationEventReciever animationEventReciever;
    public AnimationEventReciever AnimationEventReciever => animationEventReciever;
    
    [SerializeField] protected NavAgentMovement navAgentMovement;
    public NavAgentMovement NavAgentMovement =>  navAgentMovement;
    
    [SerializeField] protected AudioPlayer audioPlayer;
    public AudioPlayer AudioPlayer => audioPlayer; 

    [SerializeField] protected TimedActionQueue stateQeueue;
    public TimedActionQueue StateQueue => stateQeueue;

    [SerializeField] protected AiActionAgent aiActionAgent;
    public AiActionAgent AiActionAgent => aiActionAgent;

    /// 
    /// Optional Components.
    /// 


    [Header(nameof(Enemy) + " Optional Components")]
    
    [SerializeField] protected GroundCheck groundCheck;
    public GroundCheck GroundCheck => groundCheck;
    
    [SerializeField] protected AiStateAgent aiStateAgent;
    public AiStateAgent AiStateAgent => aiStateAgent;


    /// 
    /// Data. 
    /// 


    [Header(nameof(Enemy) + " Data")]
    [SerializeField] private float faceTargetDirectionSpeed = 3.33f;
    [SerializeField] private float faceMoveDirectionSpeed = 3.33f;
    [SerializeField] private float faceTargetDotThreshold = 0.9f;


    /// 
    /// Base.
    /// 


    void Awake()
    {

        LinkEvents();
        InitialiseAiAgentContexts();
    }

    void OnDestroy()
    {
        UnlinkEvents();
    }

    ///
    /// State Machine.
    /// 

    private void InitialiseAiAgentContexts()
    {        
        // immediately start tracking the player.

        AiAgentContext actionContext = aiActionAgent.AiAgentContext;

        target = Opponent.Singleton.transform;
        navAgentMovementTarget = Opponent.Singleton.NavAgentMovementTarget;

        switch (actionContext)
        {
            
            // check IOpponentContext first as it implements the target context;

            case Entropek.Ai.Contexts.IOpponentContext opponentContext:
                opponentContext.Target = Opponent.Singleton.transform;
                opponentContext.Health = Opponent.Singleton.Health;
            break;
            case Entropek.Ai.Contexts.ITargetContext targetContext:
                targetContext.Target = Opponent.Singleton.transform;
            break;
        }
    }

    public abstract void IdleState();
    public abstract void IdleState(float time);
    
    public virtual void ChaseState()
    {
        aiActionAgent.BeginEvaluationLoop();
        navAgentMovement.ResumePath();
        navAgentMovement.StartPath(navAgentMovementTarget);    
    }

    public virtual void FleeState()
    {
        aiActionAgent.BeginEvaluationLoop();
        navAgentMovement.ResumePath();
        navAgentMovement.MoveAway(navAgentMovementTarget, 24);
    }

    public virtual void AttackState()
    {
        if (aiStateAgent != null)
        {
            aiStateAgent.HaltEvaluationLoop();
        }
    }

    protected virtual void AttackEndedState()
    {
        aiActionAgent.BeginChosenActionCooldown();

        // get the attack that has just been completed. 

        Entropek.Ai.AiAction endedAttack = aiActionAgent.ChosenAction;

        // evaulate for a new action immediately up time out if set to true.

        if (endedAttack.EvaluateOnIdleTimeout == true)
        {
            if (aiActionAgent.Evaluate() == true)
            {
                return;
            }
        }

        // start idle state for however long it is defined for.

        IdleState(endedAttack.IdleTime);

        // go back to the chosen state when the idle state has finished.
        
        if (aiStateAgent != null)
        {
            stateQeueue.Enqueue(
                () =>
                {
                    OnStateAgentOutcomeChosen(aiStateAgent.ChosenState.Name);
                    aiStateAgent.BeginEvaluationLoop();
                }
            );            
        }
    }

    /// 
    /// Functions. 
    /// 


    public abstract void Kill();

    protected void FaceMoveDirection()
    {
        Vector3 moveDirection = navAgentMovement.moveDirection;
        if (moveDirection != Vector3.zero)
        {
            Entropek.UnityUtils.TransformUtils.RotateYAxisToDirection(graphicsObject, navAgentMovement.moveDirection, faceMoveDirectionSpeed * Time.deltaTime);
        }
    }

    protected void RotateGraphicsTransformToGroundNormal()
    {

        // Get the rotation needed to align up with the ground.

        Quaternion alignToGround = Quaternion.FromToRotation(graphicsObject.up, groundCheck.GroundNormal) * graphicsObject.rotation;

        // Extract Euler Angles.

        Vector3 originalEuler = graphicsObject.rotation.eulerAngles;
        Vector3 targetEuler = alignToGround.eulerAngles;

        Vector3 finalEuler = new Vector3(targetEuler.x, originalEuler.y, targetEuler.z);
        graphicsObject.rotation = Quaternion.Euler(finalEuler);
    }

    protected bool FaceTargetFixedUpdate()
    {
        return FaceWorldSpacePosition(target.position);
    }

    protected bool FaceWorldSpacePosition(Vector3 worldPosition)
    {
        // keep the same y-position so this enemy only rotates on the y-rotational axis.
        Vector3 worldPositionFlattened = new Vector3(worldPosition.x, graphicsObject.transform.position.y, worldPosition.z);
        Vector3 directionToOther = (worldPositionFlattened - graphicsObject.transform.position).normalized;
        float dot = Vector3.Dot(graphicsObject.transform.forward, directionToOther);
        if (dot < faceTargetDotThreshold)
        {
            Entropek.UnityUtils.TransformUtils.RotateYAxisToDirection(graphicsObject, directionToOther, faceTargetDirectionSpeed * Time.deltaTime);
            return false;
        }
        return true;
    }


    ///
    /// Animation Events.
    /// 


    private void OnSetGraphicsObjectDirectionBackwardsAnimationEvent(){
        graphicsObject.rotation = Quaternion.LookRotation(-graphicsObject.forward);
    }


    /// 
    /// Linkage.
    /// 


    protected virtual void LinkEvents()
    {
        LinkHealthEvents();
        LinkCombatAgentEvents();
        LinkAnimationEventRecieverEvents();
        LinkAiStateAgentEvents();
    }

    protected virtual void UnlinkEvents()
    {
        UnlinkHealthEvents();
        UnlinkCombatAgentEvents();
        UnlinkAnimationEventRecieverEvents();
        UnlinkAiStateAgentEvents();
    }


    ///
    /// State Agent Linkage.
    /// 


    protected void LinkAiStateAgentEvents()
    {
        if(aiStateAgent != null)
        {
            aiStateAgent.OutcomeChosen += OnStateAgentOutcomeChosenWrapper;
        }
    }

    protected void UnlinkAiStateAgentEvents()
    {
        if(aiStateAgent != null)
        {
            aiStateAgent.OutcomeChosen -= OnStateAgentOutcomeChosenWrapper;
        }
    }
    
    private void OnStateAgentOutcomeChosenWrapper(string outcomeName)
    {
        OnStateAgentOutcomeChosen(outcomeName);
    }

    protected virtual bool OnStateAgentOutcomeChosen(string outcomeName)
    {
        switch (outcomeName)
        {
            case ChaseStateAgentOutcome:
                ChaseState();
                return true;
            case FleeStateAgentOutcome:
                FleeState();
                return false;
            default:
                return false;
        }
    }

    /// 
    /// Combat Agent Linkage.
    /// 


    private void LinkCombatAgentEvents()
    {
        aiActionAgent.OutcomeChosen += OnCombatActionChosenWrapper;
    }

    private void UnlinkCombatAgentEvents()
    {
        aiActionAgent.OutcomeChosen -= OnCombatActionChosenWrapper;
    }

    private void OnCombatActionChosenWrapper(string actionName)
    {
        // always clear state queue when choosing a combat action.
        // a new combat action (best action to choosen in the current scenario)
        // should always overwrite the previous state, otherwise two states may collide
        // and generate undefined behaviour.

        stateQeueue.Clear();

        OnCombatActionChosen(actionName);
    }

    protected virtual bool OnCombatActionChosen(in string actionName)
    {
        return false;
    }

    protected abstract void OnOpponentEngaged(Transform opponent);


    /// 
    /// Health Linkage.
    /// 


    protected virtual void LinkHealthEvents()
    {
        health.Damaged += OnHealthDamaged;
        health.Death += OnHealthDeath;
    }

    protected virtual void UnlinkHealthEvents()
    {
        health.Damaged -= OnHealthDamaged;
        health.Death -= OnHealthDeath;
    }

    protected abstract void OnHealthDeath();

    protected abstract void OnHealthDamaged(DamageContext damageContext);

    /// 
    /// Animation Event Reciever Linkage.
    /// 


    private void LinkAnimationEventRecieverEvents()
    {
        animationEventReciever.AnimationEventTriggered += OnAnimationEventTriggeredWrapper;
    }

    private void UnlinkAnimationEventRecieverEvents()
    {
        animationEventReciever.AnimationEventTriggered -= OnAnimationEventTriggeredWrapper;
    }

    /// <summary>
    /// Wrapper function to ensure compatibility between base Enemy class and children inheritence of
    /// OnAnimationEventTriggered switch cases. AnimationEventTriggered expects return type of void 
    /// where as the children of enemy depend upon a boolean return type for evaluation short-circuiting
    /// by the base class.
    /// </summary>
    /// <param name="eventName"></param>

    private void OnAnimationEventTriggeredWrapper(string eventName)
    {
        OnAnimationEventTriggered(eventName);
    }

    /// <summary>
    /// Callback function for Ai Combat Agent EventTriggered event.
    /// </summary>
    /// <param name="eventName"></param>
    /// <returns>true, if the internal evaluation had an entry linked to event name. otherwise false.</returns>

    protected virtual bool OnAnimationEventTriggered(string eventName)
    {
        switch (eventName)
        {
            case "SetGraphicsObjectDirectionBackwards":
                OnSetGraphicsObjectDirectionBackwardsAnimationEvent();
                return true;

            case "EndAttack":               
                AttackEndedState();                         
                return true;

            default:
                return false;
        }
    }
}