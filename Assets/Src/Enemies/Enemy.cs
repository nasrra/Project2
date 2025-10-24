using Entropek.Systems;
using UnityEngine;
using Entropek;
using Entropek.UnityUtils.AnimatorUtils;
using System;
using Entropek.Physics;
using UnityEngine.Timeline;
using Unity.VisualScripting;

public abstract class Enemy : MonoBehaviour{
    

    /// 
    /// Required Components.
    /// 
    

    [Header(nameof(Enemy)+" Required Components")]
    [SerializeField] protected Transform graphicsObject; // gameobject that holds the enemy mesh, vfx, etc.
    [SerializeField] protected Transform target;
    [SerializeField] protected Entropek.EntityStats.HealthSystem health;
    [SerializeField] protected Entropek.Ai.Combat.AiCombatAgent combatAgent;
    [SerializeField] protected AnimationEventReciever animationEventReciever;
    [SerializeField] protected NavAgentMovement movement;
    [SerializeField] protected ForceApplier forceApplier;
    [SerializeField] protected Entropek.Audio.AudioPlayer audioPlayer;


    /// 
    /// Optional Components.
    /// 


    [Header(nameof(Enemy) + " Optional Components")]
    [SerializeField] protected GroundChecker groundChecker;


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


    void OnEnable(){
        LinkEvents();
    }

    void OnDisable(){
        UnlinkEvents();
    }

    /// 
    /// Functions. 
    /// 


    public abstract void Kill();
    
    protected void FaceMoveDirection(){
        Vector3 moveDirection = movement.moveDirection;
        if(moveDirection != Vector3.zero){
            Entropek.UnityUtils.Transform.RotateYAxisToDirection(graphicsObject, movement.moveDirection, faceMoveDirectionSpeed * Time.deltaTime);
        }
    }

    protected void RotateGraphicsTransformToGroundNormal(){

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
    /// Linkage.
    /// 


    protected virtual void LinkEvents()
    {
        LinkHealthEvents();
        LinkCombatAgentEvents();
        LinkAnimationEventRecieverEvents();
    }

    protected virtual void UnlinkEvents(){
        UnlinkHealthEvents();
        UnlinkCombatAgentEvents();
        UnlinkAnimationEventRecieverEvents();
    }


    /// 
    /// Combat Agent Linkage.
    /// 


    private void LinkCombatAgentEvents(){
        combatAgent.ActionChosen += OnCombatActionChosen;
        combatAgent.EngagedOpponent += OnOpponentEngaged;
    }

    private void UnlinkCombatAgentEvents(){
        combatAgent.ActionChosen -= OnCombatActionChosen;
        combatAgent.EngagedOpponent -= OnOpponentEngaged;        
    }

    protected abstract void OnCombatActionChosen(Entropek.Ai.Combat.AiCombatAction action); 
    protected abstract void OnOpponentEngaged(Transform opponent);


    /// 
    /// Health Linkage.
    /// 


    protected virtual void LinkHealthEvents(){
        health.Death += OnHealthDeath;
    }

    protected virtual void UnlinkHealthEvents(){
        health.Death -= OnHealthDeath;
    }

    protected abstract void OnHealthDeath();
    

    /// 
    /// Animation Event Reciever Linkage.
    /// 


    private void LinkAnimationEventRecieverEvents(){
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
            case "StartCombatActionCooldown":
                combatAgent.BeginChosenCombatActionCooldown();
                return true;
            default:
                return false;
        }
    }
}
