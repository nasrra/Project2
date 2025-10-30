using System.Collections;
using Entropek.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Entropek.Time.OneShotTimer))]
public class ChargeField : MonoBehaviour
{
    private enum State : byte
    {
        Depleted,   // default as depleted.
        Charging,   // when a charger object has entered the field.
        Paused,     // when a charger has no chargers inside the field but has been charged. 
        Charged,    // the field is fully charged.
    }

    [Header("Components")]
    [SerializeField] private new Collider collider;
    [SerializeField] private Entropek.Time.OneShotTimer timer;

    [Header("Data")]
    [SerializeField] private LayerMask chargerObjectLayer;
    [Entropek.UnityUtils.Attributes.RuntimeField] private SwapbackList<GameObject> chargerObjectsInRange = new();
    [Entropek.UnityUtils.Attributes.RuntimeField] private State state = State.Depleted;
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
        state = State.Depleted;

        StopAllCoroutines();
    }

    private void ChargingState()
    {
        
        switch (state)
        {
            case State.Depleted:
                timer.Begin();
            break;
            case State.Paused:
                timer.Resume();
            break;
        }
        
        state = State.Charging;
                
        Entropek.UnityUtils.Coroutine.Replace(this, ref PollChargerObjectsCoroutine, PollChargerObjects());
        Entropek.UnityUtils.Coroutine.Replace(this, ref PollTimerProgressCoroutine, PollTimerProgress());
    }

    private void PausedState()
    {
        state = State.Paused;
        timer.Pause();

        StopAllCoroutines();
    }

    private void ChargedState()
    {
        state = State.Charged;
        progress = 100;

        StopAllCoroutines();
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

        if(state != State.Charged
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

        if(state != State.Charged
        && chargerObjectsInRange.Count == 0)
        {
            PausedState();
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

            if(state != State.Depleted && state != State.Charged)
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
