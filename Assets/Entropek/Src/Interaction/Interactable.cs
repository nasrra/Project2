using System;
using UnityEngine;

namespace Entropek.Interaction{


    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Interactable : MonoBehaviour{

        public event Action<Interactor> Interacted;
        public event Action EnteredInteractorSight;
        public event Action ExitedInteractorSight;
        public event Action EnabledInteraction;
        public event Action DisabledInteraction;
        [SerializeField] private bool IsInteractable = true;
        [SerializeField] public bool IsInSight {get; private set;}

        public void Interact(Interactor interactor){
            if(IsInteractable==true){
                Interacted?.Invoke(interactor);
            }
        }

        public void EnableInteraction(){
            IsInteractable = true;
            EnabledInteraction?.Invoke();
        }

        public void DisableInteraction(){
            IsInteractable = false;
            DisabledInteraction?.Invoke();
        }

        public void EnterInteractorSight(){
            IsInSight = true;
            EnteredInteractorSight?.Invoke();
        }

        public void ExitInteractorSight(){
            IsInSight = false;
            ExitedInteractorSight?.Invoke();
        }

    }


}
