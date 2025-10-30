using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

[DefaultExecutionOrder(-10)]
public class InputManager : MonoBehaviour, InputActions.IGameplayActions{
    
    public static InputManager Singleton {get;private set;}

    public InputActions inputActions {get; private set;}

    void Awake(){
        inputActions = new InputActions();
        inputActions.Enable();
        EnableGameplayInput();
    }

    public void EnableGameplayInput(){
        inputActions.Gameplay.Enable();
        inputActions.Gameplay.SetCallbacks(this);
    }

    public void DisableGameplayInput(){
        inputActions.Gameplay.Disable();
        inputActions.Gameplay.RemoveCallbacks(this);
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

    public event Action Attack;
    void InputActions.IGameplayActions.OnAttack(InputAction.CallbackContext context){
        if(context.performed==true){
            Attack?.Invoke();
        }
    }

    public event Action Dodge;
    void InputActions.IGameplayActions.OnDodge(InputAction.CallbackContext context){
        if(context.performed==true){
            Dodge?.Invoke();
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

    private static class Bootstrap{
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialise(){            
            GameObject main = new();
            main.name = "InputManager";
            main.AddComponent<InputManager>();
            DontDestroyOnLoad(main);
            Singleton=main.GetComponent<InputManager>();
        }
    }
}
