using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-10)]
public class InputManager : MonoBehaviour, InputActions.IGameplayActions{
    
    public static InputManager Singleton;
    public InputActions inputActions {get; private set;}

    public void Awake(){
        if(Singleton==null){
            Singleton=this;
        }
        else{
            throw new Exception("Only one Input Manager can be active!");
        }
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
    public Vector2 moveInput;
    void InputActions.IGameplayActions.OnMove(InputAction.CallbackContext context){
        moveInput = context.ReadValue<Vector2>();
        Move?.Invoke(moveInput);
    }

    public event Action<Vector2> Look; // delta mouse input.
    void InputActions.IGameplayActions.OnLook(InputAction.CallbackContext context){
        Look?.Invoke(context.ReadValue<Vector2>());
    }

    public event Action JumpStart;
    public event Action JumpStop;
    void InputActions.IGameplayActions.OnJump(InputAction.CallbackContext context){
        if(context.performed==true){
            JumpStart?.Invoke();
        }
        else if(context.canceled==true){
            JumpStop?.Invoke();
        }
    }
}
