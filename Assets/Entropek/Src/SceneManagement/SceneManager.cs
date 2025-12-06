using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Entropek.SceneManaging
{
    public class CustomSceneManager : MonoBehaviour{

        public static CustomSceneManager Singleton {get; private set;}

        static string sceneToLoad;
        public event Action PreparingSceneLoad, UnloadingScene, LoadedScene, TransitioningScene, LoadedTempScene;
        public event Action<string> LoadingScene, UnloadedScene;

        private const string TempSceneName = "Temp";

        public void LoadScene(string scene){
            PrepareForSceneLoad(scene);
            LoadScene();
        }

        public void LoadSceneWithTransitions(string scene){
            PrepareForSceneLoad(scene);
            StartCoroutine(LoadSceneWithTransitionsCoroutine());
        } 

        private void PrepareForSceneLoad(string scene){
            // InputManager.disable_ui_event_system_input();
            sceneToLoad = scene;
            // AudioManager.dim_sfx_audio();
            // if(SceneInfo.instance.transition == true)
                // transitioning_scene?.Invoke();
        }

        private void LoadScene(){
            StartCoroutine(LoadSceneCoroutine());
        }
        private IEnumerator LoadSceneCoroutine(){
            
            // Wait to unload scene
            Scene active = SceneManager.GetActiveScene();
            string previous_scene = active.name;
            AsyncOperation load;
            AsyncOperation unload;
            
            // load temp
            load = SceneManager.LoadSceneAsync(TempSceneName, LoadSceneMode.Additive);
            PreparingSceneLoad?.Invoke();
            yield return load;
            
            UnloadingScene?.Invoke();
            unload = SceneManager.UnloadSceneAsync(active);
            yield return unload;

            // load scene.
            LoadedTempScene?.Invoke(); // now in temp scene.
            UnloadedScene?.Invoke(previous_scene);
            LoadingScene?.Invoke(sceneToLoad);
            load = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
            yield return load;
            
            LoadedScene?.Invoke();
            ScreenTransitions.Singleton.FadeFromBlack();
            // AudioManager.restore_sfx_audio();
            yield break;
        }

        private IEnumerator LoadSceneWithTransitionsCoroutine(){
            if(ScreenTransitions.Singleton != null){
                ScreenTransitions.Singleton.FadeToBlackCompleted += OnFadeToBlackComplete; // has to be linked beforehand to ensure the IEnumerator instance of the action isnt null.
                ScreenTransitions.Singleton.FadeToBlack();
            }
            else
                LoadScene();
            yield break;
        } 

        private void OnFadeToBlackComplete()
        {
            ScreenTransitions.Singleton.FadeToBlackCompleted -= OnFadeToBlackComplete; 
            LoadScene();
        }


        /// 
        /// Bootstrapper
        /// 


        static class Bootstrap
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            private static void Initialise()
            {
                GameObject main = new();
                main.name = nameof(CustomSceneManager);
                DontDestroyOnLoad(main);
                Singleton = main.AddComponent<CustomSceneManager>();
            }
        }
    }    
}