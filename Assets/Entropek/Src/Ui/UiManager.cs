using System.Collections.Generic;
using Entropek.Audio;
using Entropek.Collections;
using UnityEngine;

namespace Entropek.Ui
{    
    public class UiManager : MonoBehaviour
    {
        public static UiManager Singleton {get; private set;}

        /// <summary>
        /// An AudioPlayer for Ui elements to use; so that audio persists even when
        /// the UiElement Gameobject has been disabled.
        /// </summary>

        public AudioPlayer AudioPlayer;

        private static class Bootstrap
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            private static void Initialise()
            {
                GameObject main = new GameObject();
                main.name = nameof(UiManager);
                Singleton = main.AddComponent<UiManager>();
                Singleton.AudioPlayer = main.AddComponent<AudioPlayer>();
                DontDestroyOnLoad(main);
            }
        }
    }
}

