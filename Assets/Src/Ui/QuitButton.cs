using UnityEngine;
using UnityEngine.EventSystems;

public class QuitButton : MenuButton
{
    protected override void OnPointerClick(PointerEventData eventData)
    {
        // do nothing.
    }

    protected override void OnPointerClickAnimationCompleted()
    {
        Application.Quit();   
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
