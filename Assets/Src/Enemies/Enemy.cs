using Entropek.Systems.Ai.Combat;
using Entropek.Systems;
using UnityEngine;
using Entropek;
using Entropek.Systems.Combat;
using Entropek.UnityUtils.AnimatorUtils;
using System;
using Entropek.Physics;
using UnityEngine.Timeline;

public abstract class Enemy : MonoBehaviour{
    
    /// 
    /// Components.
    /// 
    

    [Header(nameof(Enemy)+"Required Components")]
    [SerializeField] protected Transform graphicsObject; // gameobject that holds the enemy mesh, vfx, etc.
    [SerializeField] protected Transform target;
    [SerializeField] protected Entropek.EntityStats.HealthSystem health;
    [SerializeField] protected AiCombatAgent combatAgent;
    [SerializeField] protected AttackManager attackManager;
    [SerializeField] protected AnimationEventReciever animationEventReciever;
    [SerializeField] protected NavAgentMovement movement;
    [SerializeField] protected ForceApplier forceApplier;
    [SerializeField] protected Entropek.Audio.AudioPlayer audioPlayer;

    [Header(nameof(Enemy) + "Optional Components")]
    [SerializeField] protected GroundChecker groundChecker;
    private const float FaceMoveDirectionSpeed = 3.33f;

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
            Entropek.UnityUtils.Transform.RotateYAxisToDirection(graphicsObject, movement.moveDirection, FaceMoveDirectionSpeed * Time.deltaTime);
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

    /// 
    /// Linkage.
    /// 


    protected virtual void LinkEvents(){
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

    protected abstract void OnCombatActionChosen(string actionName); 
    protected abstract void OnOpponentEngaged(Transform opponent);


    /// 
    /// Health Linkage.
    /// 


    private void LinkHealthEvents(){
        health.Death += OnHealthDeath;
    }

    private void UnlinkHealthEvents(){
        health.Death -= OnHealthDeath;
    }

    protected abstract void OnHealthDeath();
    

    /// 
    /// Animation Event Reciever Linkage.
    /// 


    private void LinkAnimationEventRecieverEvents(){
        animationEventReciever.AnimationEventTriggered += OnAnimationEventTriggered;
    }

    private void UnlinkAnimationEventRecieverEvents(){
        animationEventReciever.AnimationEventTriggered -= OnAnimationEventTriggered;
    }

    protected abstract void OnAnimationEventTriggered(string eventName);
}
