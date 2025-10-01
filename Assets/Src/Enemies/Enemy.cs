using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour{
    [Header("Components")]
    [SerializeField] Health health;
    [SerializeField] Transform target;
    [SerializeField] NavMeshAgent navAgent;

    void OnEnable(){
        LinkEvents();
    }

    private void FixedUpdate(){
        navAgent.destination = target.position;
    }

    protected virtual void LinkEvents(){
        health.Death += Kill;
    }

    protected virtual void UnlinkEvents(){
        health.Death -= Kill;
    }

    public void Kill(){
        Destroy(gameObject);
    }
}
