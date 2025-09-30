using System;
using Entropek.Systems;
using Entropek.Systems.Autoload;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.VFX;

public class Player : MonoBehaviour{


    ///
    /// Definitions.
    /// 


    private enum State : byte{
        Idle,
        Attack,
        Dodge,
        Run,
        Jump
    }


    /// 
    /// Components.
    /// 


    [Header("Components")]
    [SerializeField] private CameraController cam;
    [SerializeField] private Health health;
    [SerializeField] private JumpMovement jumpMovement;
    [SerializeField] private Movement movement;
    [SerializeField] private VisualEffect slash1Vfx;
    [SerializeField] private VisualEffect slash2Vfx;
    [SerializeField] private Hitbox slash1Hitbox;
    [SerializeField] private Hitbox slash2Hitbox;
    [SerializeField] private Timer attackTimer; 
    [SerializeField] private Timer dodgeTimer;
    

    /// 
    /// Data.
    /// 

    [Header("Data")]
    [SerializeField]private State state = State.Idle;
    private event Action FaceChosenDirection;

    private const float AttackLungeForce            = 3.33f;
    private const float AttackLungeDecaySpeed       = AttackLungeForce * 3f;
    private const float DodgeForce                  = 25;
    private const float DodgeDecaySpeed             = DodgeForce * 3f;
    private const float FaceMoveDirectionSpeed      = 16.7f;
    private const float FaceAttackDirectionSpeed    = 16.7f;
    private const float AttackHitCameraShakeForce   = 3.33f;
    private const float AttackHitCameraShakeTime    = 0.167f;


    /// 
    /// Base.
    /// 


    private void OnEnable(){
        LinkEvents();
    }

    private void Update(){
        FaceChosenDirection?.Invoke();
    }

    private void OnDisable(){
        UnlinkEvents();
    }


    /// 
    /// Functions.
    /// 


    private void UpdateMoveDirection(Vector3 moveDirection){
        Vector3 cameraForwardXZ = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z).normalized;
        Vector3 cameraRightXZ   = new Vector3(cam.transform.right.x, 0, cam.transform.right.z).normalized;
        movement.moveDirection  = moveDirection.x * cameraRightXZ + moveDirection.y * cameraForwardXZ;
    }
    
    private void FaceMoveDirection(){
        Vector3 moveDirection = movement.moveDirection;
        if(moveDirection != Vector3.zero){
            Entropek.UnityUtils.Transform.RotateYAxisToDirection(transform, movement.moveDirection, FaceMoveDirectionSpeed * Time.deltaTime);
        }
    }

    private void FaceAttackDirection(){
        Entropek.UnityUtils.Transform.RotateYAxisToDirection(transform, cam.transform.forward, FaceAttackDirectionSpeed * Time.deltaTime);
    }


    ///
    /// State Machine.
    /// 


    public void Idle(){
        state = State.Idle;
        UpdateMoveDirection(Vector3.zero);
    }

    private void Run(){
        Run(InputManager.Singleton.moveInput);
    }

    public void Run(Vector3 directtion){
        state = State.Run;
        UpdateMoveDirection(directtion);
        FaceChosenDirection = FaceMoveDirection;
    }

    public void JumpStart(){
        state = State.Jump;
        jumpMovement.StartJumping();
    }

    public void JumpStop(){
        state = State.Jump;
        jumpMovement.StopJumping();

        // TODO:
        //  Will need an extra check here later down the line to check if the player
        //  is grounded or not. The player should enter a falling state when not grounded.

        if(InputManager.Singleton.moveInputSqrMagnitude == 0){
            Idle();
        }
        else{
            Run();
        }
    }

    public void Dodge(){
        state = State.Dodge;
        movement.moveDirection = Vector3.zero;
        movement.ImpulseRelativeToGround(transform.forward, DodgeForce, DodgeDecaySpeed);
        dodgeTimer.Begin();
    }

    bool slashFlag = false;

    public void Attack(){
        
        state = State.Attack;
        
        // get the next slash vfx and hitbox.

        VisualEffect slash = slashFlag==true? slash1Vfx : slash2Vfx;
        Hitbox hitbox = slashFlag==true? slash1Hitbox : slash2Hitbox;
        
        // enablethe slash.

        slash.Play();
        hitbox.Enable();
        
        // swap slashes for next time.

        slashFlag=!slashFlag;

        // face in the attack direction.
        
        FaceChosenDirection = FaceAttackDirection;
        
        // stop moving.

        movement.moveDirection = Vector3.zero;

        // apply a forward force.

        movement.ImpulseRelativeToGround(transform.forward, AttackLungeForce, AttackLungeDecaySpeed);

        attackTimer.Begin();
    }
    

    /// 
    /// Linkage.
    /// 


    private void LinkEvents(){
        LinkCameraEvents();
        LinkInputEvents();
        LinkHitboxEvents();
        LinkTimerEvents();
    }

    private void UnlinkEvents(){
        UnlinkCameraEvents();
        UnlinkInputEvents();
        UnlinkHitboxEvents();
        UnlinkTimerEvents();
    }


    ///
    /// Timer event linkage.
    /// 

    
    private void LinkTimerEvents(){
        dodgeTimer.Timeout += OnDodgeTimerTimeout;
        attackTimer.Timeout += OnAttackTimerTimeout;
    }

    private void UnlinkTimerEvents(){
        dodgeTimer.Timeout -= OnDodgeTimerTimeout;
        attackTimer.Timeout -= OnAttackTimerTimeout;
    }

    private void OnDodgeTimerTimeout(){
        
        // TODO:
        //  Add a falling state evalutation.
        
        if(InputManager.Singleton.moveInputSqrMagnitude>0){
            Run();
        }
        else{
            Idle();
        }

    }

    private void OnAttackTimerTimeout(){
        
        // TODO:
        //  Add a falling state evalutation.
        
        if(InputManager.Singleton.moveInputSqrMagnitude>0){
            Run();
        }
        else{
            Idle();
        }

    }


    /// 
    /// Hitbox Linkage.
    /// 


    private void LinkHitboxEvents(){
        slash1Hitbox.Hit += OnHitboxHit;
        slash2Hitbox.Hit += OnHitboxHit;    
    }

    private void UnlinkHitboxEvents(){
        slash1Hitbox.Hit -= OnHitboxHit;
        slash2Hitbox.Hit -= OnHitboxHit;    
    }

    private void OnHitboxHit(GameObject other){
        cam.StartShaking(AttackHitCameraShakeForce, AttackHitCameraShakeTime);
    }


    ///
    /// Camera Linkage.
    /// 

    
    private void LinkCameraEvents(){
        cam.Rotated += OnCameraRotated;
    }

    private void UnlinkCameraEvents(){
        cam.Rotated -= OnCameraRotated;
    }

    private void OnCameraRotated(){
        
        if(state != State.Run 
        && state != State.Jump 
        && state != State.Dodge){
            return;
        }
        
        UpdateMoveDirection(InputManager.Singleton.moveInput);
    }


    /// 
    /// Input Linkage.
    /// 


    private void LinkInputEvents(){
        
        InputManager input = InputManager.Singleton; 
        
        input.Move          += OnMoveInput;
        input.JumpStart     += OnJumpStartInput;
        input.JumpStop      += OnJumpStopInput;
        input.Attack        += OnAttackInput;
        input.Dodge         += OnDodgeInput;
    }

    private void UnlinkInputEvents(){
        
        InputManager input = InputManager.Singleton; 
        
        input.Move          -= OnMoveInput;
        input.JumpStart     -= OnJumpStartInput;
        input.JumpStop      -= OnJumpStopInput;
        input.Attack        -= OnAttackInput;
        input.Dodge         -= OnDodgeInput;
    }

    private void OnMoveInput(Vector2 moveInput){
        
        if(state!=State.Idle 
        && state!=State.Run 
        && state!=State.Jump){
            return;
        }

        // TODO:
        //  Add a falling state evaluation.

        if(moveInput.sqrMagnitude == 0){
            Idle();
        }
        else{
            Run();
        }
    }

    private void OnJumpStartInput(){
        
        if(state!=State.Idle 
        && state!=State.Run){
            return;
        }
        
        JumpStart();
    }

    private void OnJumpStopInput(){

        if(state!=State.Jump){
            return;
        }

        JumpStop();

    }


    private void OnDodgeInput(){
        
        if(state==State.Attack 
        || state==State.Dodge){
            return;
        }

        Dodge();
    }

    private void OnAttackInput(){
        
        if(state==State.Attack
        || state==State.Dodge){
            return;
        }
    
        Attack();
    }
}
