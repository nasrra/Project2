using UnityEngine;

public class Managers : MonoBehaviour{    
    static class Initialiser{
        // The [RuntimeInitializeOnLoadMethod] attribute will ensure InitializeOnStart is called as soon as the game starts.
        // The RuntimeInitializeLoadType.BeforeSceneLoad ensures the method runs before any scene loads, so it can be used to set up essential components at the start.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init(){
            
            // create gameobject to store managers.
            
            GameObject managersObject = GameObject.Instantiate(new GameObject());
            managersObject.name = "Managers";
            managersObject.AddComponent<Managers>();
            DontDestroyOnLoad(managersObject);

            // add input manager.

            managersObject.AddComponent<InputManager>();
        }
    }
}
