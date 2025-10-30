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
    [SerializeField] Entropek.Combat.Hitbox tailHitbox;
    [SerializeField] Entropek.Combat.Hitbox biteLungeHitbox;
    [SerializeField] Entropek.Combat.Hitbox clawSlashHitbox;

    [Header("Vfx")]
    [SerializeField] Entropek.Vfx.CompositeVfxPlayer biteVfx;
    [SerializeField] Entropek.Vfx.SingleVfxPlayer tailSwipeVfx;
    [SerializeField] Entropek.Vfx.CompositeVfxPlayer clawSlashVfx;

    private const string BiteAnimation = "Bite";
    private const string TailSwipeAnimation = "TailSwipe";
    private const string IdleAnimation = "Idle";
    private const string ChaseAnimation = "Walk";
    private const string DashForwardAnimation = "DashForward";
    private const string ClawSlashAnimation = "ClawSlash";

    private const float BiteLungeForce = 24;
    private const float BiteLungeDecaySpeed = 36;
    private const float DashForwardLungeForce = 36;
    private const float DashForwardLungeDecay = 60;

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
        throw new NotImplementedException();
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
        combatAgent.BeginEvaluationLoop();
        movement.ResumePath();
        movement.StartPath(target);
        animator.CrossFade(ChaseAnimation, 0.167f);
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
    /// Bite Attack.
    /// 


    public void BiteAttack()
    {
        animator.Play(BiteAnimation);
        AttackState();
    }

    private void OnBiteLungeAnimationEvent()
    {
        forceApplier.ImpulseRelativeToGround(graphicsObject.forward, BiteLungeForce, BiteLungeDecaySpeed);
    }

    private void OnBiteAttackFrameAnimationEvent()
    {
        biteHitbox.Enable();
        biteVfx.Play();
        audioPlayer.PlaySound("SlinkBite", gameObject);
    }

    private void OnBiteLungeAttackFrameAnimationEvent()
    {
        biteLungeHitbox.Enable();
    }

    /// 
    /// Tail Swipe Attack.
    /// 


    public void TailSwipeAttack()
    {
        animator.Play(TailSwipeAnimation);
        AttackState();
    }

    private void OnTailSwipeAttackFrameAnimationEvent()
    {
        tailSwipeVfx.Play();
        tailHitbox.Enable();
        audioPlayer.PlaySound("SlinkTailSwipe", gameObject);
    }

    private void OnTailSwipeGrowlAnimationEvent()
    {
        audioPlayer.PlaySound("SlinkGrowl", gameObject);
    }


    ///
    /// Dash Forward.
    /// 

    public void DashForward()
    {
        animator.Play(DashForwardAnimation);
        AttackState();
    }

    private void OnDashForwardLungeAnimationEvent()
    {
        forceApplier.ImpulseRelativeToGround(graphicsObject.forward, DashForwardLungeForce, DashForwardLungeDecay);
    }

    ///
    /// Claw Slash
    /// 

    private void ClawSlash()
    {
        animator.Play(ClawSlashAnimation);
        AttackState();
    }

    private void OnClawSlashAttackFrameAnimationEvent()
    {
        audioPlayer.PlaySound("SlinkClawSlash", gameObject);
        clawSlashHitbox.Enable();
        clawSlashVfx.Play();
    }

    ///
    /// Test Swivel.
    /// 

    private void TestSwivel()
    {
        animator.Play("TestSwivel");
        AttackState();
    }

    /// 
    /// Generic Animation Events.
    /// 


    private void OnFootstepAnimationEvent()
    {
        audioPlayer.PlaySound("FootstepGrassMedium", transform.position);
    }

    private void OnWingFlapAnimationEvent()
    {
        audioPlayer.PlaySound("SmallWingsFlap", gameObject);
    }

    private void OnStartGrowlAnimationEvent()
    {        
        audioPlayer.PlaySound("SlinkGrowl", gameObject);
    }

    private void OnStopGrowlAnimationEvent()
    {
        audioPlayer.StopSound("SlinkGrowl", immediate: false);
    }



    /// 
    /// Linkage Override.
    /// 


    protected override void OnCombatActionChosen(AiCombatAction action)
    {
        Action actionCallback;

        switch (action.Name)
        {
            case "Bite":        actionCallback = BiteAttack;       break;
            case "TailSwipe":   actionCallback = TailSwipeAttack;  break;
            case "TestSwivel":  actionCallback = TestSwivel;       break;
            case "DashForward": actionCallback = DashForward;      break;
            case "ClawSlash":   actionCallback = ClawSlash;        break;
            default: throw new NotImplementedException(action.Name);
        }

        if(action.TurnToTarget == true)
        {
            FaceTargetThenPerformAction(actionCallback);
        }
        else
        {
            actionCallback();
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

    protected override void OnOpponentEngaged(Transform opponent){
        target = opponent;
    }

    protected override bool OnAnimationEventTriggered(string eventName){
        
        if (base.OnAnimationEventTriggered(eventName) == true)
        {
            return true;
        }
        
        switch(eventName){
            
            // Generic Animations events.
            
            case "Footstep":                OnFootstepAnimationEvent();                 return true;
            case "EndAttack":               AttackEndedState();                         return true;
            case "StartGrowl":              OnStartGrowlAnimationEvent();               return true;
            case "StopGrowl":               OnStopGrowlAnimationEvent();                return true;
            case "WingFlap":                OnWingFlapAnimationEvent();                 return true;

            // Bite Animation Events.

            case "BiteLungeAttackFrame":    OnBiteLungeAttackFrameAnimationEvent();     return true;
            case "BiteAttackFrame":         OnBiteAttackFrameAnimationEvent();          return true;
            case "BiteLunge":               OnBiteLungeAnimationEvent();                return true;
            
            // Tail Swipe Animation Events.

            case "TailSwipeAttackFrame":    OnTailSwipeAttackFrameAnimationEvent();     return true;

            // Dash Forwards Animation Events.

            case "DashForwardLunge":        OnDashForwardLungeAnimationEvent();         return true;

            // Claw Slash Animation Events.
            case "ClawSlashAttackFrame":    OnClawSlashAttackFrameAnimationEvent();     return true;

            default: throw new InvalidOperationException("Animation Event Not Implemented "+eventName);
        }
    }
}
