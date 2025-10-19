using System;
using TreeEditor;
using UnityEngine;

public class TRex : Enemy{


    [Header("Components")]
    [SerializeField] Animator animator;

    private const string BiteAnimation = "Bite";
    private const int BiteAttackId = 0;

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


    protected override void OnHealthDeath(){
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
            case "BiteAttack":  
                attackManager.BeginAttack(BiteAttackId); 
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
