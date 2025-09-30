using UnityEngine;

public class LockOnTargetTest : MonoBehaviour
{

    [SerializeField] private new CameraController camera;
    [SerializeField] private Transform lockOnTarget;

    void OnEnable(){
        InputManager.Singleton.LockOnToggle += Toggle;
    }

    void OnDisable(){
        InputManager.Singleton.LockOnToggle -= Toggle;        
    }

    bool toggled;
    void Toggle(){
        if(toggled==false){
            camera.SetLockOnTarget(lockOnTarget);
        }
        else{
            camera.SetLockOnTarget(null);
        }
        toggled=!toggled;
    }
}
