using UnityEngine;
using UnityEngine.EventSystems;

namespace Entropek.Ui
{
    public class AcceptTempDisplaySettingsButton : MenuButton
    {
        [SerializeField] Canvas TempSettingsConfirmationCanvas;

        protected override void OnPointerClickAnimationCompleted()
        {
            base.OnPointerClickAnimationCompleted();
            TempSettingsConfirmationCanvas.gameObject.SetActive(false);
        }

        protected override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            DisplaySettingsUiElement.TempStartedDisplaySettingsUiElement.AcceptTempSet();
        }

    }    
}

