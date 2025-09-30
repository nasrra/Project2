using UnityEngine;

public class Enemy : MonoBehaviour{
    [Header("Components")]
    [SerializeField] Health health;

    void OnEnable(){
        LinkEvents();
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
