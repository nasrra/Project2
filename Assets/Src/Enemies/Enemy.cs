using Entropek.Systems.Ai.Combat;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour{
    [Header("Enemy Components")]
    [SerializeField] protected Health health;
    [SerializeField] protected NavMeshAgent navAgent;
    [SerializeField] protected AiCombatAgent combatAgent;

    void OnEnable(){
        LinkEvents();
    }

    void OnDisable(){
        UnlinkEvents();
    }

    protected virtual void LinkEvents(){
        LinkHealthEvents();
        LinkCombatAgentEvents();
    }

    protected virtual void UnlinkEvents(){
        UnlinkHealthEvents();
        UnlinkCombatAgentEvents();
    }

    private void LinkCombatAgentEvents(){
        combatAgent.ActionChosen += OnCombatActionChosen;
        combatAgent.EngagedOpponent += OnOpponentEngaged;
    }

    private void UnlinkCombatAgentEvents(){
        combatAgent.ActionChosen -= OnCombatActionChosen;
        combatAgent.EngagedOpponent -= OnOpponentEngaged;        
    }

    private void LinkHealthEvents(){
        health.Death += OnHealthDeath;
    }

    private void UnlinkHealthEvents(){
        health.Death -= OnHealthDeath;
    }

    protected abstract void OnCombatActionChosen(string actionName); 
    protected abstract void OnOpponentEngaged(Transform opponent);
    protected abstract void OnHealthDeath();
    public abstract void Kill();
}
