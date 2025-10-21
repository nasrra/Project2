using System;
using Entropek.Systems;
using Entropek.Systems.Combat;
using Entropek.Time;
using Entropek.Systems.Interaction;
using Entropek.Systems.Trails;
using UnityEngine;
using Entropek.Physics;
using Entropek.Audio;
using Entropek.UnityUtils.AnimatorUtils;

public class Player : MonoBehaviour {


    ///
    /// Definitions.
    /// 


    private enum State : byte {
        Idle,
        Attack,
        Dodge,
        Run,
        Jump,
        Fall
    }


    /// 
    /// Components.
    /// 


    [Header("Components")]
    [SerializeField] private CameraController cam;
    [SerializeField] private Entropek.EntityStats.ShieldedHealth health;
    [SerializeField] private JumpMovement jumpMovement;
    [SerializeField] private CharacterControllerMovement movement;
    [SerializeField] private AttackManager attackManager;
    [SerializeField] private Timer attackStateTimer;
    [SerializeField] private Timer dodgeStateTimer;
    [SerializeField] private Timer iFrameStateTimer;
    [SerializeField] private Animator animator;
    [SerializeField] private Interactor interactor;
    [SerializeField] private SkinnedMeshTrailSystem arcGhost;
    [SerializeField] private DodgeTrailController dodgeTrail;
    [SerializeField] private GroundChecker groundChecker;
    [SerializeField] private ForceApplier forceApplier;
    [SerializeField] private AudioPlayer audioPlayer;
    [SerializeField] private AnimationEventReciever animationEventReciever;


    /// 
    /// Data.
    /// 


    [Header("Data")]
    [SerializeField] private State state = State.Idle;
    private event Action FaceChosenDirection;
    private bool canDodge = true;


    /// 
    /// Animations.
    /// 


    private const string IdleAnimation = "Rig_Sword_Idle";
    private const string WalkAnimation = "Rig_Jog_Fwd_Loop";
    private const string DodgeAnimation = "Rig_Roll";
    private const string JumpStartAnimation = "Rig_Jump_Start";
    private const string FallAnimation = "Rig_Jump_Loop";
    private const string GroundedAnimation = "Rig_Jump_Land";
    private const string AttackAnimation = "Rig_Sword_Attack";


    /// 
    /// Data constants.
    /// 


    private const float AttackLungeForce = 3.33f;
    private const float AttackLungeDecaySpeed = AttackLungeForce * 3f;
    private const float DodgeForce = 25;
    private const float DodgeDecaySpeed = DodgeForce * 3.33f;
    private const float FaceMoveDirectionSpeed = 16.7f;
    private const float FaceAttackDirectionSpeed = 16.7f;
    private const float AttackHitCameraShakeForce = 3.33f;
    private const float AttackHitCameraShakeTime = 0.167f;
    private const float AttackShieldRestorationAmount = 5f;


    /// 
    /// Attack Ids.
    /// 


    private const int Slash1AttackId = 0;
    private const int Slash2AttackId = 1;


    /// 
    /// Base.
    /// 

    private void OnEnable()
    {
        LinkEvents();
        Entropek.EntityStats.ShieldedHealthBarHud.Singleton.ShieldedHealthBar.DisplayShieldedHealth(health);
    }

    private void Update() {
        FaceChosenDirection?.Invoke();
    }

    private void OnDisable() {
        UnlinkEvents();
    }


    /// 
    /// Functions.
    /// 


    private void UpdateMoveDirection(Vector3 moveDirection) {
        Vector3 cameraForwardXZ = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(cam.transform.right.x, 0, cam.transform.right.z).normalized;
        movement.moveDirection = moveDirection.x * cameraRightXZ + moveDirection.y * cameraForwardXZ;
    }

    private void FaceMoveDirection() {
        Vector3 moveDirection = movement.moveDirection;
        if (moveDirection != Vector3.zero) {
            Entropek.UnityUtils.Transform.RotateYAxisToDirection(transform, movement.moveDirection, FaceMoveDirectionSpeed * Time.deltaTime);
        }
    }

    private void FaceAttackDirection() {
        Entropek.UnityUtils.Transform.RotateYAxisToDirection(transform, cam.transform.forward, FaceAttackDirectionSpeed * Time.deltaTime);
    }


    ///
    /// State Machine.
    /// 


    public void Idle() {
        state = State.Idle;
        UpdateMoveDirection(Vector3.zero);
        if (state != State.Jump && state != State.Fall) {
            animator.Play(IdleAnimation);
        }
    }

    private void Run() {
        Run(InputManager.Singleton.moveInput);
    }

    public void Run(Vector3 directtion) {
        state = State.Run;
        UpdateMoveDirection(directtion);
        FaceChosenDirection = FaceMoveDirection;
        if (state != State.Fall && state != State.Jump) {
            animator.Play(WalkAnimation);
        }
    }

    public void JumpStart() {
        state = State.Jump;
        jumpMovement.StartJumping();
        animator.Play(JumpStartAnimation);
    }

    public void JumpStop() {
        state = State.Jump;
        jumpMovement.StopJumping();

        // TODO:
        //  Will need an extra check here later down the line to check if the player
        //  is grounded or not. The player should enter a falling state when not grounded.
        if (groundChecker.IsGrounded == true) {

            if (InputManager.Singleton.moveInputSqrMagnitude == 0) {
                Idle();
            }
            else {
                Run();
            }
        }
        else {
            Fall();
        }
    }

    public void DodgeStart() {

        // enter state.

        dodgeStateTimer.Begin();
        state = State.Dodge;
        canDodge = false;

        // move only in the direction of our dodge.

        movement.moveDirection = Vector3.zero;
        movement.HaltMoveDirectionVelocity();

        movement.ClearGravityVelocity();

        jumpMovement.StopJumping();

        forceApplier.ImpulseRelativeToGround(transform.forward, DodgeForce, DodgeDecaySpeed);

        // play animations and vfx.

        arcGhost.SpawnMeshes();
        dodgeTrail.EnableTrail();
        animator.Play(DodgeAnimation);
        audioPlayer.PlaySound("Dodge", gameObject);

        // enable i-frames.

        EnterIFrames();
    }

    public void DodgeStop() {

        if (groundChecker.IsGrounded == true) {

            // only reset out dodge when grounded.

            canDodge = true;

            if (InputManager.Singleton.moveInputSqrMagnitude > 0) {
                Run();
            }
            else {
                Idle();
            }
        }
        else {
            Fall();
        }

        // set move direction back to the player input.

        UpdateMoveDirection(InputManager.Singleton.moveInput);

        // re-enable gravity.

    }

    bool slashFlag = false;

    public void Attack() {

        state = State.Attack;

        attackManager.BeginAttack(slashFlag == true ? Slash1AttackId : Slash2AttackId);

        // swap slashes for next time.

        slashFlag = !slashFlag;

        // face in the attack direction.

        FaceAttackDirection();
        FaceChosenDirection = null;

        // stop moving.

        movement.moveDirection = Vector3.zero;
        movement.HaltMoveDirectionVelocity();

        // apply a forward force.

        forceApplier.ImpulseRelativeToGround(transform.forward, AttackLungeForce, AttackLungeDecaySpeed);

        animator.Play(AttackAnimation);

        audioPlayer.PlaySound("MeleeSwing", transform.position);

        attackStateTimer.Begin();

    }

    public void Fall() {
        state = State.Fall;
    }

    public void EnterIFrames() {
        health.Vulnerable = false;
        iFrameStateTimer.Begin();
    }

    public void ExitIFrames() {
        health.Vulnerable = true;
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents() {
        LinkCameraEvents();
        LinkInputEvents();
        LinkAttackManagerEvents();
        LinkTimerEvents();
        LinkGroundCheckerEvents();
        LinkAnimationEventRecieverEvents();
    }

    private void UnlinkEvents() {
        UnlinkCameraEvents();
        UnlinkInputEvents();
        UnlinkAttackManagerEvents();
        UnlinkTimerEvents();
        UnlinkGroundCheckerEvents();
        UnlinkAnimationEventRecieverEvents();
    }


    ///
    /// Movement event linkage.
    /// 


    private void LinkGroundCheckerEvents() {
        groundChecker.Grounded += OnGrounded;
    }

    private void UnlinkGroundCheckerEvents() {
        groundChecker.Grounded -= OnGrounded;
    }

    private void OnGrounded() {
        if (state != State.Dodge) {
            if (InputManager.Singleton.moveInputSqrMagnitude > 0) {
                Run();
            }
            else {
                // animator.Play(GroundedAnimation);
                Idle();
            }
        }

        // reset our dodge whenever grounded.

        canDodge = true;
    }


    ///
    /// Timer event linkage.
    /// 


    private void LinkTimerEvents() {
        dodgeStateTimer.Timeout += OnDodgeStateTimerTimeout;
        attackStateTimer.Timeout += OnAttackStateTimerTimeout;
        iFrameStateTimer.Timeout += OnIFrameStateTimeout;
    }

    private void UnlinkTimerEvents() {
        dodgeStateTimer.Timeout -= OnDodgeStateTimerTimeout;
        attackStateTimer.Timeout -= OnAttackStateTimerTimeout;
        iFrameStateTimer.Timeout -= OnIFrameStateTimeout;
    }

    private void OnDodgeStateTimerTimeout() {
        DodgeStop();
    }

    private void OnAttackStateTimerTimeout() {

        if (groundChecker.IsGrounded == true) {
            if (InputManager.Singleton.moveInputSqrMagnitude > 0) {
                Run();
            }
            else {
                Idle();
            }
        }
        else {
            Fall();
        }

    }

    private void OnIFrameStateTimeout() {
        ExitIFrames();
    }


    /// 
    /// Hitbox Linkage.
    /// 


    private void LinkAttackManagerEvents() {
        attackManager.AttackHit += OnAttackHit;
    }

    private void UnlinkAttackManagerEvents() {
        attackManager.AttackHit -= OnAttackHit;
    }

    private void OnAttackHit() {
        health.RestoreShield(AttackShieldRestorationAmount);
        cam.StartShaking(AttackHitCameraShakeForce, AttackHitCameraShakeTime);
    }


    ///
    /// Camera Linkage.
    /// 


    private void LinkCameraEvents() {
        cam.Rotated += OnCameraRotated;
    }

    private void UnlinkCameraEvents() {
        cam.Rotated -= OnCameraRotated;
    }

    private void OnCameraRotated() {

        if (state != State.Run
        && state != State.Jump) {
            return;
        }

        UpdateMoveDirection(InputManager.Singleton.moveInput);
    }


    /// 
    /// Input Linkage.
    /// 


    private void LinkInputEvents() {

        InputManager input = InputManager.Singleton;

        input.Move += OnMoveInput;
        input.JumpStart += OnJumpStartInput;
        input.JumpStop += OnJumpStopInput;
        input.Attack += OnAttackInput;
        input.Dodge += OnDodgeInput;
        input.Interact += OnInteractInput;
        input.NextInteractable += OnNextInteractable;
        input.PreviousInteractable += OnPreviousInteractable;
    }

    private void UnlinkInputEvents() {

        InputManager input = InputManager.Singleton;

        input.Move -= OnMoveInput;
        input.JumpStart -= OnJumpStartInput;
        input.JumpStop -= OnJumpStopInput;
        input.Attack -= OnAttackInput;
        input.Dodge -= OnDodgeInput;
        input.Interact -= OnInteractInput;
        input.NextInteractable -= OnNextInteractable;
        input.PreviousInteractable -= OnPreviousInteractable;
    }

    private void OnMoveInput(Vector2 moveInput) {

        if (state != State.Idle
        && state != State.Run
        && state != State.Fall) {
            return;
        }

        // TODO:
        //  Add a falling state evaluation.

        if (moveInput.sqrMagnitude == 0) {
            Idle();
        }
        else {
            Run();
        }
    }

    private void OnJumpStartInput() {

        if (state != State.Idle
        && state != State.Run) {
            return;
        }

        JumpStart();
    }

    private void OnJumpStopInput() {

        if (state != State.Jump) {
            return;
        }

        JumpStop();

    }


    private void OnDodgeInput() {


        if (state == State.Attack
        || state == State.Dodge
        || canDodge == false)
        {
            return;
        }

        DodgeStart();
    }

    private void OnAttackInput() {

        if (state == State.Attack
        || state == State.Dodge) {
            return;
        }

        Attack();
    }

    private void OnInteractInput() {
        interactor.Interact();
    }

    private void OnPreviousInteractable() {
        interactor.PreviousInteractable();
    }

    private void OnNextInteractable()
    {
        interactor.NextInteractable();
    }

    // Animation Event Reciever Linkage.

    private void LinkAnimationEventRecieverEvents()
    {
        animationEventReciever.AnimationEventTriggered += OnAnimationEventTriggered;
    }

    private void UnlinkAnimationEventRecieverEvents()
    {
        animationEventReciever.AnimationEventTriggered -= OnAnimationEventTriggered;        
    }

    private void OnAnimationEventTriggered(string eventName)
    {
        switch (eventName)
        {
            case "Footstep":
                audioPlayer.PlaySound("FootstepGrass", transform.position);
                break;
            default:
                throw new InvalidOperationException($"animation event {eventName} not configured for player.");
        }
    }
}
