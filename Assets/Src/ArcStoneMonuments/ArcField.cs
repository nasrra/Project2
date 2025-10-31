using UnityEngine;

public class ArcField : MonoBehaviour
{
    [Tooltip("Note: The charge field gameObject should always start disabled.")]
    [SerializeField] private ChargeField chargeField;
    public ChargeField ChargeField => chargeField;
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
    /// Functions.
    /// 


    public void Activate()
    {
        chargeField.gameObject.SetActive(true);
        chargeFieldGraphicsAnimator.Play("Expand");
        audioPlayer.PlaySound("TeleporterExpanded", transform.position);
   
    }

    public void Deactivate()
    {
        chargeField.gameObject.SetActive(false);
        chargeFieldGraphicsAnimator.Play("Shrink");
        audioPlayer.PlaySound("TeleporterCharged", transform.position);    
    }


    /// 
    /// Linkage.
    /// 


    private void LinkEvents()
    {
        LinkChargeFieldEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkChargeFieldEvents();
    }

    private void LinkChargeFieldEvents()
    {
        chargeField.Charged += OnCharged; 
    }

    private void UnlinkChargeFieldEvents()
    {
        chargeField.Charged -= OnCharged;         
    }

    private void OnCharged()
    {
        Deactivate();
    }
}
