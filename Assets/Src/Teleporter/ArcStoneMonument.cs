using Entropek.Interaction;
using UnityEngine;

public class ArcStoneMonument : MonoBehaviour
{

    [Header("Components")]
    [Tooltip("Note: The charge field gameObject should always start disabled.")]
    [SerializeField] private ChargeField chargeField;
    [SerializeField] private Entropek.Interaction.Interactable interactable;
    [SerializeField] private Entropek.Audio.AudioPlayer audioPlayer;
    [SerializeField] private Animator chargeFieldGraphicsAnimator;


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
        LinkChargeFieldEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkInteractableEvents();
        UnlinkChargeFieldEvents();
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
        interactable.DisableInteraction();
        chargeField.enabled = true;
        chargeFieldGraphicsAnimator.Play("Expand");
        audioPlayer.PlaySound("TeleporterExpanded", transform.position);
    }

    private void LinkChargeFieldEvents()
    {
        chargeField.OnCharged += OnChargeFieldCharged;
    }

    private void UnlinkChargeFieldEvents()
    {
        chargeField.OnCharged -= OnChargeFieldCharged;
    }

    private void OnChargeFieldCharged()
    {
        chargeField.enabled = false;
        chargeFieldGraphicsAnimator.Play("Shrink");
        audioPlayer.PlaySound("TeleporterCharged", transform.position);
    }

}