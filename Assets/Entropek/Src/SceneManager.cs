using UnityEngine;

namespace Entropek.SceneManagement
{
    public class SceneManager : MonoBehaviour
    {
        // IEnumerator LoadSceneWithCleanup(string sceneName)
        // {
        //     yield return SceneManager.LoadSceneAsync(sceneName);
        //     yield return Resources.UnloadUnusedAssets(); // deallocate any unused scriptable objects, assets, textures, models, etc.
        //     System.GC.Collect(); // deallocate any unused managed heap
        // }
    }
    
}

