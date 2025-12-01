using Entropek.Interaction;
using Unity.VisualScripting;
using UnityEngine;

public class InteractableGraphics : MonoBehaviour
{
    [SerializeField] Interactable interactable;
    [SerializeField] MeshRenderer[] meshRenderers;
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
        for(int i = 0; i < meshRenderers.Length; i++)
        {
            MeshRenderer meshRenderer = meshRenderers[i];
            meshRenderer.materials[0].SetFloat("_OverlayAmount", 0.24f);
        }

        for(int i = 0; i < outline.renderers.Length; i++)
        {
            outline.renderers[i].materials[2].SetFloat("_OutlineWidth", 10);        
        }
    }

    private void ExitInRangeState()
    {
        for(int i = 0; i < meshRenderers.Length; i++)
        {
            MeshRenderer meshRenderer = meshRenderers[i];
            meshRenderer.materials[0].SetFloat("_OverlayAmount", 0f);
        }

        for(int i = 0; i < outline.renderers.Length; i++)
        {
            outline.renderers[i].materials[2].SetFloat("_OutlineWidth", 0f);        
        }
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

