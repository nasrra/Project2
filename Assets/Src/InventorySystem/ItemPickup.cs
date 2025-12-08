using Entropek.Interaction;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Entropek.Interaction.Interactable interactable;
    public Interactable Interactable => interactable;

    [Header("Data")]
    public Item Item;
    public int Amount;


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
    /// Linkage.
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

    private void OnInteracted(Entropek.Interaction.Interactor interactor)
    {

        // access root gameobject as the interactor gameobject is not expected
        // to contain te inventory component.

        interactor.RootGameObject.GetComponent<Inventory>().AddItem(Item, Amount);
        Destroy(gameObject);
    }

    
}    


