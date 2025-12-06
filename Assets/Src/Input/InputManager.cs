using System;
using Entropek.UnityUtils.Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

[DefaultExecutionOrder(-10)]
public class InputManager : Entropek.Input.InputSystem, InputActions.IGameplayActions{
    
    public static InputManager Singleton {get;private set;}
    public InputActions inputActions {get; private set;}

    void Awake(){
        inputActions = new InputActions();
        // inputActions.Enable(); <-- this enables all input action maps.
        EnableGameplayInput();
    }

    public bool EnableGameplayInput(){
        
        // short ccircuit if the input map is already enabled.
        
        if (inputActions.Gameplay.enabled == true)
        {
            return false;
        }

        // enable.

        inputActions.Gameplay.Enable();
        inputActions.Gameplay.SetCallbacks(this);
        inputActions.Gameplay.Get().actionTriggered += HandleInputActionDeviceType;
        
        return true;
    }

    public bool DisableGameplayInput(){
        
        // short circuit if alread disabled.

        if(inputActions.Gameplay.enabled == false)
        {
            return false;
        }
        
        // disable.

        inputActions.Gameplay.Disable();
        inputActions.Gameplay.RemoveCallbacks(this);
        inputActions.Gameplay.Get().actionTriggered -= HandleInputActionDeviceType;
    
        return true;
    }

    public event Action<Vector2> Move;
    public Vector2 moveInput {get; private set;}
    public float moveInputSqrMagnitude {get;private set;}
    void InputActions.IGameplayActions.OnMove(InputAction.CallbackContext context){
        moveInput = context.ReadValue<Vector2>();
        moveInputSqrMagnitude = moveInput.sqrMagnitude;
        Move?.Invoke(moveInput);
    }

    public Vector3 LookDelta { get; private set; }
    public event Action<Vector2> Look; // delta mouse input.
    void InputActions.IGameplayActions.OnLook(InputAction.CallbackContext context){
        LookDelta = context.ReadValue<Vector2>();
        Look?.Invoke(context.ReadValue<Vector2>());
    }

    public event Action JumpStart;
    public event Action JumpStop;
    public bool IsJumpPressed { get; private set; }
    void InputActions.IGameplayActions.OnJump(InputAction.CallbackContext context){
        if(context.performed==true){
            IsJumpPressed = true;
            JumpStart?.Invoke();
        }
        else if(context.canceled==true){
            IsJumpPressed = false;
            JumpStop?.Invoke();
        }
    }

    [RuntimeField] private bool primarySkillPressed;
    public bool PrimarySkillPressed => primarySkillPressed;
    public event Action PrimarySkill;
    void InputActions.IGameplayActions.OnPrimarySkill(InputAction.CallbackContext context){
        if(context.performed==true){
            primarySkillPressed = true;
            PrimarySkill?.Invoke();
        }

        else if (context.canceled == true)
        {
            primarySkillPressed = false;
        }
    }

    [RuntimeField] private bool secondarySkillPressed;
    public bool SecondarySkillPressed => secondarySkillPressed;
    public event Action SecondarySkill;
    void InputActions.IGameplayActions.OnSecondarySkill(InputAction.CallbackContext context){
        
        if(context.performed==true){
            secondarySkillPressed = true;
            SecondarySkill?.Invoke();
        }

        else if (context.canceled == true)
        {
            secondarySkillPressed = false;
        }
    }
    
    [RuntimeField] private bool utilitySkillPressed;
    public bool UtilitySkillPressed => utilitySkillPressed;
    public event Action UtilitySkill;
    void InputActions.IGameplayActions.OnUtilitySkill(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            utilitySkillPressed = true;
            UtilitySkill?.Invoke();
        }

        else if(context.canceled)
        {
            utilitySkillPressed = false;
        }
    }

    [RuntimeField] private bool specialSkillPressed;
    public bool SpecialSkillPressed => specialSkillPressed;
    public event Action SpecialSkill;
    void InputActions.IGameplayActions.OnSpecialSkill(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            specialSkillPressed = true;
            SpecialSkill?.Invoke();
        }

        else if (context.canceled)
        {
            specialSkillPressed = false;
        }
    }

    public event Action LockOnToggle;
    void InputActions.IGameplayActions.OnLockOnToggle(InputAction.CallbackContext context){
        if(context.performed==true){
            LockOnToggle?.Invoke();
        }
    }

    public event Action LockOnNext;
    void InputActions.IGameplayActions.OnLockOnNext(InputAction.CallbackContext context){
        if(context.performed==true){
            LockOnNext?.Invoke();
        }
    }

    public event Action LockOnPrevious;
    void InputActions.IGameplayActions.OnLockOnPrevious(InputAction.CallbackContext context){
        if(context.performed==true){
            LockOnPrevious?.Invoke();
        }
    }

    public event Action Interact;
    void InputActions.IGameplayActions.OnInteract(InputAction.CallbackContext context){
        if(context.performed==true){
            Interact?.Invoke();
        }
    }

    public event Action NextInteractable;
    void InputActions.IGameplayActions.OnNextInteractable(InputAction.CallbackContext context){
        if(context.performed){
            NextInteractable?.Invoke();
        }
    }

    public event Action PreviousInteractable;
    void InputActions.IGameplayActions.OnPreviousInteractable(InputAction.CallbackContext context){
        if(context.performed){
            PreviousInteractable?.Invoke();
        }
    }

    public event Action RunToggle;
    void InputActions.IGameplayActions.OnRunToggle(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            RunToggle?.Invoke();
        }
    }

    public event Action PauseToggle;
    void InputActions.IGameplayActions.OnPauseToggle(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PauseToggle?.Invoke();
        }
    }


    /// 
    /// Bootstrap Singleton.
    /// 


    private static class Bootstrap{
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialise(){            
            GameObject main = new();
            main.name = "InputManager";
            Singleton = main.AddComponent<InputManager>();
            DontDestroyOnLoad(main);

            // Start Polling for controllers.

            Singleton.StartPollingControllers();
        }
    }
}
