using System.Collections;
using Entropek;
using UnityEngine;

namespace Entropek.Ui
{
    public abstract class DisplaySettingsUiElement : MonoBehaviour
    {

        public static DisplaySettingsUiElement TempStartedDisplaySettingsUiElement {get; private set;}


        /// 
        /// Constants.
        /// 


        protected const int TempSetTimeInSeconds = 3;
        protected const string TempSetTextPrimary = "Accept Display Settings Change?";
        protected const string TempSetTextSecondary = "(Changes will revert in 3 seconds)";


        /// 
        /// Components.
        /// 


        [Header(nameof(DisplaySettingsUiElement)+" Components")]
        [SerializeField] private Canvas tempSetMenu;
        [SerializeField] private MenuButton acceptButton;
        [SerializeField] private MenuButton revertButton;

        private Coroutine tempSetCoroutine;


        /// 
        /// Base.
        /// 


        protected void Awake()
        {
            LoadValue();
            LinkEvents();
        }

        protected void OnDestroy()
        {
            UnlinkEvents();
        }


        /// 
        /// Unique Functions.
        /// 


        protected abstract void LoadValue();
        

        /// <summary>
        /// Resets the displayed value of the Ui Element to the last saved value.
        /// </summary>

        private void ResetValue()
        {
            UnlinkUiElementEvents();
            DisplaySettingsManager.Singleton.LoadPlayerPrefs();
            LoadValue();
            LinkUiElementEvents();    
        }

        /// <summary>
        /// Temporarily enters a state where after a specified amount of time, resets the assigned value of the ui element.
        /// </summary>
        /// <param name="time">The amount of time to wait before resetting the value.</param>
        
        protected void StartTempSet(float time = TempSetTimeInSeconds)
        {
            TempStartedDisplaySettingsUiElement = this;
            tempSetCoroutine = StartCoroutine(TempSetCoroutine());
        }

        private IEnumerator TempSetCoroutine()
        {
            tempSetMenu.gameObject.SetActive(true);
            
            // block the pause toggle so the display setting cant be cancelled out
            // of during gameplay.

            InputManager.Singleton.BlockPauseMenuToggle = true;

            yield return new WaitForSecondsRealtime(TempSetTimeInSeconds);

            tempSetMenu.gameObject.SetActive(false);

            RevertTempSet();

            InputManager.Singleton.BlockPauseMenuToggle = false;

            yield break;
        }


        /// <summary>
        /// Stops the temp set coroutine and resets the Ui Element value.
        /// </summary>

        public void RevertTempSet()
        {
            ResetValue();
            StopCoroutine(tempSetCoroutine);
        }

        /// <summary>
        /// Stops the temp set coroutine and saves the currently assigned ui element value as the new default.
        /// </summary>

        public void AcceptTempSet()
        {
            DisplaySettingsManager.Singleton.SavePlayerPrefs();
            StopCoroutine(tempSetCoroutine);   
        }


        /// 
        /// Linkage.
        /// 

        
        private void LinkEvents()
        {
            LinkUiElementEvents();
        }

        private void UnlinkEvents()
        {
            UnlinkUiElementEvents();
        }

        protected abstract void LinkUiElementEvents();
        protected abstract void UnlinkUiElementEvents();
    }
}

