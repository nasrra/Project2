using System;
using Entropek.Ai.Combat;
using TreeEditor;
using UnityEngine;

public class Slink : Enemy {


    [Header(nameof(Slink) + " Components")]
    [SerializeField] Animator animator;
    [SerializeField] Entropek.UnityUtils.BoneStagger boneStagger;

    [Header("Hitboxes")]
    [SerializeField] Entropek.Combat.Hitbox biteHitbox;

    [Header("Vfx")]
    [SerializeField] Entropek.Vfx.CompositeVfxPlayer biteVfx;

    private const string BiteAnimation = "Bite";

    private event Action FixedUpdateCallback;


    /// 
    /// base.
    /// 

    void Awake()
    {
        ChasingState();
    }

    void Start()
    {
        ChasingState();
    }

    public override void Kill()
    {
        throw new System.NotImplementedException();
    }


    private void FixedUpdate() {
        FixedUpdateCallback?.Invoke();
    }

    ///
    /// State Machine.
    /// 


    private void FaceTargetThenPerformAction(Action action)
    {
        Vector3 cachedTargetPosition = target.position;
        FixedUpdateCallback = () => { FaceWorldPositionThenPerformActionFixedUpdate(cachedTargetPosition, action); };
    }

    private void FaceWorldPositionThenPerformActionFixedUpdate(Vector3 worldPosition, Action action)
    {
        if (FaceWorldSpacePosition(worldPosition) == false)
        {
            return;
        }
        FixedUpdateCallback = null;
        action();
    }

    private void AttackingState()
    {
        FixedUpdateCallback = AttackingStateFixedUpdate;
        movement.PausePath();
    }

    private void AttackingStateFixedUpdate(){
        
    }

    private void ChasingState(){
        FixedUpdateCallback = ChaseStateFixedUpdate;
        movement.ResumePath();
        movement.StartPath(target);
    }

    private void ChaseStateFixedUpdate()
    {  
        RotateGraphicsTransformToGroundNormal();
        FaceMoveDirection();
    }


    /// 
    /// Linkage Override.
    /// 

    protected override void OnCombatActionChosen(AiCombatAction action){
        if (action.TurnToTarget == true)
        {
            switch (action.Name)
            {
                case "Bite":
                    FaceTargetThenPerformAction(BiteAttack);
                    break;
                default:
                    throw new NotImplementedException(action.Name);
            }
        }
        else
        {
            switch (action.Name)
            {
                case "Bite":
                    BiteAttack();
                    break;
                default:
                    throw new NotImplementedException(action.Name);
            }            
        }
    }

    protected override void LinkHealthEvents()
    {
        base.LinkHealthEvents();
        health.HealthDamaged += OnHealthDamaged;
    }

    protected override void UnlinkHealthEvents()
    {
        base.UnlinkHealthEvents();
        health.HealthDamaged -= OnHealthDamaged;
    }

    private void OnHealthDamaged(float amount)
    {
        boneStagger.TriggerStagger();
    }

    protected override void OnHealthDeath()
    {
        Kill();
    }

    public void BiteAttack(){
        animator.Play(BiteAnimation);
        AttackingState();
    }

    protected override void OnOpponentEngaged(Transform opponent){
        target = opponent;
    }

    protected override bool OnAnimationEventTriggered(string eventName){
        
        if (base.OnAnimationEventTriggered(eventName) == true)
        {
            return true;
        }
        
        switch(eventName){
            case "Footstep":
                audioPlayer.PlaySound("FootstepGrass", transform.position);
                return true;
            case "BiteGrowl":
                audioPlayer.PlaySound("SlinkGrowl", gameObject);
                return true;
            case "BiteAttack":
                biteHitbox.Enable();
                biteVfx.Play();
                audioPlayer.PlaySound("SlinkBite", gameObject);
                return true;
            case "BiteLunge": forceApplier.ImpulseRelativeToGround(graphicsObject.forward, 24, 36); return true;
            case "EndAttack": ChasingState(); combatAgent.Begin(); return true;
            default: throw new Exception("Animation Event Not Implemented "+eventName);
        }
    }
}
