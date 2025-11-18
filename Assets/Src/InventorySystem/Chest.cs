using Entropek.Interaction;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private ItemDropper itemDropper;
    [SerializeField] private Interactable interactable;


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
    }

    private void UnlinkEvents()
    {
        UnlinkInteractableEvents();
    }

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
        itemDropper.DropItem();
        // interactable.DisableInteraction();
    }
}
