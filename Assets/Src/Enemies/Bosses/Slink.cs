using System;
using Entropek.Ai.Combat;
using TreeEditor;
using UnityEngine;

public class Slink : Enemy {


    private enum State : byte
    {
        Idle,
        Chase,
        Attack
    }


    [Header(nameof(Slink) + " Components")]
    [SerializeField] Animator animator;
    [SerializeField] Entropek.UnityUtils.BoneStagger boneStagger;

    [Header("Hitboxes")]
    [SerializeField] Entropek.Combat.Hitbox biteHitbox;

    [Header("Vfx")]
    [SerializeField] Entropek.Vfx.CompositeVfxPlayer biteVfx;

    private const string BiteAnimation = "Bite";
    private const string TailSwipeAnimation = "TailSwipe";
    private const string IdleAnimation = "Idle";
    private const string ChaseAnimation = "Walk";

    private event Action FixedUpdateCallback;


    /// 
    /// base.
    /// 

    void Awake()
    {
        ChaseState();
    }

    void Start()
    {
        ChaseState();
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

    public override void IdleState()
    {
        FixedUpdateCallback = null;
        animator.Play(IdleAnimation);
    }

    public override void IdleState(float time)
    {
        IdleState();
        stateQeueue.Begin(time);
    }

    public override void AttackState()
    {
        FixedUpdateCallback = AttackingStateFixedUpdate;
        movement.PausePath();
    }

    private void AttackingStateFixedUpdate(){
        
    }

    public override void ChaseState(){
        FixedUpdateCallback = ChaseStateFixedUpdate;
        combatAgent.Begin();
        movement.ResumePath();
        movement.StartPath(target);
        animator.Play(ChaseAnimation);
    }

    private void ChaseStateFixedUpdate()
    {
        RotateGraphicsTransformToGroundNormal();
        FaceMoveDirection();
    }

    private void AttackEndedState()
    {
        // get the attack that has just been completed. 

        AiCombatAction endedAttack = combatAgent.ChosenCombatAction;

        // evaulate for a new action immediately up time out if set to true.

        if (endedAttack.EvaluateOnIdleTimeout == true)
        {
            stateQeueue.Enqueue(combatAgent.Evaluate);
        }

        // go back to chasing when idle times out.

        stateQeueue.Enqueue(ChaseState);
        
        // start idle state.

        IdleState(endedAttack.IdleTime);
    }


    /// 
    /// Linkage Override.
    /// 


    protected override void OnCombatActionChosen(AiCombatAction action)
    {
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
                case "TailSwipe":
                    TailSwipeAttack();
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

    public void BiteAttack()
    {
        animator.Play(BiteAnimation);
        AttackState();
    }

    public void TailSwipeAttack()
    {
        animator.Play(TailSwipeAnimation);
        AttackState();
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
            case "EndAttack":  AttackEndedState();  return true;
            default: throw new Exception("Animation Event Not Implemented "+eventName);
        }
    }
}
