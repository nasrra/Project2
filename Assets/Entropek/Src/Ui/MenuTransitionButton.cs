using Entropek.Ui;
using UnityEngine;


namespace Entropek.Ui
{
    public class MenuTransitionButton : MenuButton
    {
        [Header(nameof(MenuButton)+" Components")]
        [SerializeField] Canvas currentMenu;
        [SerializeField] Canvas nextMenu;

        private const string TransitionSfx = "MenuTransition";

        protected override void OnPointerClickAnimationCompleted()
        {
            currentMenu.gameObject.SetActive(false);
            nextMenu.gameObject.SetActive(true);
            UiManager.Singleton.AudioPlayer.PlaySound(TransitionSfx);
        }

        protected override void OnPointerEnterAnimationCompleted()
        {
        }
    }    
}
