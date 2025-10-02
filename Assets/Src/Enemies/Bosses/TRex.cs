using System;
using UnityEngine;

public class TRex : Enemy{

    [Header("Components")]
    [SerializeField] Animator animator;
    Transform target;

    private const string BiteAnimation = "Armature_TRex_Attack";
    
    /// 
    /// base.
    /// 

    void FixedUpdate(){
        if(target!=null){
            navAgent.destination = target.position;
        }
    }

    public override void Kill(){
        throw new System.NotImplementedException();
    }

    protected override void OnCombatActionChosen(string actionName){
        switch(actionName){
            case "Bite":
                Bite();
                break;
            default:
                throw new NotImplementedException(actionName);
        }
    }

    protected override void OnHealthDeath(){
        Kill();
    }

    public void Bite(){
        animator.Play(BiteAnimation);
    }

    protected override void OnOpponentEngaged(Transform opponent){
        target = opponent;
    }
}
