using System;
using UnityEngine;

namespace Entropek.Systems.Interaction{


[RequireComponent(typeof(SphereCollider))]
public class Interactable : MonoBehaviour{


    public event Action Interacted;
    public event Action EnteredInteractorSight;
    public event Action ExitedInteractorSight;
    public event Action EnabledInteraction;
    public event Action DisabledInteraction;
    [SerializeField] private bool IsInteractable = true;
    [SerializeField] public bool IsInSight {get; private set;}

    public void Interact(){
        if(IsInteractable==true){
            Interacted?.Invoke();
            Destroy(gameObject);
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
