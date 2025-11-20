using Entropek.Interaction;
using TMPro;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private ItemDropper itemDropper;
    [SerializeField] private Interactable interactable;
    [SerializeField] private CurrencyRequirement currencyRequirement;
    [SerializeField] private TextLookAt textLookAt;
    [SerializeField] private TextMeshPro text;


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
    /// Functions.
    /// 


    private void EnableCurrencyRequirementText(Transform target)
    {
        text.gameObject.SetActive(true);
        textLookAt.LookAtTarget = target;
    }

    private void DisableCurrencyRequirementText()
    {
        text.gameObject.SetActive(false);
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
        interactable.EnteredInteractorSight += OnEnteredInteractorSight;
        interactable.ExitedInteractorSight += OnExitedInteractorSight;
        interactable.DisabledInteraction += OnDisabledInteraction;
    }

    private void UnlinkInteractableEvents()
    {
        interactable.Interacted -= OnInteracted;
        interactable.EnteredInteractorSight -= OnEnteredInteractorSight;
        interactable.ExitedInteractorSight -= OnExitedInteractorSight;
    }

    private void OnInteracted(Interactor interactor)
    {
        if (currencyRequirement.FullfillRequirement(interactor.RootGameObject) == true)
        {            
            itemDropper.DropItem();
            interactable.DisableInteraction();
            DisableCurrencyRequirementText();
        }
    }

    private void OnEnteredInteractorSight(Interactor interactor)
    {
        if (interactable.IsInteractable)
        {
            EnableCurrencyRequirementText(interactor.transform);
        }
    }

    private void OnExitedInteractorSight(Interactor interactor)
    {
        if (interactable.IsInteractable)
        {
            DisableCurrencyRequirementText();
        }
    }

    private void OnDisabledInteraction()
    {
        DisableCurrencyRequirementText();        
    }
}
