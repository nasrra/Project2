using System;
using UnityEngine;

public class CameraController : MonoBehaviour{
    
    public event Action Rotated;

    Quaternion previousRotation;    
    Quaternion lookRotationEuler;
    
    [SerializeField] Transform followPoint;  
    
    [SerializeField] private Vector2 sensitivity;
    [SerializeField] private Vector3 followOffset;
    
    Vector2 inputRotation;
    
    private const float YAW_LIMIT = 75f;


    /// 
    /// Base.
    /// 


    private void OnEnable(){
        Link();
    }

    private void OnDisable(){
        Unlink();
    }

    private void LateUpdate(){
        CheckRotation();
        UpdatePosition();
        LookAtTarget();
    }


    /// 
    /// Functions.
    /// 


    private void OnRotated(){
        Rotated?.Invoke();
    }

    public void SetFollowOffset(Vector3 followOffset){
        this.followOffset = followOffset;
    }

    private void CheckRotation(){
        if(transform.rotation != previousRotation){
            OnRotated();
        }
        previousRotation = transform.rotation;
    }

    private void UpdatePosition(){
        Vector3 desiredPosition = followPoint.position + lookRotationEuler * followOffset;
        transform.position = desiredPosition;
    }

    private void LookAtTarget(){
        transform.LookAt(followPoint);
    }


    /// 
    /// Linkage.
    /// 


    private void Link(){
        InputManager.Singleton.Look += OnLook;
    }

    private void Unlink(){
        InputManager.Singleton.Look -= OnLook;
    }

    private void OnLook(Vector2 deltaMovement){
        inputRotation.x += sensitivity.x * deltaMovement.x;
        inputRotation.y = Mathf.Clamp(inputRotation.y - sensitivity.y * deltaMovement.y, -YAW_LIMIT, YAW_LIMIT);
        lookRotationEuler = Quaternion.Euler(inputRotation.y, inputRotation.x,0);
        // pivotPoint.transform.rotation = Quaternion.Euler(inputRotation.y, inputRotation.x,0);
    }
}
