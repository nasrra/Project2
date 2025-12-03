using Entropek.SceneManaging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneLoadButton : MenuButton
{
    [SerializeField] string sceneToLoad;
    [SerializeField] bool useTransitions = true;

    protected override void OnPointerClick(PointerEventData eventData)
    {
        // do nothing.
    }

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

    protected override void OnPointerEnter(PointerEventData eventData)
    {
        // do nothing.
    }

    protected override void OnPointerEnterAnimationCompleted()
    {
        // do nothing.
    }
}
