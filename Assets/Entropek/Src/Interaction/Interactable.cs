using System;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Interaction{


    public class Interactable : MonoBehaviour{


        /// <summary>
        /// Callbacks.
        /// </summary>


        public event Action<Interactor> Interacted;
        public event Action<Interactor> EnteredInteractorSight;
        public event Action<Interactor> ExitedInteractorSight;

        public event Action<Interactor> EnteredInteractorRange;
        public event Action<Interactor> ExitedInteractorRange;

        public event Action EnabledInteraction;
        public event Action DisabledInteraction;

        [SerializeField] private bool isInteractable = true;
        public bool IsInteractable => isInteractable;

        [RuntimeField] InteractableState interactableState = InteractableState.NotInSight;
        public InteractableState InteractableState => interactableState;

        [SerializeField] private LayerMask interactorLayer;

        public void Interact(Interactor interactor){
            if(isInteractable==true){
                Interacted?.Invoke(interactor);
            }
        }

        public void EnableInteraction(){
            EnabledInteraction?.Invoke();
        }

        public void DisableInteraction(){
            isInteractable = false;
            DisabledInteraction?.Invoke();
        }

        public void EnterInteractorSight(Interactor interactor){
            interactableState |= InteractableState.InSight;
            interactableState &= ~InteractableState.NotInSight;
            EnteredInteractorSight?.Invoke(interactor);
        }

        public void ExitInteractorSight(Interactor interactor){
            interactableState &= ~InteractableState.InSight;
            interactableState |= InteractableState.NotInSight;
            ExitedInteractorSight?.Invoke(interactor);
        }

        public void EnterInteractorRange(Interactor interactor)
        {
            interactableState |= InteractableState.InRange;
            EnteredInteractorRange?.Invoke(interactor);
        }

        public void ExitInteractorRange(Interactor interactor)
        {
            interactableState &= ~InteractableState.InRange;
            ExitedInteractorRange?.Invoke(interactor);
        }

    }


}
