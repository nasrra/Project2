using System;
using System.Collections;
using Entropek.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Entropek.Time.OneShotTimer))]
public class ChargeField : MonoBehaviour
{
    public event Action Standby;
    public event Action Charging;
    public event Action Depleted;
    public event Action Charged;

    [Header("Components")]
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private Entropek.Time.OneShotTimer timer;

    [Header("Data")]
    
    [Entropek.UnityUtils.Attributes.RuntimeField] private SwapbackList<GameObject> chargerObjectsInRange = new();
    
    [SerializeField] private LayerMask chargerObjectLayer;
    
    [Entropek.UnityUtils.Attributes.RuntimeField] private ChargeFieldState state = ChargeFieldState.Depleted;
    public ChargeFieldState State => state;
    
    [Entropek.UnityUtils.Attributes.RuntimeField] private float progress;
    public float Progress => progress;

    private Coroutine PollChargerObjectsCoroutine;
    private Coroutine PollTimerProgressCoroutine;

    /// 
    /// Base.
    /// 

    void Awake()
    {
        LinkEvents();
    }

    void OnDestroy()
    {
        UnlinkEvents();
        StopAllCoroutines();
    }
    
    public void OnTriggerEnter(Collider other)
    {        
        GameObject otherGameObject = other.gameObject;

        if(IsGameObjectAChargerObject(otherGameObject) == true)
        {
            ChargerObjectEnteredArea(otherGameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject otherGameObject = other.gameObject;

        if(IsGameObjectAChargerObject(otherGameObject) == true)
        {
            ChargerObjectExitedArea(otherGameObject);
        }
    }


    ///
    /// State Machine.
    /// 


    private void DepletedState()
    {
        state = ChargeFieldState.Depleted;

        StopAllCoroutines();

        Depleted?.Invoke();
    }

    private void ChargingState()
    {
        
        switch (state)
        {
            case ChargeFieldState.Depleted:
                timer.Begin();
            break;
            case ChargeFieldState.Standy:
                timer.Resume();
            break;
        }
        
        state = ChargeFieldState.Charging;
                
        Entropek.UnityUtils.Coroutine.Replace(this, ref PollChargerObjectsCoroutine, PollChargerObjects());
        Entropek.UnityUtils.Coroutine.Replace(this, ref PollTimerProgressCoroutine, PollTimerProgress());

        Charging?.Invoke();
    }

    private void StandbyState()
    {
        state = ChargeFieldState.Standy;
        timer.Pause();

        StopAllCoroutines();

        Standby?.Invoke();
    }

    private void ChargedState()
    {
        state = ChargeFieldState.Charged;
        progress = 100;
        StopAllCoroutines();

        Charged?.Invoke();
    }


    ///
    /// Utility Functions.
    /// 


    /// <summary>
    /// Checks whether or not a gameobject is considered a charger object.
    /// </summary>
    /// <param name="other">The specified gameobject to check.</param>
    /// <returns>true, if it is a charger object. false, if it is not.</returns>

    private bool IsGameObjectAChargerObject(GameObject other){
        int otherLayer = 1 << other.layer; // bitwise to get actual layer.        
        return (otherLayer & chargerObjectLayer) != 0;
    }

    /// <summary>
    /// Adds a charger object to the tracked gameObject list, starting the charge timer if there were no charger objects on the previous call.
    /// </summary>
    /// <param name="chargerObject">The specified gameobject to track.</param>

    private void ChargerObjectEnteredArea(GameObject chargerObject)
    {        
        // if there are currently no charger objects in the area.

        if(state != ChargeFieldState.Charged
        && chargerObjectsInRange.Count == 0){
            ChargingState(); 
        }

        chargerObjectsInRange.Add(chargerObject);
    }

    /// <summary>
    /// Removes a charger object from the tracked objects list.
    /// </summary>
    /// <param name="chargerObject">The specified gameobject to track.</param>

    private void ChargerObjectExitedArea(GameObject chargerObject)
    {
        chargerObjectsInRange.Remove(chargerObject);
        CheckChargerObjectsInRange();
    }

    /// <summary>
    /// Halts the charge timer if there are no more charger objects within range.
    /// </summary>

    private void CheckChargerObjectsInRange(){

        // if there are no chargers in the field and this is not fully charged.

        if(state != ChargeFieldState.Charged
        && chargerObjectsInRange.Count == 0)
        {
            StandbyState();
        }
    }


    /// <summary>
    /// Polls all tracked charger objects, removing any that may now be null due to gameObject deallocation.
    /// </summary>
    /// <returns>Indefinently.</returns>

    private IEnumerator PollChargerObjects()
    {
        while (true)
        {
            // poll charger objects every second.
            // Note:
            //  Wait the second first so that when an object enters range,
            //  the charge field doesnt immediately finish.

            yield return new WaitForSeconds(1);
            
            // only poll if this has started charging.

            if(state != ChargeFieldState.Depleted && state != ChargeFieldState.Charged)
            {
                // remove any charger objects that may be null.
                chargerObjectsInRange.RemoveAll(o => o == null);
                CheckChargerObjectsInRange();                
            }


        }
    }

    private IEnumerator PollTimerProgress()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            progress = (1-timer.NormalisedCurrentTime) * 100;
        }
    }


    ///
    /// Linkage.
    /// 


    private void LinkEvents()
    {
        LinkTimerEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkTimerEvents();
    }

    private void LinkTimerEvents()
    {
        timer.Timeout += ChargedState;
    }

    private void UnlinkTimerEvents()
    {
        timer.Timeout -= ChargedState;        
    }
}
