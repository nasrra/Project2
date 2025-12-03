using Entropek.SceneManaging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneLoadButton : MenuButton
{
    [SerializeField] string sceneToLoad;

    protected override void OnPointerClick(PointerEventData eventData)
    {
        // do nothing.
    }

    protected override void OnPointerClickAnimationCompleted()
    {
        CustomSceneManager.Singleton.LoadScene(sceneToLoad);
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
