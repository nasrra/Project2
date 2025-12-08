using Entropek.Interaction;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

public class ArcStoneMonument : MonoBehaviour
{

    private const int CreditIncrementTime = 30;

    [Header("Components")]
    [Tooltip("Note: The charge field gameObject should always start disabled.")]
    [SerializeField] private Entropek.Interaction.Interactable interactable;
    [SerializeField] private ArcField arcField;
    [Tooltip("The ArcStone should be disabled in the editor.")]
    [SerializeField] private ArcStone arcStonePrefab;
    [SerializeField] private Transform arcStoneSpawnPoint;
    [RuntimeField] private ArcStone arcStone; 


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
        // EnemyDirector.Singleton.SpawnMiniboss();
        interactable.DisableInteraction();
        arcField.Activate();
        float credits = EnemyDirector.Singleton.CreditDirector.CalculateCreditIncrement(CreditIncrementTime); 
        EnemyDirector.Singleton.ExecuteSpawnBehaviour(transform.position, credits, UnityEngine.Random.Range(0,4), out _);   
    }

    public void ChargedState()
    {
        EnemyDirector.Singleton.SlowState();
        arcStone = Instantiate(arcStonePrefab, arcStoneSpawnPoint.position, arcStoneSpawnPoint.rotation);
        LinkArcStoneEvents();
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents()
    {
        LinkInteractableEvents();
        LinkArcFieldEvents();
        LinkArcStoneEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkInteractableEvents();
        UnlinkArcFieldEvents();
        UnlinkArcStoneEvents();
    }


    /// 
    /// Interactable Events.
    /// 


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


    /// 
    /// Arcfield Events.
    /// 


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


    ///
    /// Arcstone Events.
    /// 

    
    private void LinkArcStoneEvents()
    {
        if (arcStone != null)
        {
            arcStone.Interactable.Interacted += OnArcStoneInteracted;
        }
    }

    private void UnlinkArcStoneEvents()
    {
        if (arcStone != null)
        {
            arcStone.Interactable.Interacted -= OnArcStoneInteracted;
        }
    }

    private void OnArcStoneInteracted(Interactor interactor)
    {
        // Debug.Log(2);
        Destroy(gameObject);
    }

}