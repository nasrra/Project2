using System;
using System.Collections;
using UnityEngine;

namespace Entropek.Input
{
    public class Gamepad : MonoBehaviour
    {
        public event Action GamepadConnected;
        public event Action GamepadDisconnected;
        public static Gamepad Singleton { get; private set; }

        /// <summary>
        /// Whether or not a gamepad is connected.
        /// </summary>

        public static bool Connected { get; private set; }

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

        private IEnumerator PollControllers()
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

        private void CheckControllerConnection()
        {
            string[] joystickNames = UnityEngine.Input.GetJoystickNames();

            // if there were no controllers connected last call and the josytick names has entries.

            for (int i = 0; i < joystickNames.Length; i++)
            {
                Debug.Log(joystickNames[i]);
            }
            
            foreach (var device in UnityEngine.InputSystem.InputSystem.devices)
    Debug.Log($"{device.displayName} | {device.layout}");


            if (Connected == false && joystickNames.Length > 0)
            {
                Debug.Log("Gamepad Connected!");
                Connected = true;
                GamepadConnected?.Invoke();
            }

            // if there were controllers connected last call and the joystick names has zero entries.

            else if (Connected == true && joystickNames.Length == 0)
            {
                Debug.Log("Gamepad Disconnected");
                Connected = false;
                GamepadDisconnected?.Invoke();
            }
        }

        private static class Bootstrap
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            private static void Initialise()
            {
                // Singleton creation.
                GameObject main = new();
                main.name = nameof(Gamepad);
                Singleton = main.AddComponent<Gamepad>();

                // Start Polling for controllers.

                Singleton.StartPollingControllers();
            }
        }
    }
    
}

