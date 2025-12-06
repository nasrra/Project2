using Entropek.SceneManaging;
using UnityEngine;

namespace Entropek.Ui
{    
    public class SceneLoadButton : MenuButton
    {
        [SerializeField] string sceneToLoad;
        [SerializeField] bool useTransitions = true;

        protected override void OnPointerClickAnimationCompleted()
        {
            if (useTransitions == true)
            {
                CustomSceneManager.Singleton.LoadSceneWithTransitions(sceneToLoad);            
            }
            else
            {
                CustomSceneManager.Singleton.LoadScene(sceneToLoad);
            }
        }

        protected override void OnPointerEnterAnimationCompleted()
        {
            // do nothing.
        }
    }
}

