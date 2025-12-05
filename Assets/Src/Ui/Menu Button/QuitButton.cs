using UnityEngine;
using UnityEngine.EventSystems;

public class QuitButton : MenuButton
{
    protected override void OnPointerClickAnimationCompleted()
    {
        Application.Quit();   
    }

    protected override void OnPointerEnterAnimationCompleted()
    {
        // do nothing.
    }
}
