using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Entropek.SceneManaging
{
    public class CustomSceneManager : MonoBehaviour{

        public static CustomSceneManager Singleton {get; private set;}

        static string scene_to_load;
        public static event Action preparing_scene_load, unloading_scene, loaded_scene, transitioning_scene, temp_scene;
        public static event Action<string> loading_scene, unloaded_scene;

        private const string TempSceneName = "Temp";

        public void LoadScene(string scene){
            PrepareForSceneLoad(scene);
            LoadScene();
        }

        public void LoadSceneWithTransitions(string _scene){
            PrepareForSceneLoad(_scene);
            StartCoroutine(LoadSceneWithTransitionsCoroutine());
        } 

        private void PrepareForSceneLoad(string _scene){
            // InputManager.disable_ui_event_system_input();
            scene_to_load = _scene;
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
            preparing_scene_load?.Invoke();
            yield return load;
            
            unloading_scene?.Invoke();
            unload = SceneManager.UnloadSceneAsync(active);
            yield return unload;

            // load scene.
            temp_scene?.Invoke(); // now in temp scene.
            unloaded_scene?.Invoke(previous_scene);
            loading_scene?.Invoke(scene_to_load);
            load = SceneManager.LoadSceneAsync(scene_to_load, LoadSceneMode.Single);
            yield return load;
            
            loaded_scene?.Invoke();
            ScreenTransitions.Singleton.FadeFromBlack();
            // AudioManager.restore_sfx_audio();
            yield break;
        }

        private IEnumerator LoadSceneWithTransitionsCoroutine(){
            if(ScreenTransitions.Singleton != null){
                ScreenTransitions.Singleton.FadeToBlackCompleted += LoadScene; // has to be linked beforehand to ensure the IEnumerator instance of the action isnt null.
                ScreenTransitions.Singleton.FadeToBlack();
            }
            else
                LoadScene();
            yield break;
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