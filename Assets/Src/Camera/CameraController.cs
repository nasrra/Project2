using System;
using Entropek.Time;
using UnityEngine;

public class CameraController : MonoBehaviour{
    
    private event Action Shake;
    public event Action Rotated;

    Quaternion previousRotation;    
    Quaternion lookRotation;
    
    [Header("Componentes")]
    [SerializeField] private Transform shaker;
    [SerializeField] private Transform followTarget;
    [SerializeField] private Transform lockOnTarget;  
    [SerializeField] private Timer shakeTimer;
    [SerializeField] private LockOnTargetDetector lockOnTargetDetector; // this must be a separate game object childed to this one.


    [Header("Data")]
    private Vector3 smoothedfollowTargetPosition;
    private Vector3 desiredWorldPosition; // the next position in world space the camera will take.
    private Vector3 desiredFollowDirection;
    private Vector3 nextShakerLocalPosition;
    private Vector3 targetShakerOffset;
    [SerializeField] private Vector3 followOffset;
    [SerializeField] private Vector2 sensitivity;
    [SerializeField] private LayerMask obstructionLayer;
    private float shakeStrength;

    private const float FollowSpeed             = 13.33f;
    private const float MouseInputSmoothSpeed   = 100f;
    private const float LockOnTargetSmoothSpeed = 16.7f;
    private const float UpperPitchLimit         = 70f;
    private const float LowerPitchLimit         = -45f;
    private const float CameraRadius            = 0.33f;
    private const float LookAtLockOnTargetSmoothSpeed = 6.66f;


    /// 
    /// Base.
    /// 


    private void OnEnable(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetFollowTarget(followTarget);

        LinkEvents();
    }

    private void OnDisable(){
        UnlinkEvents();
    }

    private void LateUpdate(){
        
        CalculateSmoothedFollowTargetPosition();
        CalculateDesiredWorldPosition();
        UpdateFollowPosition();

        LookAtTarget();
        
        CalculateDesiredShakerPosition();
        UpdateShakerPosition();
        
        CheckRotation();

        UpdateLockOnTargetDetector();
    }

    private void FixedUpdate(){
        Shake?.Invoke();
    }


    /// 
    /// Setters.
    /// 


    public void SetFollowOffset(Vector3 followOffset){
        this.followOffset = followOffset;
    }

    public void SetFollowTarget(Transform followTarget){
        this.followTarget = followTarget;
        smoothedfollowTargetPosition = followTarget.position; 
    }

    public void SetLockOnTarget(Transform lockOnTarget){
        this.lockOnTarget = lockOnTarget;
    }


    /// 
    /// World Position setting.
    /// 


    private void CalculateSmoothedFollowTargetPosition(){
        smoothedfollowTargetPosition = Vector3.Lerp(smoothedfollowTargetPosition, followTarget.position, FollowSpeed * Time.deltaTime);
    }


    private void CalculateDesiredWorldPosition(){
        Vector3 offset = Vector3.zero;
        
        if(lockOnTarget!=null){
            
            // offset the camera in relation to the lock on target.
            // otherwise offset the camera in relation to the mouse input.

            Vector3 directionToLockOnTarget = (lockOnTarget.position - transform.position).normalized;
            
            lookRotation = Quaternion.LookRotation(directionToLockOnTarget, Vector3.up); 
            
        }        

        offset = lookRotation * followOffset;

        desiredWorldPosition    = smoothedfollowTargetPosition + offset; // apply the player camera input as positional offset.
        Vector3 distance        = smoothedfollowTargetPosition - desiredWorldPosition;
        desiredFollowDirection  = distance.normalized;

        // check if there are any obstructions along the way.

        if(Physics.SphereCast(
            smoothedfollowTargetPosition, 
            CameraRadius,
            -desiredFollowDirection, 
            out RaycastHit hit,
            distance.magnitude, 
            obstructionLayer, 
            QueryTriggerInteraction.Ignore
        )){

            // apply a 'bubble' around the camera so it doesnt clip into walls.

            desiredWorldPosition = hit.point + (desiredFollowDirection * CameraRadius) + (hit.normal * CameraRadius);
        }
    }

    private void UpdateFollowPosition(){
        
        Vector3 deltaPosition;

        if(lockOnTarget!=null){
            deltaPosition = Vector3.Slerp(transform.position, desiredWorldPosition, LockOnTargetSmoothSpeed * Time.deltaTime);
        }
        else{
            deltaPosition = Vector3.MoveTowards(transform.position, desiredWorldPosition, MouseInputSmoothSpeed * Time.deltaTime);
        }

        // if we are currently not shaking.

        transform.position = deltaPosition;
    }


    /// 
    /// Look At Target Functionality.
    /// 


    private void LookAtTarget(){

        // look at the direction that the camera is pointing in relation to players input
        // rather than tracking the target so the camera doesnt bug out when the target is
        // too close to the player; as well as making sure that shaking the camera doesnt
        // track the player.

        if(lockOnTarget!=null){
            
            // smoothly lerp to lock on target.
            
            transform.rotation = 
                Quaternion.Slerp(
                    transform.rotation, 
                    Quaternion.LookRotation(desiredFollowDirection, Vector3.up), 
                    LookAtLockOnTargetSmoothSpeed * Time.deltaTime
                );
        }
        else{
                            
            // snap to input for responsiveness.

            transform.rotation = Quaternion.LookRotation(desiredFollowDirection, Vector3.up);            
        }
    }

    private void CheckRotation(){
        if(transform.rotation.Equals(previousRotation)==false){
            OnRotated();
        }
        previousRotation = transform.rotation;
    }

    private void OnRotated(){
        Rotated?.Invoke();
    }

    private void UpdateLockOnTargetDetector(){
        lockOnTargetDetector.transform.position = followTarget.position; 
    }


    /// 
    /// Camera Shake Functionality.
    /// 


    private void CalculateDesiredShakerPosition(){

        // smoothly update position instead of snapping

        nextShakerLocalPosition = Vector3.MoveTowards(shaker.localPosition, targetShakerOffset, Time.deltaTime * shakeStrength);

        // short-circuit if we have returned to default state.

        if (nextShakerLocalPosition == Vector3.zero && shaker.localPosition == Vector3.zero)
            return;

        // clamp shake strength so it doesn't overshoot

        nextShakerLocalPosition.x = Mathf.Clamp(nextShakerLocalPosition.x, -shakeStrength, shakeStrength);
        nextShakerLocalPosition.y = Mathf.Clamp(nextShakerLocalPosition.y, -shakeStrength, shakeStrength);
        nextShakerLocalPosition.z = Mathf.Clamp(nextShakerLocalPosition.z, -shakeStrength, shakeStrength);

        // convert to world space

        Vector3 nextShakerWorldPos = transform.TransformPoint(nextShakerLocalPosition);
        Vector3 worldDir = (nextShakerWorldPos - transform.position).normalized;
        float worldDist = Vector3.Distance(transform.position, nextShakerWorldPos);

        // check for obstruction
        if (Physics.SphereCast(
            transform.position,
            CameraRadius,
            worldDir,
            out RaycastHit hit,
            worldDist,
            obstructionLayer,
            QueryTriggerInteraction.Ignore
        )){

            // push the shaker out so it doesn’t clip into walls
            Vector3 correctedWorldPos = hit.point - (worldDir * CameraRadius) + (hit.normal * CameraRadius);
            nextShakerLocalPosition = transform.InverseTransformPoint(correctedWorldPos);
        }
    }

    private void UpdateShakerPosition(){
        shaker.localPosition = nextShakerLocalPosition;
    }

    public void StartShaking(float strength){
        Shake = ShakeFixedUpdate;
        shakeStrength = strength;
        shakeTimer.Loop = true;
        shakeTimer.Begin(int.MaxValue); // max value so it doesnt call loop increbily often.
    }

    public void StartShaking(float strength, float time){
        Shake = ShakeFixedUpdate;
        shakeStrength = strength;
        shakeTimer.Loop = false;
        shakeTimer.Begin(time);
    }

    private void StopShaking(){
        shakeTimer.Halt();
        Shake = null;
        targetShakerOffset = Vector3.zero;
    }

    private void ShakeFixedUpdate(){
        targetShakerOffset = new Vector3(
            UnityEngine.Random.Range(-shakeStrength, shakeStrength),
            UnityEngine.Random.Range(-shakeStrength, shakeStrength),
            UnityEngine.Random.Range(-shakeStrength, shakeStrength)
        );
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        LinkTimerEvents();
        LinkInputEvents();
        LinkLockOnTargetDetectorEvents();
    }


    private void UnlinkEvents(){
        UnlinkTimerEvents();
        UnlinkInputEvents();
        UnlinkLockOnTargetDetectorEvents();
    }


    ///
    /// Lock On Target Detector Linkage.
    /// 

    
    private void LinkLockOnTargetDetectorEvents(){
        lockOnTargetDetector.TargetLeftRange += OnLockOnTargetLeftRange;
    }

    private void UnlinkLockOnTargetDetectorEvents(){
        lockOnTargetDetector.TargetLeftRange -= OnLockOnTargetLeftRange;
    }

    private void OnLockOnTargetLeftRange(){
        lockOnTarget = null;
    }


    ///
    /// Timer Linkage.
    /// 


    private void LinkTimerEvents(){
        shakeTimer.Halted += OnShakeTimerHalted;
    }

    private void UnlinkTimerEvents(){
        shakeTimer.Halted -= OnShakeTimerHalted;
    }

    private void OnShakeTimerHalted(){
        StopShaking();
    }


    /// 
    /// Input Linkage.
    /// 


    private void LinkInputEvents(){
        
        InputManager input = InputManager.Singleton; 
        
        input.Look              += OnLook;
        input.LockOnToggle      += OnLockOnToggle;
        input.LockOnPrevious    += OnLockOnPrevious;
        input.LockOnNext        += OnLockOnNext;
    }

    private void UnlinkInputEvents(){

        InputManager input = InputManager.Singleton; 
        
        input.Look              -= OnLook;
        input.LockOnToggle      -= OnLockOnToggle;
        input.LockOnPrevious    -= OnLockOnPrevious;
        input.LockOnNext        -= OnLockOnNext;
    }

    private void OnLook(Vector2 deltaMovement){
        deltaMovement *= sensitivity;
        Vector3 eulerAngles = lookRotation.eulerAngles;

        // Convert pitch (x) to -180..180 range
        // as Unity never gives angles from -180..180,
        // always 0-360.

        float pitch = eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        // Apply delta and clamp

        pitch = Mathf.Clamp(pitch - deltaMovement.y, LowerPitchLimit, UpperPitchLimit);

        // apply yaw.

        float yaw = eulerAngles.y + deltaMovement.x;

        // set the new rotation.

        lookRotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void OnLockOnToggle(){
        if(lockOnTarget==null){
            
            // get a new target in front of the camera.
            
            Transform newLockOnTarget = lockOnTargetDetector.GetTarget(transform.forward);
            
            if(newLockOnTarget!=null){

                // if a target was found: lock onto it.

                SetLockOnTarget(newLockOnTarget);
            }
        }
        else{

            // stop locking onto a target if we are tracking one.

            SetLockOnTarget(null);
        }
    }

    private void OnLockOnPrevious(){
        
        // if we are currently locked onto a target.

        if(lockOnTarget!=null){
            Transform previousLockOnTarget = lockOnTargetDetector.GetPrevious();
            if(previousLockOnTarget!=null){
                SetLockOnTarget(previousLockOnTarget);
            }
        }
    }

    private void OnLockOnNext(){
        
        // if we are currently locked onto a target.

        if(lockOnTarget!=null){
            Transform nextLockOnTarget = lockOnTargetDetector.GetNext();
            if(nextLockOnTarget!=null){
                SetLockOnTarget(nextLockOnTarget);
            }
        }
    }

}
