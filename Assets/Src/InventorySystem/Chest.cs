using Entropek.Interaction;
using Entropek.UnityUtils.AnimatorUtils;
using TMPro;
using UnityEngine;

public class Chest : MonoBehaviour
{
    private const string DropItemAnimationEvent = "DropItem";
    private const string OpenAnimation = "chest_opening";

    [Header("Components")]
    [SerializeField] private ItemDropper itemDropper;
    [SerializeField] private Interactable interactable;
    [SerializeField] private CurrencyRequirement currencyRequirement;
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationEventReciever animationEventReciever;


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
    /// Event Linkage.
    /// 


    private void LinkEvents()
    {
        LinkInteractableEvents();
        LinkAnimationEventRecieverEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkInteractableEvents();
        UnlinkAnimationEventRecieverEvents();
    }


    /// 
    /// Interactable Linkage.
    /// 


    private void LinkInteractableEvents()
    {
        interactable.Interacted += OnInteracted; 
    }

    private void UnlinkInteractableEvents()
    {
        interactable.Interacted -= OnInteracted;
    }

    private void OnInteracted(Interactor interactor)
    {
        if (currencyRequirement.FullfillRequirement(interactor.RootGameObject) == true)
        {            
            animator.Play(OpenAnimation);
            interactable.DisableInteraction();
        }
    }


    ///
    /// Animation Event Linkage.
    /// 

    private void LinkAnimationEventRecieverEvents()
    {
        animationEventReciever.AnimationEventTriggered += OnAnimationEventTriggered;
    }

    private void UnlinkAnimationEventRecieverEvents()
    {
        animationEventReciever.AnimationEventTriggered -= OnAnimationEventTriggered;        
    }

    private void OnAnimationEventTriggered(string eventName)
    {
        switch (eventName)
        {
            case DropItemAnimationEvent:
                OnDropItemAnimationEvent();
            break;
        }
    }

    private void OnDropItemAnimationEvent()
    {
        itemDropper.DropItem();
    }

}
