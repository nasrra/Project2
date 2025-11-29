using UnityEngine;
using Entropek.Combat;
using Entropek.Physics;
using Entropek.Time;
using UnityEditor.Experimental.GraphView;
using Entropek.EntityStats;

public abstract class Enemy : MonoBehaviour 
{


    /// 
    /// Required Components.
    /// 


    [Header(nameof(Enemy) + " Required Components")]
    [SerializeField] protected Transform graphicsObject; // gameobject that holds the enemy mesh, vfx, etc.
    [SerializeField] protected NavAgentMovementTarget navAgentMovementTarget;
    [SerializeField] protected Transform target;
    
    [SerializeField] protected HealthSystem health;
    public HealthSystem Health => health;

    [SerializeField] protected Animator animator;
    [SerializeField] protected Entropek.Ai.AiActionAgent combatAgent;
    [SerializeField] protected Entropek.UnityUtils.AnimatorUtils.AnimationEventReciever animationEventReciever;
    [SerializeField] protected NavAgentMovement navAgentMovement;
    public NavAgentMovement NavAgentMovement =>  navAgentMovement;
    [SerializeField] protected Entropek.Audio.AudioPlayer audioPlayer;
    [SerializeField] protected TimedActionQueue stateQeueue;


    /// 
    /// Optional Components.
    /// 


    [Header(nameof(Enemy) + " Optional Components")]
    [SerializeField] protected GroundCheck groundChecker;


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

        // immediately start tracking the player.

        Entropek.Ai.Contexts.AiAgentContext combatContext = combatAgent.AiAgentContext;

        target = Opponent.Singleton.transform;
        Debug.Log(Opponent.Singleton);
        navAgentMovementTarget = Opponent.Singleton.NavAgentMovementTarget;

        switch (combatContext)
        {
            
            // check IOpponentContext first as it implements the target context;

            case Entropek.Ai.Contexts.IOpponentContext opponentContext:
                opponentContext.Target = Opponent.Singleton.transform;
                opponentContext.HealthSystem = Opponent.Singleton.HealthSystem;
            break;
            case Entropek.Ai.Contexts.ITargetContext targetContext:
                targetContext.Target = Opponent.Singleton.transform;
            break;
        }

    }

    void OnDestroy()
    {
        UnlinkEvents();
    }

    ///
    /// State Machine.
    /// 

    public abstract void IdleState();
    public abstract void IdleState(float time);
    public abstract void ChaseState();
    public abstract void AttackState();

    protected virtual void AttackEndedState()
    {
        combatAgent.BeginChosenActionCooldown();

        // get the attack that has just been completed. 

        Entropek.Ai.AiAction endedAttack = combatAgent.ChosenAction;

        // evaulate for a new action immediately up time out if set to true.

        if (endedAttack.EvaluateOnIdleTimeout == true)
        {
            if (combatAgent.Evaluate() == true)
            {
                return;
            }
        }

        // start idle state.

        IdleState(endedAttack.IdleTime);
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
            Entropek.UnityUtils.Transform.RotateYAxisToDirection(graphicsObject, navAgentMovement.moveDirection, faceMoveDirectionSpeed * Time.deltaTime);
        }
    }

    protected void RotateGraphicsTransformToGroundNormal()
    {

        // Get the rotation needed to align up with the ground.

        Quaternion alignToGround = Quaternion.FromToRotation(graphicsObject.up, groundChecker.GroundNormal) * graphicsObject.rotation;

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
            Entropek.UnityUtils.Transform.RotateYAxisToDirection(graphicsObject, directionToOther, faceTargetDirectionSpeed * Time.deltaTime);
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
    }

    protected virtual void UnlinkEvents()
    {
        UnlinkHealthEvents();
        UnlinkCombatAgentEvents();
        UnlinkAnimationEventRecieverEvents();
    }


    /// 
    /// Combat Agent Linkage.
    /// 


    private void LinkCombatAgentEvents()
    {
        combatAgent.OutcomeChosen += OnCombatActionChosenWrapper;
    }

    private void UnlinkCombatAgentEvents()
    {
        combatAgent.OutcomeChosen -= OnCombatActionChosenWrapper;
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

    protected abstract void OnCombatActionChosen(in string actionName);
    protected abstract void OnOpponentEngaged(Transform opponent);


    /// 
    /// Health Linkage.
    /// 


    protected virtual void LinkHealthEvents()
    {
        health.Death += OnHealthDeath;
        health.Damaged += OnHealthDamaged;
    }

    protected virtual void UnlinkHealthEvents()
    {
        health.Death -= OnHealthDeath;
        health.Damaged -= OnHealthDamaged;
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