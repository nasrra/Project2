using Entropek.Interaction;
using UnityEngine;

namespace Entropek.InventorySystem
{
    public class CurrencyPickup : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Interactable interactable;

        [Header("Data")]
        public Currency Currency;
        public uint Amount;

        private void Awake()
        {
            LinkEvents();
        }

        private void OnDestroy()
        {
            UnlinkEvents();
        }

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
            interactor.GetComponent<Inventory>().AddCurrency(Currency, Amount);
        }
    }    
}

