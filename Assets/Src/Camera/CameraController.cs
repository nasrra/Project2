using System;
using UnityEngine;

public class CameraController : MonoBehaviour{
    
    public event Action Rotated;

    Quaternion previousRotation;    
    Quaternion lookRotationEuler;
    
    [Header("Componentes")]
    [SerializeField] Transform followPoint;  
    
    [Header("Data")]
    private Vector3 desiredPosition;
    private Vector3 desiredDistance;
    private Vector3 desiredDirection;
    private Vector3 lookAtPosition;
    [SerializeField] private Vector3 followOffset;
    [SerializeField] private Vector2 sensitivity;
    [SerializeField] private LayerMask obstructionLayer;

    Vector2 inputRotation;
    
    private const float FOLLOW_SPEED        = 100;
    private const float UPPER_YAW_LIMIT     = 70f;
    private const float LOWER_YAW_LIMIT     = -35f;
    private const float CAMERA_RADIUS       = 0.33f;
    private const float CAMERA_RADIUS_SQRD  = CAMERA_RADIUS * CAMERA_RADIUS;


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
        CalculateDesiredPosition();
        UpdatePosition();
        LookAtTarget();
        CheckRotation();
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

    private void CalculateDesiredPosition(){

        desiredPosition = followPoint.position + lookRotationEuler * followOffset;

        desiredDistance = followPoint.position - desiredPosition;
        desiredDirection = desiredDistance.normalized;

        // check if there are any obstructions along the way.

        if(Physics.SphereCast(
            followPoint.position, 
            CAMERA_RADIUS,
            -desiredDirection, 
            out RaycastHit hit,
            desiredDistance.magnitude, 
            obstructionLayer, 
            QueryTriggerInteraction.Ignore
        )){

            // apply a 'bubble' around the camera so it doesnt clip into walls.

            desiredPosition = hit.point + (desiredDirection * CAMERA_RADIUS) + (hit.normal * CAMERA_RADIUS);
        }
    }

    private void UpdatePosition(){

        // smoothly update position instead of snapping.

        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, FOLLOW_SPEED * Time.deltaTime);
    }

    private void LookAtTarget(){
        Vector3 distanceToFollowPoint = followPoint.position - transform.position;

        
        if(distanceToFollowPoint.sqrMagnitude > CAMERA_RADIUS_SQRD){

            // look at our follow point when the camera is far enough.

            lookAtPosition = followPoint.position;
        }
        else{

            // look at our desired direction when the camera is too close to the target.
            // avoiding camera flipping on itself when up against walls or really close to target.

            lookAtPosition = transform.position + desiredDirection;
        }

        transform.LookAt(lookAtPosition);
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
        inputRotation.y = Mathf.Clamp(inputRotation.y - sensitivity.y * deltaMovement.y, LOWER_YAW_LIMIT, UPPER_YAW_LIMIT);
        lookRotationEuler = Quaternion.Euler(inputRotation.y, inputRotation.x,0);
    }
}
