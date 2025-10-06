using Entropek.Systems.Ai.Combat;
using Entropek.Systems;
using UnityEngine;
using UnityEngine.AI;
using Entropek.Systems.Combat;
using Entropek.UnityUtils.AnimatorUtils;

public abstract class Enemy : MonoBehaviour{
    
    /// 
    /// Components.
    /// 
    

    [Header(nameof(Enemy)+" Components")]
    [SerializeField] protected HealthSystem health;
    [SerializeField] protected NavMeshAgent navAgent;
    [SerializeField] protected AiCombatAgent combatAgent;
    [SerializeField] protected AttackManager attackManager;
    [SerializeField] protected AnimationEventReciever animationEventReciever;


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
