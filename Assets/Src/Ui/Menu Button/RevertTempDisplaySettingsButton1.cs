using UnityEngine;
using UnityEngine.EventSystems;

public class RevertTempDisplaySettingsButton : MenuButton
{
    [SerializeField] Canvas TempSettingsConfirmationCanvas;

    protected override void OnPointerClickAnimationCompleted()
    {
        base.OnPointerClickAnimationCompleted();
        TempSettingsConfirmationCanvas.gameObject.SetActive(false);
    }

    protected override void OnPointerClick(PointerEventData eventData)
    {
        DisplaySettingsUiElement.TempStartedDisplaySettingsUiElement.RevertTempSet();
        base.OnPointerClick(eventData);
    }

}
