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
    /// Data constants.
    /// 

    private const string IdleAnimation = "HoldingIdle";
    private const string WalkAnimation = "RunWithSword";
    private const string RunAnimation = "Sprint";
    private const string JumpStartAnimation = "Jump";
    private const string FallAnimation = "Fall";
    private const string GroundedAnimation = "Rig_Jump_Land";
    private const string StaggerAnimation = "Rig_Stagger";
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
    private const int PrimarySkillId = 0;
    private const int SecondarySkillId = 1;
    private const int UtilitySkillId = 2;
    private const int SpecialSkillId = 3;
    private const int MaxJumpCount = 2;
    private const DamageType StaggerDamageType = DamageType.Heavy;


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
    
    [SerializeField] private CameraAimTarget cameraAimTarget;
    public CameraAimTarget CameraAimTarget => cameraAimTarget;

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

    [SerializeField] private AnimationEventReciever animationEventReciever;
    public AnimationEventReciever AnimationEventReciever => animationEventReciever;

    [SerializeField] private VfxPlayerSpawner vfxPlayerSpawner;
    public VfxPlayerSpawner VfxPlayerSpawner => vfxPlayerSpawner;

    [SerializeField] private PlayerStats playerStats;
    public PlayerStats PlayerStats => playerStats;

    [SerializeField] private Inventory inventory;
    public Inventory Inventory => inventory;

    [Header("Timers")]
    [SerializeField] private Entropek.Time.OneShotTimer fallCoyoteTimer;


    /// 
    /// Data.
    /// 


    [Header("Data")]
    
    [SerializeField] SkillCollection skillCollection;
    public SkillCollection SkillCollection => skillCollection;

    [RuntimeField] private bool blockRunToggleInput = false;
    [RuntimeField] private bool blockMoveInput = false;
    [RuntimeField] private bool blockJumpInput = false;


    /// 
    /// Runtime Fields
    /// 


    [RuntimeField] private CharacterHorizontalMoveState horizontalMoveState = CharacterHorizontalMoveState.Walk;
    [RuntimeField] private CharacterVerticalMoveState verticalMoveState = CharacterVerticalMoveState.None; 
    [RuntimeField] private CoyoteState coyoteState = CoyoteState.None;
    public CoyoteState CoyoteState => coyoteState;
    private string moveAnimation {get; set;} = WalkAnimation; // always start in the walk state.
    [RuntimeField] private int jumpCount;
    [RuntimeField] private bool staggered = false;


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
    /// Default Player States.
    /// 


    /// <summary>
    /// Returns the player to a resting state to branch from into different actions; depending upon their endivronmental circumnstances.
    /// </summary>

    public void EnterRestState()
    {
        // clear player state to guarantee that one of the state switches in this function
        // will be set when called.

        if (groundCheck.IsGrounded == true) {

            // only reset out dodge when grounded.

            if (InputManager.Singleton.moveInputSqrMagnitude > 0) {
                Move(InputManager.Singleton.moveInput);
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
        FaceChosenDirection = null; // dont look face any direction when idle.
        characterControllerMovement.moveDirection = GetMoveDirectionRelativeToCamera(Vector2.zero);
                
        if (verticalMoveState == CharacterVerticalMoveState.None)
        {
            animator.Play(IdleAnimation);
        }
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
        staggered = true;
        characterControllerMovement.moveDirection = Vector3.zero;
        animator.Play(StaggerAnimation);
    }

    public void ExitStagger()
    {
        staggered = false;
        EnterRestState();
    }



    /// 
    /// Horizontal Movement Player States.
    /// 


    public void Move(Vector2 direction) {

        // verify that we can run in a given direction if run is toggled.

        if (horizontalMoveState == CharacterHorizontalMoveState.Run && IsMoveDirectionRunnable(direction) == false)
        {
            EnterWalkState();
        }   

        characterControllerMovement.moveDirection = GetMoveDirectionRelativeToCamera(direction);
        if (skillCollection.AnimatedSkillIsInUse(out _) == false)
        {
            FaceMoveDirection();
        }

        if(verticalMoveState == CharacterVerticalMoveState.None)
        {
            animator.Play(moveAnimation);            
        }

    }

    private void ToggleRun()
    {
        switch (horizontalMoveState)
        {
            case CharacterHorizontalMoveState.Walk:
                EnterRunState();
                break;
            case CharacterHorizontalMoveState.Run:
                EnterWalkState();
                break;
            default:
                break;
        }

        if(characterControllerMovement.moveDirection != Vector3.zero && verticalMoveState == CharacterVerticalMoveState.None)
        {
            animator.Play(moveAnimation);
        }

    } 

    public void EnterRunState()
    {   
        horizontalMoveState = CharacterHorizontalMoveState.Run;
        moveAnimation = RunAnimation;
        characterControllerMovement.SetMaxSpeed(playerStats.RunMaxSpeed.ScaledValue);
        cameraController.StartLerpingFov(90, 0.33f);
    }

    public void EnterWalkState()
    {
        horizontalMoveState = CharacterHorizontalMoveState.Walk;
        moveAnimation = WalkAnimation;
        characterControllerMovement.SetMaxSpeed(playerStats.WalkMaxSpeed.ScaledValue);        
        cameraController.StartLerpingFov(CameraController.InitialFov, 0.167f);
    }

    /// <summary>
    /// Checks whether or not a move direction vector can be moved in the Run PlayerMovementState.
    /// </summary>
    /// <returns>true, if the move direction can be ran in; otherwise false</returns>

    private bool IsMoveDirectionRunnable(Vector2 moveDirection)
    {
        return
        moveDirection.x <= 0.85
        && moveDirection.x >= -0.85
        && moveDirection.y >= 0;
    }


    /// 
    /// Vertical Movement Player States.
    /// 


    public void JumpStart() {
        verticalMoveState = CharacterVerticalMoveState.Jump;
        jumpMovement.StartJumping();
        animator.Play(JumpStartAnimation);
        characterControllerMovement.ClearGravityVelocity();
    }

    public void JumpStop() {
        verticalMoveState = CharacterVerticalMoveState.Jump;
        jumpMovement.StopJumping();
        EnterRestState();
    }

    public void Fall() {
        verticalMoveState = CharacterVerticalMoveState.Fall;
        animator.Play(FallAnimation);
    }


    /// 
    /// Skill Functions.
    /// 


    public void PrimarySkill()
    {
        skillCollection.UseSkill(PrimarySkillId);
    }

    public void SecondarySkill() 
    {
        skillCollection.UseSkill(SecondarySkillId);
    }

    public void UtilitySkill()
    {
        skillCollection.UseSkill(UtilitySkillId); 
    }

    public void SpecialSkill()
    {
        skillCollection.UseSkill(SpecialSkillId);
    }


    ///
    /// Input Blockers.
    /// 

    public void BlockMoveInput()
    {
        blockMoveInput = true;
    }

    public void UnblockMoveInput()
    {
        blockMoveInput = false;        
    }

    public void BlockJumpInput()
    {
        blockJumpInput = true;                
    }

    public void UnblockJumpInput()
    {
        blockJumpInput = false;        
    }

    public void BlockRunToggleInput()
    {
        blockRunToggleInput = true;
    }

    public void UnblockRunToggleInput()
    {
        blockRunToggleInput = false;
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
        if (InputManager.Singleton.PrimarySkillPressed)
        {
            PrimarySkill();
        }
        else if (InputManager.Singleton.SecondarySkillPressed)
        {
            SecondarySkill();
        }
        else if (InputManager.Singleton.UtilitySkillPressed)
        {
            UtilitySkill();
        }
        else if (InputManager.Singleton.SpecialSkillPressed)
        {
            SpecialSkill();
        }
        else{            
            CoyoteCallback?.Invoke();
        }

        CoyoteCallback = null;

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
        LinkInventoryEvents();
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
        UnlinkInventoryEvents();
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
        verticalMoveState = CharacterVerticalMoveState.None;
        // only enter the rest state if we are currently not performing any skills.

        if (skillCollection.AnimatedSkillIsInUse(out _)==false) 
        {
            if (InputManager.Singleton.moveInputSqrMagnitude > 0)
            {
                Move(InputManager.Singleton.moveInput);
            }
            else
            {
                Idle();
            }
        }


        jumpCount = 0;
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

        if (staggered == true
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

        // move input.

        input.Move += OnMoveInput;
        input.RunToggle += OnRunToggleInput;
        input.JumpStart += OnJumpStartInput;
        input.JumpStop += OnJumpStopInput;
        
        // interact input.

        input.Interact += OnInteractInput;
        input.NextInteractable += OnNextInteractable;
        input.PreviousInteractable += OnPreviousInteractable;
        
        // skill input.

        input.PrimarySkill += OnPrimarySkillInput;
        input.SecondarySkill += OnSecondarySkillInput;
        input.UtilitySkill += OnUtilitySkillInput;
        input.SpecialSkill += OnSpecialSkillInput;
    }

    private void UnlinkInputEvents() {

        InputManager input = InputManager.Singleton;

        // move input.

        input.Move -= OnMoveInput;
        input.RunToggle -= OnRunToggleInput;
        input.JumpStart -= OnJumpStartInput;
        input.JumpStop -= OnJumpStopInput;
        
        // interact input.

        input.Interact -= OnInteractInput;
        input.NextInteractable -= OnNextInteractable;
        input.PreviousInteractable -= OnPreviousInteractable;
        
        // Skill input.
        
        input.PrimarySkill -= OnPrimarySkillInput;
        input.SecondarySkill -= OnSecondarySkillInput;
        input.UtilitySkill -= OnUtilitySkillInput;
        input.SpecialSkill -= OnSpecialSkillInput;
    }

    private void OnMoveInput(Vector2 moveInput) {

        if(blockMoveInput == true || staggered == true)
        {
            return;
        }

        if (moveInput.sqrMagnitude == 0) {
            EnterWalkState();
            EnterRestState();
        }
        else {
            Move(InputManager.Singleton.moveInput);
        }
    }

    private void OnJumpStartInput() {

        if (
        (groundCheck.IsGrounded == true || jumpCount < MaxJumpCount)
        &&  blockJumpInput == false)
        {
            JumpStart();
            jumpCount++;
        }
        else
        {
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
        }
    }

    private void OnJumpInputCoyoteCallback()
    {
        if (InputManager.Singleton.IsJumpPressed == true)
        {
            // block check to ensure that exiting and re-entering a coyote state doesn't 
            // trigger a jump start if both states dont allow it.

            if(blockJumpInput == false)
            {
                JumpStart();
            }
        }
    }

    private void OnJumpStopInput()
    {
        if (verticalMoveState != CharacterVerticalMoveState.Jump
        || skillCollection.AnimatedSkillIsInUse(out _))
        {
            return;
        }

        JumpStop();

    }

    private void OnPrimarySkillInput() 
    {
        OnSkillInput(PrimarySkill, PrimarySkillId);
    }

    private void OnSecondarySkillInput() 
    {
        OnSkillInput(SecondarySkill, SecondarySkillId);
    }

    private void OnUtilitySkillInput() 
    {
        OnSkillInput(UtilitySkill, UtilitySkillId);
    }

    private void OnSpecialSkillInput()
    {
        OnSkillInput(SpecialSkill, SpecialSkillId);    
    }

    private void OnSkillInput(Action skillFunction, int skillId)
    {
        if (coyoteState == CoyoteState.AnimatedSkill
        ||  coyoteState == CoyoteState.Stagger)
        {
            CoyoteCallback = skillFunction;
        }

        if (skillCollection.SkillIsInUse(skillId)
        ||  staggered == true)
        {
            return;
        }

        skillFunction();        
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

    private void OnRunToggleInput()
    {
        if(blockRunToggleInput == true)
        {
            return;
        }

        if (IsMoveDirectionRunnable(InputManager.Singleton.moveInput))
        {
            ToggleRun();
        }
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


    ///
    /// Inventory Event Linkage.
    /// 


    private void LinkInventoryEvents()
    {
        inventory.ItemAdded += OnItemAdded;
        inventory.ItemRemoved += OnItemRemoved;
    }

    private void UnlinkInventoryEvents()
    {
        inventory.ItemAdded -= OnItemAdded;
        inventory.ItemRemoved -= OnItemRemoved;
    }

    private void OnItemAdded(ItemAddedContext itemAddedContext)
    {
        for(int i = 0; i < itemAddedContext.Amount; i++)
        {
            itemAddedContext.Item.ApplyModifier(playerStats);
        }
    }

    private void OnItemRemoved(ItemRemovedContext itemRemovedContext)
    {
        for(int i = 0; i < itemRemovedContext.Amount; i++)
        {
            itemRemovedContext.Item.RemoveModifier(playerStats);
        }
    }
}
