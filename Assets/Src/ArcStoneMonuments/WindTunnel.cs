using System.Reflection;
using UnityEngine;

public class WindTunnel : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Entropek.Interaction.Interactable interactable;
    [SerializeField] ArcField arcField;
    [SerializeField] Entropek.InventorySystem.Currency currencyTypeRequired;
    [SerializeField] uint currentAmountRequired;


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
        Entropek.InventorySystem.Inventory inventory = interactor.RootGameObject.GetComponent<Entropek.InventorySystem.Inventory>();
        if(inventory.HasSufficientCurrency(currencyTypeRequired, currentAmountRequired))
        {
            arcField.Activate();
        }
    }
}
