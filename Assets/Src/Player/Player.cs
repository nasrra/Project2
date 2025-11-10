using System;
using Entropek.Interaction;
using Entropek.Systems.Trails;
using UnityEngine;
using Entropek.Physics;
using Entropek.Audio;
using Entropek.UnityUtils.AnimatorUtils;
using Entropek.Vfx;
using Entropek.UnityUtils.Attributes;
using Entropek.Combat;
using Entropek.Camera;
using Entropek.EntityStats;
using UnityEditor;
using FMOD.Studio;

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
    
    [SerializeField] private CameraController cameraController;
    public CameraController CameraController => cameraController;

    [SerializeField] private CameraPostProcessingController cameraPostProcessing;
    public CameraPostProcessingController CameraPostProcessingController => cameraPostProcessing;
    
    [SerializeField] private Health health;
    public Health Health => health;

    [SerializeField] private JumpMovement jumpMovement;
    public JumpMovement JumpMovement => jumpMovement;

    [SerializeField] private CharacterControllerMovement characterControllerMovement;
    public CharacterControllerMovement CharacterControllerMovement => characterControllerMovement;

    [SerializeField] private Animator animator;
    public Animator Animator => animator;

    [SerializeField] private Interactor interactor;
    public Interactor Interactor => interactor;

    [SerializeField] private GroundCheck groundCheck;
    public GroundCheck GroundCheck => groundCheck;

    [SerializeField] private AudioPlayer audioPlayer;
    public AudioPlayer AudioPlayer => audioPlayer;

    [SerializeField] private ForceApplier forceApplier;
    public ForceApplier ForceApplier => forceApplier;

    [SerializeField] private AnimationEventReciever animationEventReciever;
    public AnimationEventReciever AnimationEventReciever => animationEventReciever;

    [SerializeField] private VfxPlayerSpawner vfxPlayerSpawner;
    public VfxPlayerSpawner VfxPlayerSpawner => vfxPlayerSpawner;

    [Header("Timers")]
    [SerializeField] private Entropek.Time.OneShotTimer fallCoyoteTimer;


    /// 
    /// Data.
    /// 


    [Header("Data")]
    
    [SerializeField] SkillCollection skillCollection;
    public SkillCollection SkillCollection => skillCollection;

    [RuntimeField] public bool blockMoveInput = false;

    /// 
    /// Runtime Fields
    /// 


    [RuntimeField] private PlayerState playerState = PlayerState.Idle;
    public PlayerState PlayerState => playerState;
    [RuntimeField] private CoyoteState coyoteState = CoyoteState.None;
    public CoyoteState CoyoteState => coyoteState;


    /// 
    /// Animations.
    /// 


    private const string IdleAnimation = "Rig_Sword_Idle";
    private const string WalkAnimation = "Rig_Jog_Fwd_Loop";
    private const string JumpStartAnimation = "Rig_Jump_Start";
    private const string FallAnimation = "Rig_Jump_Loop";
    private const string GroundedAnimation = "Rig_Jump_Land";
    private const string StaggerAnimation = "Rig_Stagger";


    /// 
    /// Data constants.
    /// 
    
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

    private const int Skill1Id = 0;
    private const int Skill2Id = 1;
    private const int Skill3Id = 2;

    private const DamageType StaggerDamageType = DamageType.Heavy;


    /// 
    /// Base.
    /// 


    private void Awake()
    {
        LinkEvents();
    }

    private void Start()
    {   
        SkillHudManager.Singleton.LinkToSkills(skillCollection.Skills);
    }

    private void OnEnable()
    {
        HealthBarHud.Singleton.HealthBar.Activate(health);
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
        Vector3 cameraForwardXZ = new Vector3(cameraController.transform.forward.x, 0, cameraController.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(cameraController.transform.right.x, 0, cameraController.transform.right.z).normalized;
        return moveDirection.x * cameraRightXZ + moveDirection.y * cameraForwardXZ;
    }

    private void HandleFootstepEffects()
    {
        audioPlayer.PlaySound("FootstepGrassLight", transform.position);
    }


    /// 
    /// Facing Direction.
    /// 


    public void FaceMoveDirection()
    {
        FaceChosenDirection = FaceMoveDirectionFixedUpdateCallback;
    }

    private void FaceMoveDirectionFixedUpdateCallback()
    {
        Vector3 moveDirection = characterControllerMovement.moveDirection;
        if (moveDirection != Vector3.zero)
        {
            Entropek.UnityUtils.Transform.RotateYAxisToDirection(transform, characterControllerMovement.moveDirection, FaceMoveDirectionSpeed * Time.deltaTime);
        }
    }

    public void SnapToFaceMoveInput()
    {
        // only snap if there is a direction to snap to.
        // if we snap when input zero, the result is undefined behaviour.

        Vector2 moveInput = InputManager.Singleton.moveInput;

        if (moveInput != Vector2.zero)
        {
            Entropek.UnityUtils.Transform.RotateYAxisToDirection(transform, GetMoveDirectionRelativeToCamera(InputManager.Singleton.moveInput), 1);
        }
    }

    public void FaceAttackDirection()
    {
        FaceChosenDirection = FaceAttackDirectionFixedUpdateCallback;
    }

    private void FaceAttackDirectionFixedUpdateCallback()
    {
        Entropek.UnityUtils.Transform.RotateYAxisToDirection(transform, cameraController.transform.forward, FaceAttackDirectionSpeed * Time.deltaTime);
    }


    ///
    /// Player State Machine.
    /// 


    /// <summary>
    /// Returns the player to a resting state to branch from into different actions; depending upon their endivronmental circumnstances.
    /// </summary>

    public void EnterRestState()
    {
        // clear player state to guarantee that one of the state switches in this function
        // will be set when called.

        playerState = PlayerState.None;

        if (groundCheck.IsGrounded == true) {

            // only reset out dodge when grounded.

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

        characterControllerMovement.moveDirection = GetMoveDirectionRelativeToCamera(InputManager.Singleton.moveInput);
    }

    public void Idle()
    {
        playerState = PlayerState.Idle;
        
        FaceChosenDirection = null; // dont look face any direction when idle.
        characterControllerMovement.moveDirection = GetMoveDirectionRelativeToCamera(Vector2.zero);
                
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
        
        characterControllerMovement.moveDirection = GetMoveDirectionRelativeToCamera(direction);

        if (skillCollection.AnimatedSkillIsInUse() == false)
        {
            FaceMoveDirection();
        }
        
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

    public void Skill1()
    {
        skillCollection.UseSkill(Skill1Id);
    }

    public void Skill2() 
    {
        skillCollection.UseSkill(Skill2Id);
    }

    public void Skill3()
    {
        skillCollection.UseSkill(Skill3Id); 
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
        characterControllerMovement.moveDirection = Vector3.zero;
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

    public void EnterCoyoteState(CoyoteState coyoteState)
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

    public void ExitCoyoteState()
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
        UnlinkHealthEvents();
    }


    ///
    /// Health Event Linkage.
    /// 

    private void LinkHealthEvents()
    {
        health.Damaged += OnDamaged;
    }

    private void UnlinkHealthEvents()
    {
        health.Damaged -= OnDamaged;
    }

    private void OnDamaged(DamageContext damageContext){
        cameraController.StartShaking(DamagedCameraShakeStrength, DamagedCameraShakeTime);

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

        audioPlayer.PlaySound("PlayerHit");

        if((damageContext.DamageType & StaggerDamageType) != 0)
        {
            EnterStagger();
        }
    }

    ///
    /// Ground check event linkage.
    /// 

    private void LinkGroundCheckerEvents()
    {
        groundCheck.Grounded += OnGrounded;
        groundCheck.Airborne += OnAirborne;
    }

    private void UnlinkGroundCheckerEvents()
    {
        groundCheck.Grounded -= OnGrounded;
        groundCheck.Airborne += OnAirborne;
    }

    private void OnGrounded()
    {
        // only enter the rest state if we are currently not performing any skills.

        if (skillCollection.AnimatedSkillIsInUse()==false) 
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
    /// Camera Linkage.
    /// 


    private void LinkCameraEvents() {
        cameraController.Rotated += OnCameraRotated;
    }

    private void UnlinkCameraEvents() {
        cameraController.Rotated -= OnCameraRotated;
    }

    private void OnCameraRotated() {

        if (playerState == PlayerState.Stagger
        || blockMoveInput == true) {
            return;
        }

        characterControllerMovement.moveDirection = GetMoveDirectionRelativeToCamera(InputManager.Singleton.moveInput);
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
        input.Skill3 += OnSkill3Input;
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
        input.Skill3 -= OnSkill3Input;
    }

    private void OnMoveInput(Vector2 moveInput) {

        if(blockMoveInput == true)
        {
            return;
        }

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

        if (coyoteState == CoyoteState.AnimatedSkill
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

        if ((playerState != PlayerState.Idle && playerState != PlayerState.Run) || groundCheck.IsGrounded == false)
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
        if (playerState != PlayerState.Jump
        || skillCollection.AnimatedSkillIsInUse())
        {
            return;
        }

        JumpStop();

    }

    private void OnAttackInput() {

        if (coyoteState == CoyoteState.AnimatedSkill
        || coyoteState == CoyoteState.Stagger)
        {
            CoyoteCallback = Skill1;
        }

        if (skillCollection.SkillIsInUse(Skill1Id)
        ||  playerState == PlayerState.Stagger)
        {
            return;
        }


        Skill1();
    }

    private void OnDodgeInput() {

        if (coyoteState == CoyoteState.AnimatedSkill
        ||  coyoteState == CoyoteState.Stagger)
        {
            CoyoteCallback = Skill2;
        }

        if (skillCollection.SkillIsInUse(Skill2Id)
        ||  playerState == PlayerState.Stagger)
        {
            return;
        }

        Skill2();
    }


    private void OnSkill3Input() {

        if (coyoteState == CoyoteState.AnimatedSkill
        ||  coyoteState == CoyoteState.Stagger)
        {
            CoyoteCallback = Skill3;
        }

        if (skillCollection.SkillIsInUse(Skill3Id)
        ||  playerState == PlayerState.Stagger)
        {
            return;
        }

        Skill3();
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

            case "EnterStaggerCoyoteState": EnterCoyoteState(CoyoteState.Stagger); break;

            // Note:
            //  Exit Coyote state directly when exiting an action/animation state
            //  to ensure any queued actions occur directly after the action has finished.
            //  Do NOT put these exit coyote calls inside the actual functions, that will cause
            //  a recursive loop as exit coyote state calls the stop functions of a given player state. 

            case "ExitStaggerState": ExitStagger(); ExitCoyoteState(); break;
            
            // miscellaneous.
            
            case "Footstep": HandleFootstepEffects(); break;
        }
    }
}
