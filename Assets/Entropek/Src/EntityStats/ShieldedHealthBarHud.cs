using UnityEngine;
using Entropek.Logger.Exceptions;

namespace Entropek.EntityStats{

[DefaultExecutionOrder(-10)]
public class ShieldedHealthBarHud : MonoBehaviour{
    
    public static ShieldedHealthBarHud Singleton {get;private set;} 

    [Header("Components")]
    [SerializeField] private ShieldedHealthBar shieldedHealthBar;
    public ShieldedHealthBar ShieldedHealthBar => shieldedHealthBar;

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

