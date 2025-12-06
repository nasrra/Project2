using System;
using Entropek.Audio;
using Entropek.Ui;
using Entropek.UnityUtils.AnimatorUtils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Entropek.Ui
{
    public abstract class MenuButton : MonoBehaviour,
        IPointerEnterHandler,
        IPointerClickHandler
    {


        /// 
        /// Constants.
        /// 


        private const string PointerEnterCompletedAnimationEvent = "PointerEnterCompleted";
        private const string PointerClickCompletedAnimationEvent = "PointerClickCompleted";

        private const string PointerEnterAnimation = "PointerEnter";
        private const string PointerClickAnimation = "PointerClick";

        protected const string PointerEnterSfx = "PointerEnter";
        protected const string PointerClickSfx = "PointerClick";


        ///
        /// Callbacks.
        /// 


        public event Action PointerClicked;
        public event Action PointerEntered;
        public event Action PointerEnterAnimationCompleted;
        public event Action PointerClickAnimationCompleted;


        /// 
        /// Components.
        /// 


        [Header(nameof(MenuButton)+" Components")]
        [SerializeField] Animator animator;
        [SerializeField] AnimationEventReciever animationEventReciever;


        /// 
        /// Base.
        /// 


        private void Awake()
        {
            LinkEvents();
        }

        private void OnDestroy()
        {
            UnlinkEvents();
        }


        /// 
        /// Unique Functions.
        /// 


    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            OnPointerClick(eventData);
            PointerClicked?.Invoke();
        }

        protected virtual void OnPointerClick(PointerEventData eventData)
        {        
            animator.Play(PointerClickAnimation);
            UiManager.Singleton.AudioPlayer.PlaySound(PointerClickSfx);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnter(eventData);
            PointerEntered?.Invoke();
        }

        protected virtual void OnPointerEnter(PointerEventData eventData)
        {        
            animator.Play(PointerEnterAnimation);
            UiManager.Singleton.AudioPlayer.PlaySound(PointerEnterSfx);
        }


        /// 
        /// Event Linkage.
        /// 


        private void LinkEvents()
        {
            LinkAnimationEventRecieverEvents();
        }

        private void UnlinkEvents()
        {
            UnlinkAnimationEventRecieverEvents();
        }


        /// 
        /// AnimationEventReciever Event Linkage.
        /// 


        private void LinkAnimationEventRecieverEvents()
        {
            animationEventReciever.AnimationEventTriggered += OnAnimationEventTriggeredWrapper;
        }

        private void UnlinkAnimationEventRecieverEvents()
        {
            animationEventReciever.AnimationEventTriggered -= OnAnimationEventTriggeredWrapper;        
        }

        private void OnAnimationEventTriggeredWrapper(string eventName)
        {
            OnAnimationEventTriggerd(eventName);
        }

        protected bool OnAnimationEventTriggerd(string eventName)
        {
            switch (eventName)
            {
                case PointerEnterCompletedAnimationEvent:
                    OnPointerEnterAnimationCompletedWrapper();
                    return true;
                case PointerClickCompletedAnimationEvent:
                    OnPointerClickAnimationCompletedWrapper();
                    return true;
                default:
                    return false;
            }
        }

        private void OnPointerEnterAnimationCompletedWrapper()
        {
            PointerEnterAnimationCompleted?.Invoke();
            OnPointerEnterAnimationCompleted();
        }

        protected virtual void OnPointerEnterAnimationCompleted(){}
        
        private void OnPointerClickAnimationCompletedWrapper()
        {
            PointerClickAnimationCompleted?.Invoke();
            OnPointerClickAnimationCompleted(); 
        }
        
        protected virtual void OnPointerClickAnimationCompleted(){}
    }
}

