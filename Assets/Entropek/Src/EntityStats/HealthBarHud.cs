using UnityEngine;

namespace Entropek.EntityStats{

[DefaultExecutionOrder(-10)]
public class HealthBarHud : MonoBehaviour{
    
    public static HealthBarHud Singleton {get;private set;} 

    [Header("Components")]
    [SerializeField] private HealthBar healthBar;
    public HealthBar HealthBar => healthBar;

    void OnEnable(){
        if(Singleton!=null){
            throw new Exceptions.SingletonException();
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

