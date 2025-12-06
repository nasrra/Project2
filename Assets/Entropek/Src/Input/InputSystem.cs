using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Entropek.UnityUtils.Attributes;

namespace Entropek.Input
{
    /// <summary>
    /// An abstract utility class for project specific Input Managers.
    /// </summary>

    public abstract class InputSystem : MonoBehaviour
    {
        public event Action GamepadConnected;
        public event Action GamepadDisconnected;
        public event Action<InputDeviceType> InputDeviceTypeSet;
        [RuntimeField] private InputDeviceType inputDeviceType;
        public InputDeviceType InputDeviceType => inputDeviceType;


        /// <summary>
        /// Whether or not a gamepad is connected.
        /// </summary>

        public bool IsGamepadConnected { get; private set; }

        private Coroutine pollControllersCoroutine;

        /// <summary>
        /// Begins a coroutine that polls the connected controllers every second (realtime).
        /// </summary>

        public void StartPollingControllers()
        {
            CheckControllerConnection();

            if (pollControllersCoroutine != null)
            {
                StopCoroutine(pollControllersCoroutine);
                pollControllersCoroutine = null;
            }
            pollControllersCoroutine = StartCoroutine(PollControllers());
        }

        /// <summary>
        /// A coroutine that checks for connected controllers, looping indefinently.
        /// </summary>
        /// <returns></returns>

        protected IEnumerator PollControllers()
        {
            while (true)
            {
                CheckControllerConnection();

                // wait for realtime seconds to ensure this runs even if
                // the game is paused. 

                yield return new WaitForSecondsRealtime(1);
            }
        }

        /// <summary>
        /// Checks to see if any controllers have been connected or disconnected.
        /// </summary>

        protected void CheckControllerConnection()
        {
            string[] joystickNames = UnityEngine.Input.GetJoystickNames();

            // if there were no controllers connected last call and the josytick names has entries.

            if (IsGamepadConnected == false && joystickNames.Length > 0 && joystickNames[0] != "")
            {
                IsGamepadConnected = true;                
                GamepadConnected?.Invoke();
                SetGamepadDeviceTypeState();
            }

            // if there were controllers connected last call and the joystick names has zero entries.

            else if (IsGamepadConnected == true && joystickNames[0] == "")
            {
                IsGamepadConnected = false;
                GamepadDisconnected?.Invoke();
                SetKeyboardAndMouseDeviceTypeState();
            }
        }

        /// <summary>
        /// Checks whether an input action was caused by a gamepad or mouse and keyboard.
        /// Settings the InputDeviceType depending on which was used.
        /// </summary>
        /// <param name="context">The CallbackContext of an input action.</param>

        protected void HandleInputActionDeviceType(InputAction.CallbackContext context)
        {
            InputControl control = context.control; 
            if(control == null)
            {
                return;
            }

            if(control.device.name == "Keyboard" || control.device.name == "Mouse")
            {
                SetKeyboardAndMouseDeviceTypeState();
            }
            else
            {
                SetGamepadDeviceTypeState();
            }

        }

        /// <summary>
        /// Sets the InputDeviceType to Keyboard and Mouse, invoking the InputDeviceTypeSet callback.
        /// </summary>

        protected void SetKeyboardAndMouseDeviceTypeState()
        {
            inputDeviceType = InputDeviceType.KeyboardAndMouse;
            InputDeviceTypeSet?.Invoke(inputDeviceType);
        }

        /// <summary>
        /// Sets the InputDeviceType to Gamepad, invoking the InputDeviceTypeSet callback.
        /// </summary>

        protected void SetGamepadDeviceTypeState()
        {
            inputDeviceType = InputDeviceType.Gamepad;
            InputDeviceTypeSet?.Invoke(inputDeviceType);            
        }
    }
    
}

