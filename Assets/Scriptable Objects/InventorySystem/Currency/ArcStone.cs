using UnityEngine;

public class ArcStone : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] CurrencyPickup currencyPickup;
    [SerializeField] Entropek.Interaction.Interactable interactable;
    [SerializeField] Entropek.Audio.AudioPlayer audioPlayer;
    [SerializeField] GameObject graphicsObject;


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
        audioPlayer.PlaySound("ArcStonePickup", transform.position);
        graphicsObject.SetActive(false);
        Destroy(gameObject, 1.5f);
    }
}
