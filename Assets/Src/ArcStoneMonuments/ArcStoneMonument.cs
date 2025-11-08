using UnityEngine;

public class ArcStoneMonument : MonoBehaviour
{

    [Header("Components")]
    [Tooltip("Note: The charge field gameObject should always start disabled.")]
    [SerializeField] private Entropek.Interaction.Interactable interactable;
    [SerializeField] private ArcField arcField;
    [Tooltip("The ArcStone should be disabled in the editor.")]
    [SerializeField] private GameObject arcStone;


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


    public void ActivatedState()
    {
        EnemyDirector.Singleton.FastState();
        EnemyDirector.Singleton.SpawnMiniboss();
        interactable.DisableInteraction();
        arcField.Activate();        
    }

    public void ChargedState()
    {
        EnemyDirector.Singleton.SlowState();
        arcStone.SetActive(true);
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents()
    {
        LinkInteractableEvents();
        LinkArcFieldEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkInteractableEvents();
        UnlinkArcFieldEvents();
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
        ActivatedState();
    }

    private void LinkArcFieldEvents()
    {
        arcField.ChargeField.Charged += OnArcFieldCharged;
    }

    private void UnlinkArcFieldEvents()
    {
        arcField.ChargeField.Charged -= OnArcFieldCharged;
    }

    private void OnArcFieldCharged()
    {
        ChargedState();
    }

}