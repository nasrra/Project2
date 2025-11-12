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

    [RuntimeField] private bool skill1Pressed;
    public bool Skill1Pressed => skill1Pressed;
    public event Action Skill1;
    void InputActions.IGameplayActions.OnSkill1(InputAction.CallbackContext context){
        if(context.performed==true){
            skill1Pressed = true;
            Skill1?.Invoke();
        }

        if (context.canceled == true)
        {
            skill1Pressed = false;
        }
    }

    [RuntimeField] private bool skill2Pressed;
    public bool Skill2Pressed => skill2Pressed;
    public event Action Skill2;
    void InputActions.IGameplayActions.OnSkill2(InputAction.CallbackContext context){
        
        if(context.performed==true){
            skill2Pressed = true;
            Skill2?.Invoke();
        }

        if (context.canceled == true)
        {
            skill2Pressed = false;
        }
    }
    
    [RuntimeField] private bool skill3Pressed;
    public bool Skill3Pressed => skill3Pressed;
    public event Action Skill3;
    void InputActions.IGameplayActions.OnSkill3(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            skill3Pressed = true;
            Skill3?.Invoke();
        }

        if(context.canceled)
        {
            skill3Pressed = false;
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
