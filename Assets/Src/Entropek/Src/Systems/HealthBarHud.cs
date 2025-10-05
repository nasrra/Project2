using UnityEngine;
using Entropek.Logger;
using Entropek.Logger.Exceptions;
using System;

namespace Entropek.Systems{

[DefaultExecutionOrder(-10)]
public class HealthBarHud : MonoBehaviour{
    
    public static HealthBarHud Singleton {get;private set;} 

    [Header("Components")]
    [SerializeField] private HealthBar healthBar;
    public HealthBar HealthBar => healthBar;

    void OnEnable(){
        if(Singleton!=null){
            throw new SingletonException();
        }
        else{
            Singleton = this;
        }
    }

    void OnDisable(){
        Singleton = null;
    }
}


}

