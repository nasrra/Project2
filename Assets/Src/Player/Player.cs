using System;
using Entropek.Systems.Interaction;
using Entropek.Systems.Trails;
using UnityEngine;
using Entropek.Physics;
using Entropek.Audio;
using Entropek.UnityUtils.AnimatorUtils;
using Entropek.Vfx;
using Entropek.UnityUtils.Attributes;
using Unity.VisualScripting;

public class Player : MonoBehaviour {


    /// 
    /// Callbacks.
    /// 


    private event Action CoyoteCallback;
    private event Action FaceChosenDirection;


    /// 
    /// Components.
    /// 


    [Header("Components")]
    [SerializeField] private Entropek.Camera.CameraController cam;
    [SerializeField] private Entropek.Camera.CameraPostProcessingController cameraPostProcessing;
    [SerializeField] private Entropek.EntityStats.ShieldedHealth health;
    [SerializeField] private JumpMovement jumpMovement;
    [SerializeField] private CharacterControllerMovement movement;
    [SerializeField] private Animator animator;
    [SerializeField] private Interactor interactor;
    [SerializeField] private SkinnedMeshTrailSystem arcGhost;
    [SerializeField] private DodgeTrailController dodgeTrail;
    [SerializeField] private GroundCheck groundChecker;
    [SerializeField] private ForceApplier forceApplier;
    [SerializeField] private AudioPlayer audioPlayer;
    [SerializeField] private AnimationEventReciever animationEventReciever;
    [SerializeField] private VfxManager vfx;

    [Header("Timers")]
    [SerializeField] private Entropek.Time.OneShotTimer fallCoyoteTimer;

    [Header("HitBoxes")]
    [SerializeField] private Entropek.Combat.Hitbox slashLeftHitBox;
    [SerializeField] private Entropek.Combat.Hitbox slashRightHitBox;

    /// 
    /// Data.
    /// 


    [Header("Data")]
    [RuntimeField] private PlayerState playerState = PlayerState.Idle;
    public PlayerState PlayerState => playerState;
    [RuntimeField] private CoyoteState coyoteState = CoyoteState.None;
    public CoyoteState CoyoteState => coyoteState;
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
    private const string StaggerAnimation = "Rig_Stagger";


    /// 
    /// Data constants.
    /// 


    private const float AttackLungeForce = 4.44f;
    private const float AttackLungeDecaySpeed = AttackLungeForce * 3f;
    private const float AttackShieldRestorationAmount = 5f;
    private const float AttackHitCameraShakeForce = 4f;
    private const float AttackHitCameraShakeTime = 0.167f;
    private const float AttackHitLensDistortionIntensity = 0.24f;
    private const float AttackHitLensDistortionDuration = 0.16f;
    private const float AttackHitMotionBlurDuration = 0.33f;
    private const float AttackHitMotionBlurIntensity = 1f;

    private const float DodgeForce = 23.33f;
    private const float DodgeDecaySpeed = DodgeForce * 2.66f;
    
    private const float FaceMoveDirectionSpeed = 16.7f;
    private const float FaceAttackDirectionSpeed = 16.7f;
    
    
    private const float DamagedCameraShakeStrength = 7.77f;
    private const float DamagedCameraShakeTime = 0.33f;
    private const float DamagedVignettePulseInIntensity = 0.25f;
    private const float DamagedVignettePulseOutIntensity = 0f;
    private const float DamagedVignettePulseInDuration = 0.167f;
    private const float DamagedVignettePulseOutDuration = 1f;
    private const float DamagedLensDistortionIntensity = 0.45f;
    private const float DamagedLensDistortionDuration = 0.16f;
    private const float DamagedMotionBlurDuration = 0.66f;
    private const float DamagedMotionBlurIntensity = 1f;


    /// 
    /// Vfx Ids.
    /// 


    private const int LeftSlashVfxId = 0;
    private const int RightSlashVfxId = 1;
    private const int AttackHitVfxId = 2;


    /// 
    /// Base.
    /// 

    private void Awake()
    {
        LinkEvents();
    }

    private void OnEnable()
    {
        Entropek.EntityStats.ShieldedHealthBarHud.Singleton.ShieldedHealthBar.DisplayShieldedHealth(health);
    }

    private void Update() {
        FaceChosenDirection?.Invoke();
    }

    private void OnDestroy()
    {
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
        audioPlayer.PlaySound("FootstepGrassLight", transform.position);
    }


    ///
    /// Player State Machine.
    /// 


    /// <summary>
    /// Returns the player to a resting state to branch from into different actions; depending upon their endivronmental circumnstances.
    /// </summary>

    private void EnterRestState()
    {
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
    }

    public void Idle()
    {
        playerState = PlayerState.Idle;
        FaceChosenDirection = null; // dont look face any direction when idle.
        movement.moveDirection = GetMoveDirectionRelativeToCamera(Vector2.zero);
        if (playerState != PlayerState.Jump && playerState != PlayerState.Fall)
        {
            animator.Play(IdleAnimation);
        }
    }

    private void Run() {
        Run(InputManager.Singleton.moveInput);
    }

    public void Run(Vector2 direction) {
        playerState = PlayerState.Run;
        movement.moveDirection = GetMoveDirectionRelativeToCamera(direction);
        FaceChosenDirection = FaceMoveDirection;
        if (playerState != PlayerState.Fall && playerState != PlayerState.Jump) {
            animator.Play(WalkAnimation);
        }
    }

    public void JumpStart() {
        playerState = PlayerState.Jump;
        jumpMovement.StartJumping();
        animator.Play(JumpStartAnimation);
    }

    public void JumpStop() {
        playerState = PlayerState.Jump;
        jumpMovement.StopJumping();
        EnterRestState();
    }

    public void StartDodge() {

        // enter state.

        playerState = PlayerState.Dodge;
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
        EnterRestState();
    }

    bool slashFlag = false;

    public void StartAttack()
    {
        playerState = PlayerState.Attack;


        // swap slashes for next time.

        slashFlag = !slashFlag;

        // stop moving.

        movement.moveDirection = Vector3.zero;
        // movement.HaltMoveDirectionVelocity();
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
        EnterRestState();
    }

    public void Fall() {
        playerState = PlayerState.Fall;
        animator.Play(FallAnimation);
    }

    public void EnterIFrames() {
        health.Vulnerable = false;
    }

    public void ExitIFrames()
    {
        health.Vulnerable = true;
    }

    public void EnterStagger()
    {
        playerState = PlayerState.Stagger;
        movement.moveDirection = Vector3.zero;
        animator.Play(StaggerAnimation);
    }

    public void ExitStagger()
    {
        EnterRestState();
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
        // enter the desired coyote state.

        this.coyoteState = coyoteState;

        // refresh the previously chached coyote callback

        CoyoteCallback = null;

        // only begin the fall coyote state timer if the desired
        // coyote state is Fall, and stop it when not.
        // this is to ensure that the fall coyote timer does always timeout during
        // a coyote state switch, making sure that its timeout doesnt effect other states
        // by exiting coyote state too early.

        if (coyoteState == CoyoteState.Fall)
        {
            fallCoyoteTimer.Begin();
        }
        else
        {
            fallCoyoteTimer.Halt();
        }
    }

    /// <summary>
    /// Exits the coyote state for input queuing, executing the latest desired queued action by the player.
    /// </summary>

    private void ExitCoyoteState()
    {
        if (CoyoteCallback != null){            
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
        LinkHealthEvents();
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
        UnlinkHealthEvents();
    }


    ///
    /// Health Event Linkage.
    /// 

    private void LinkHealthEvents()
    {
        health.HealthDamaged += OnHealthDamaged;
        health.ShieldDamaged += OnHealthDamaged;
    }

    private void UnlinkHealthEvents()
    {
        health.HealthDamaged -= OnHealthDamaged;
        health.ShieldDamaged -= OnHealthDamaged;
    }

    private void OnHealthDamaged(float amount){
        cam.StartShaking(DamagedCameraShakeStrength, DamagedCameraShakeTime);

        cameraPostProcessing.PulseVignetteIntensity(
            DamagedVignettePulseInDuration,
            DamagedVignettePulseOutDuration,
            DamagedVignettePulseInIntensity,
            DamagedVignettePulseOutIntensity
        );

        cameraPostProcessing.PulseMotionBlurIntensity(
            DamagedMotionBlurDuration,
            DamagedMotionBlurIntensity
        );

        cameraPostProcessing.PulseLensDistortionIntensity(
            DamagedLensDistortionDuration,
            DamagedLensDistortionIntensity
        );

        EnterStagger();
    }

    ///
    /// Ground check event linkage.
    /// 

    private void LinkGroundCheckerEvents()
    {
        groundChecker.Grounded += OnGrounded;
        groundChecker.Airborne += OnAirborne;
    }

    private void UnlinkGroundCheckerEvents()
    {
        groundChecker.Grounded -= OnGrounded;
        groundChecker.Airborne += OnAirborne;
    }

    private void OnGrounded()
    {
        // only enter the rest state if we are currently not performing any actions.

        if (playerState != PlayerState.Dodge 
        && playerState != PlayerState.Attack) 
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

        // reset our dodge whenever grounded.

        canDodge = true;
    }

    private void OnAirborne()
    {
        EnterCoyoteState(CoyoteState.Fall);
    }

    ///
    /// Timer event linkage.
    /// 


    private void LinkTimerEvents()
    {
        fallCoyoteTimer.Timeout += OnFallCoyoteTimeout;
    }

    private void UnlinkTimerEvents() {
        fallCoyoteTimer.Timeout -= OnFallCoyoteTimeout;
    }

    private void OnFallCoyoteTimeout()
    {
        ExitCoyoteState();
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

        cameraPostProcessing.PulseLensDistortionIntensity(
            AttackHitLensDistortionDuration,
            AttackHitLensDistortionIntensity
        );

        cameraPostProcessing.PulseMotionBlurIntensity(
            AttackHitMotionBlurDuration,
            AttackHitMotionBlurIntensity
        );
        
        audioPlayer.PlaySound("MeleeHit", hitPoint);
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

        if (playerState != PlayerState.Run
        && playerState != PlayerState.Jump) {
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

        if (playerState != PlayerState.Idle
        && playerState != PlayerState.Run
        && playerState != PlayerState.Fall) {
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

        // queue the action if we are attacking or dodging coyote states.

        if (coyoteState == CoyoteState.Attack
        || coyoteState == CoyoteState.Dodge
        || coyoteState == CoyoteState.Stagger)
        {
            CoyoteCallback = OnJumpInputCoyoteCallback;
            return;
        }

        // immediately perform the action if we are in falling coyote state.
        if (coyoteState == CoyoteState.Fall)
        {
            CoyoteCallback = OnJumpInputCoyoteCallback;
            ExitCoyoteState();
            return;            
        }

        if ((playerState != PlayerState.Idle && playerState != PlayerState.Run) || groundChecker.IsGrounded == false)
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
        if (playerState != PlayerState.Jump)
        {
            return;
        }

        JumpStop();

    }


    private void OnDodgeInput() {

        if (coyoteState == CoyoteState.Dodge 
        ||  coyoteState == CoyoteState.Attack
        ||  coyoteState == CoyoteState.Stagger)
        {
            CoyoteCallback = StartDodge;
        }

        if (playerState == PlayerState.Attack
        ||  playerState == PlayerState.Dodge
        ||  playerState == PlayerState.Stagger
        ||  canDodge == false)
        {
            return;
        }

        StartDodge();
    }

    private void OnAttackInput() {

        if (coyoteState == CoyoteState.Dodge
        || coyoteState == CoyoteState.Attack
        || coyoteState == CoyoteState.Stagger)
        {
            CoyoteCallback = StartAttack;
        }

        if (playerState == PlayerState.Attack
        ||  playerState == PlayerState.Stagger
        ||  playerState == PlayerState.Dodge)
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


    /// 
    /// Animation Event Reciever Linkage.
    /// 


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
            case "EnterStaggerCoyoteState": EnterCoyoteState(CoyoteState.Stagger); break;

            // Note:
            //  Exit Coyote state directly when exiting an action/animation state
            //  to ensure any queued actions occur directly after the action has finished.
            //  Do NOT put these exit coyote calls inside the actual functions, that will cause
            //  a recursive loop as exit coyote state calls the stop functions of a given player state. 

            case "ExitDodgeState": StopDodge(); ExitCoyoteState(); break;
            case "ExitAttackState": StopAttack(); ExitCoyoteState(); break;
            case "ExitStaggerState": ExitStagger(); ExitCoyoteState(); break;

            // Player States.

            case "EnterIFrames": EnterIFrames(); break;
            case "ExitIFrames" : ExitIFrames(); break;

            // attack frames.

            case "SwordAttackFrame": SwordAttackFrame(); break;
            
            // miscellaneous.
            
            case "Footstep": HandleFootstepEffects(); break;
            

            default:
                throw new InvalidOperationException($"animation event {eventName} not configured for player.");
        }
    }
}
