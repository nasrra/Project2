using UnityEngine;

namespace Entropek.Ui
{    
    public class ResumeGameButton : MenuButton
    {
        protected override void OnPointerClickAnimationCompleted()
        {
            base.OnPointerClickAnimationCompleted();
            GameManager.Singleton.ResumeGame();
        }
    }
}

