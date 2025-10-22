using System;
using TreeEditor;
using UnityEngine;

public class Slink : Enemy{


    [Header(nameof(Slink)+" Components")]
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

    protected override void OnCombatActionChosen(string actionName){
        switch(actionName){
            case "Bite":
                BiteAttack();
                break;
            default:
                throw new NotImplementedException(actionName);
        }
    }

    private void FixedUpdate(){
        FixedUpdateCallback?.Invoke();
    }

    private void LateUpdate()
    {
        RotateGraphicsTransformToGroundNormal();
        FaceMoveDirection();
    }

    ///
    /// State Machine.
    /// 


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
        
    }


    /// 
    /// Linkage Override.
    /// 

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

    protected override void OnAnimationEventTriggered(string eventName){
        switch(eventName){
            case "Footstep":
                audioPlayer.PlaySound("FootstepGrass", transform.position);
                break;
            case "BiteGrowl":
                audioPlayer.PlaySound("SlinkGrowl", gameObject);
                break;
            case "BiteAttack":
                biteHitbox.Enable();
                biteVfx.Play();
                audioPlayer.PlaySound("SlinkBite", gameObject);
                break;
            case "BiteLunge":
                forceApplier.ImpulseRelativeToGround(graphicsObject.forward, 24, 36);
                break;
            case "EndAttack":   
                ChasingState(); 
                break;
            default:            throw new Exception("Animation Event Not Implemented "+eventName);
        }
    }
}
