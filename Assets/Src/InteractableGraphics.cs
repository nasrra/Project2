using Entropek.Interaction;
using Unity.VisualScripting;
using UnityEngine;

public class InteractableGraphics : MonoBehaviour
{
    [SerializeField] Interactable interactable;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Outline outline;

    private void Awake()
    {
        LinkEvents();
    }

    private void OnDestroy()
    {
        UnlinkEvents();
    }

    private void EnterInRangeState()
    {
        meshRenderer.materials[0].SetFloat("_OverlayAmount", 0.167f);
        outline.renderers[0].materials[2].SetFloat("_OutlineWidth", 10);        
    }

    private void ExitInRangeState()
    {
        meshRenderer.materials[0].SetFloat("_OverlayAmount", 0);
        outline.renderers[0].materials[2].SetFloat("_OutlineWidth", 0);        
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
        interactable.EnteredInteractorRange += OnEnterInteractorRange; 
        interactable.ExitedInteractorRange += OnExitInRange;         
        interactable.Interacted += OnInteracted;
    }

    private void UnlinkInteractableEvents()
    {
        interactable.EnteredInteractorRange -= OnEnterInteractorRange; 
        interactable.ExitedInteractorRange -= OnExitInRange;         
        interactable.Interacted -= OnInteracted;
    }

    void OnEnterInteractorRange(Interactor interactor)
    {
        if (interactable.IsInteractable)
        {
            EnterInRangeState();
        }
    }

    void OnExitInRange(Interactor interactor)
    {
        if (interactable.IsInteractable)
        {
            ExitInRangeState();
        }
    }

    void OnInteracted(Interactor interactor)
    {
        ExitInRangeState();
    }
}

