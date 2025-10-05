using System;
using System.Collections.Generic;
using Entropek.Systems.Autoload;
using UnityEngine;

namespace Entropek.Systems.Combat{
    
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Timer))]
public class Hitbox : MonoBehaviour{
    
    public event Action<GameObject> Hit;
    
    [Header("Components")]
    [SerializeField] private Collider col;
    [SerializeField] private Timer timer;
    public Timer Timer => timer;

    [Header("Data")]
    private HashSet<int> hitGameObjectInstanceIds = new HashSet<int>(); 


    /// 
    /// Base.
    /// 


    void OnEnable(){
        Link();
    }

    void OnDisable(){
        Unlink();
    }

    void OnTriggerEnter(Collider other){
        
        int otherId = other.GetInstanceID();
        
        // short-circuit if we've already hit the object.
        
        if(hitGameObjectInstanceIds.Contains(otherId)==true){
            return;
        }

        GameObject otherGameObject = other.gameObject;

        hitGameObjectInstanceIds.Add(otherId);
        Hit?.Invoke(otherGameObject);
    }

    /// 
    /// Functions.
    /// 


    public void Enable(){
        col.enabled = true;
        timer.Begin();
    }

    public void Enable(float time){
        col.enabled = true;
        timer.Begin(time);
    }


    /// 
    /// Linkage.
    /// 


    private void Link(){
        timer.Timeout += OnTimerTimeout;
    }

    private void Unlink(){
        timer.Timeout -= OnTimerTimeout;
    }

    private void OnTimerTimeout(){
        col.enabled = false;
        hitGameObjectInstanceIds.Clear();
    }
}

}
