using System;
using Entropek.Systems;
using Entropek.Time;
using Entropek.Systems.Interaction;
using Entropek.Systems.Trails;
using UnityEngine;
using Entropek.Physics;
using Entropek.Audio;
using Entropek.UnityUtils.AnimatorUtils;
using Entropek.Vfx;

public class Player : MonoBehaviour {


    ///
    /// Definitions.
    /// 


    private enum PlayerState : byte
    {
        Idle,
        Attack,
        Dodge,
        Run,
        Jump,
        Fall
    }

    private enum CoyoteState : byte
    {
        None,
        Dodge,
        Attack,
        Fall,
    }


    /// 
    /// Components.
    /// 


    [Header("Components")]
    [SerializeField] private CameraController cam;
    [SerializeField] private Entropek.EntityStats.ShieldedHealth health;
    [SerializeField] private JumpMovement jumpMovement;
    [SerializeField] private CharacterControllerMovement movement;
    [SerializeField] private Timer iFrameStateTimer;
    [SerializeField] private Animator animator;
    [SerializeField] private Interactor interactor;
    [SerializeField] private SkinnedMeshTrailSystem arcGhost;
    [SerializeField] private DodgeTrailController dodgeTrail;
    [SerializeField] private GroundChecker groundChecker;
    [SerializeField] private ForceApplier forceApplier;
    [SerializeField] private AudioPlayer audioPlayer;
    [SerializeField] private AnimationEventReciever animationEventReciever;
    [SerializeField] private VfxManager vfx;

    [Header("HitBoxes")]
    [SerializeField] private Entropek.Combat.Hitbox slashLeftHitBox;
    [SerializeField] private Entropek.Combat.Hitbox slashRightHitBox;

    /// 
    /// Data.
    /// 


    [Header("Data")]
    [SerializeField] private PlayerState state = PlayerState.Idle;
    private CoyoteState coyoteState = CoyoteState.None;
    private event Action CoyoteCallback;
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


    private const float AttackLungeForce = 4.44f;
    private const float AttackLungeDecaySpeed = AttackLungeForce * 3f;
    private const float DodgeForce = 25;
    private const float DodgeDecaySpeed = DodgeForce * 3.33f;
    private const float FaceMoveDirectionSpeed = 16.7f;
    private const float FaceAttackDirectionSpeed = 16.7f;
    private const float AttackHitCameraShakeForce = 3.33f;
    private const float AttackHitCameraShakeTime = 0.167f;
    private const float AttackShieldRestorationAmount = 5f;


    /// 
    /// Vfx Ids.
    /// 


    private const int LeftSlashVfxId = 0;
    private const int RightSlashVfxId = 1;
    private const int AttackHitVfxId = 2;


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

    private Vector3 GetMoveDirectionRelativeToCamera(Vector2 moveDirection)
    {
        Vector3 cameraForwardXZ = new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(cam.transform.right.x, 0, cam.transform.right.z).normalized;
        return moveDirection.x * cameraRightXZ + moveDirection.y * cameraForwardXZ;
    }

    private void FaceMoveDirection()
    {
        Vector3 moveDirection = movement.moveDirection;
        if (moveDirection != Vector3.zero)
        {
            Entropek.UnityUtils.Transform.RotateYAxisToDirection(transform, movement.moveDirection, FaceMoveDirectionSpeed * Time.deltaTime);
        }
    }

    private void SnapToFaceMoveInput()
    {
        // only snap if there is a direction to snap to.
        // if we snap when input zero, the result is undefined behaviour.

        Vector2 moveInput = InputManager.Singleton.moveInput;

        if (moveInput != Vector2.zero)
        {
            Entropek.UnityUtils.Transform.RotateYAxisToDirection(transform, GetMoveDirectionRelativeToCamera(InputManager.Singleton.moveInput), 1);
        }
    }

    private void FaceAttackDirection()
    {
        Entropek.UnityUtils.Transform.RotateYAxisToDirection(transform, cam.transform.forward, FaceAttackDirectionSpeed * Time.deltaTime);
    }

    private void SwordAttackFrame()
    {
        forceApplier.ImpulseRelativeToGround(transform.forward, AttackLungeForce, AttackLungeDecaySpeed);
        audioPlayer.PlaySound("MeleeSwing", transform.position);
        if (slashFlag == true)
        {
            slashRightHitBox.Enable();
            vfx.PlayVfx(LeftSlashVfxId, transform.position + (transform.forward * 1.1f), transform.forward);
        }
        else
        {
            slashLeftHitBox.Enable();
            vfx.PlayVfx(RightSlashVfxId, transform.position + (transform.forward * 1.1f), transform.forward);
        }
    }

    private void HandleFootstepEffects()
    {
        audioPlayer.PlaySound("FootstepGrass", transform.position);
    }


    ///
    /// Player State Machine.
    /// 


    public void Idle() {
        state = PlayerState.Idle;
        FaceChosenDirection = null; // dont look face any direction when idle.
        movement.moveDirection = GetMoveDirectionRelativeToCamera(Vector2.zero);
        if (state != PlayerState.Jump && state != PlayerState.Fall) {
            animator.Play(IdleAnimation);
        }
    }

    private void Run() {
        Run(InputManager.Singleton.moveInput);
    }

    public void Run(Vector2 direction) {
        state = PlayerState.Run;
        movement.moveDirection = GetMoveDirectionRelativeToCamera(direction);
        FaceChosenDirection = FaceMoveDirection;
        if (state != PlayerState.Fall && state != PlayerState.Jump) {
            animator.Play(WalkAnimation);
        }
    }

    public void JumpStart() {
        state = PlayerState.Jump;
        jumpMovement.StartJumping();
        animator.Play(JumpStartAnimation);
    }

    public void JumpStop() {
        state = PlayerState.Jump;
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

    public void StartDodge() {

        // enter state.

        state = PlayerState.Dodge;
        canDodge = false;

        SnapToFaceMoveInput();
        FaceChosenDirection = FaceMoveDirection;

        // move only in the direction of our dodge.

        movement.moveDirection = Vector3.zero;
        movement.HaltMoveDirectionVelocity();

        movement.ClearGravityVelocity();

        jumpMovement.StopJumping();

        forceApplier.ImpulseRelativeToGround(transform.forward, DodgeForce, DodgeDecaySpeed);

        // play animations and vfx.

        arcGhost.SpawnMeshes();
        dodgeTrail.EnableTrail();
        audioPlayer.PlaySound("Dodge", gameObject);

        // rebind, so the transform of the skinned mesh is correctly reset to its 
        // "rest" position before abruptly switching animations.
        // Otherwise animations will retain their transform offsets from one animation to another
        // if they are not modified by the new animation.

        animator.Rebind();

        // force animation to play with 0 normalisedTime, 
        // ensuring to override an animation that may have been queued
        // during a coyote state switch. 

        animator.Play(DodgeAnimation, 0, 0);

        // enable i-frames.

        EnterIFrames();
    }

    public void StopDodge() {

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

        movement.moveDirection = GetMoveDirectionRelativeToCamera(InputManager.Singleton.moveInput);

        // re-enable gravity.

    }

    bool slashFlag = false;

    public void StartAttack()
    {

        state = PlayerState.Attack;


        // swap slashes for next time.

        slashFlag = !slashFlag;

        // stop moving.

        movement.moveDirection = Vector3.zero;
        movement.HaltMoveDirectionVelocity();
        jumpMovement.StopJumping();

        // rebind, so the transform of the skinned mesh is correctly reset to its 
        // "rest" position before abruptly switching animations.
        // Otherwise animations will retain their transform offsets from one animation to another
        // if they are not modified by the new animation.

        animator.Rebind();

        // force animation to play with 0 normalisedTime, 
        // ensuring to override an animation that may have been queued
        // during a coyote state switch. 

        animator.Play(AttackAnimation, 0, 0);

        // face in the attack direction.

        FaceChosenDirection = FaceAttackDirection;
    }

    public void StopAttack()
    {
        if (groundChecker.IsGrounded == true)
        {
            if (InputManager.Singleton.moveInputSqrMagnitude > 0)
            {
                Run();
            }
            else
            {
                Idle();
            }
        }
        else
        {
            Fall();
        }
    }

    public void Fall() {
        state = PlayerState.Fall;
    }

    public void EnterIFrames() {
        health.Vulnerable = false;
        iFrameStateTimer.Begin();
    }

    public void ExitIFrames()
    {
        health.Vulnerable = true;
    }


    ///
    /// Coyote State Machine.
    /// 


    /// <summary>
    /// Enters the coyote state for input queuing before the end of an action (attack, dodge, fall, etc.)
    /// </summary>
    /// <param name="coyoteState"></param>

    private void EnterCoyoteState(CoyoteState coyoteState)
    {
        // refresh the previously chached coyote callback
        // and enter the desired coyote state.

        this.coyoteState = coyoteState;
        CoyoteCallback = null;
    }

    /// <summary>
    /// Exits the coyote state for input queuing, executing the latest desired queued action by the player.
    /// </summary>

    private void ExitCoyoteState()
    {
        if (CoyoteCallback != null)
        {
            // stop any state we are coming out of.
            switch (state)
            {
                case PlayerState.Attack: StopAttack(); break;
                case PlayerState.Dodge: StopDodge(); break;
            }

            
            CoyoteCallback();
        }

        // reset the coyote state.
        coyoteState = CoyoteState.None;
    }


    /// 
    /// Linkage.
    /// 


    /// <summary>
    /// Links all component events to their respective callbacks.
    /// </summary>

    private void LinkEvents()
    {
        LinkCameraEvents();
        LinkInputEvents();
        LinkTimerEvents();
        LinkGroundCheckerEvents();
        LinkAnimationEventRecieverEvents();
        LinkHitBoxEvents();
    }

    /// <summary>
    /// Unlinks all component events from theiur respective callbacks.
    /// </summary>

    private void UnlinkEvents()
    {
        UnlinkCameraEvents();
        UnlinkInputEvents();
        UnlinkTimerEvents();
        UnlinkGroundCheckerEvents();
        UnlinkAnimationEventRecieverEvents();
        UnlinkHitBoxEvents();
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
        if (state != PlayerState.Dodge) {
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
        iFrameStateTimer.Timeout += OnIFrameStateTimeout;
    }

    private void UnlinkTimerEvents() {
        iFrameStateTimer.Timeout -= OnIFrameStateTimeout;
    }

    private void OnIFrameStateTimeout() {
        ExitIFrames();
    }


    /// 
    /// Hitbox Linkage.
    /// 

    private void LinkHitBoxEvents()
    {
        slashLeftHitBox.Hit += OnAttackHit;
        slashRightHitBox.Hit += OnAttackHit;
    }

    private void UnlinkHitBoxEvents()
    {
        slashLeftHitBox.Hit -= OnAttackHit;
        slashRightHitBox.Hit -= OnAttackHit;
    }

    private void OnAttackHit(GameObject other, Vector3 hitPoint)
    {
        vfx.PlayVfx(AttackHitVfxId, hitPoint, transform.forward);
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

        if (state != PlayerState.Run
        && state != PlayerState.Jump) {
            return;
        }

        movement.moveDirection = GetMoveDirectionRelativeToCamera(InputManager.Singleton.moveInput);
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

        if (state != PlayerState.Idle
        && state != PlayerState.Run
        && state != PlayerState.Fall) {
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

        if (coyoteState == CoyoteState.Dodge
        || coyoteState == CoyoteState.Attack)
        {
            CoyoteCallback = OnJumpInputCoyoteCallback;
        }

        if (state != PlayerState.Idle
        && state != PlayerState.Run)
        {
            return;
        }

        JumpStart();
    }

    private void OnJumpInputCoyoteCallback()
    {
        if (InputManager.Singleton.IsJumpPressed == true)
        {
            JumpStart();
        }
    }

    private void OnJumpStopInput()
    {
        if (state != PlayerState.Jump)
        {
            return;
        }

        JumpStop();

    }


    private void OnDodgeInput() {

        if (coyoteState == CoyoteState.Dodge 
        ||  coyoteState == CoyoteState.Attack)
        {
            CoyoteCallback = StartDodge;
        }

        if (state == PlayerState.Attack
        ||  state == PlayerState.Dodge
        ||  canDodge == false)
        {
            return;
        }

        StartDodge();
    }

    private void OnAttackInput() {

        if (coyoteState == CoyoteState.Dodge
        || coyoteState == CoyoteState.Attack)
        {
            CoyoteCallback = StartAttack;
        }

        if (state == PlayerState.Attack
        || state == PlayerState.Dodge)
        {
            return;
        }


        StartAttack();
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

            // Coyote States.

            case "EnterAttackCoyoteState": EnterCoyoteState(CoyoteState.Attack); break;
            case "EnterDodgeCoyoteState": EnterCoyoteState(CoyoteState.Dodge); break;
            case "ExitCoyoteState": ExitCoyoteState(); break;

            // Player States.

            case "ExitDodgeState": StopDodge(); break;
            case "ExitAttackState": StopAttack(); break;

            case "SwordAttackFrame": SwordAttackFrame(); break;
            case "Footstep": HandleFootstepEffects(); break;
            default:
                throw new InvalidOperationException($"animation event {eventName} not configured for player.");
        }
    }
}
