using Entropek.UnityUtils.Attributes;
using UnityEngine;

public class CreditDirector : MonoBehaviour
{
    [RuntimeField] public float Credits;
    private const float BaseCreditIncrement = 0.0668f;
    [RuntimeField] private float coefficient = 1;
    [RuntimeField] private float coefficientIncrement = 0.0668f;

    private void Awake()
    {
        LinkEvents();
    }

    private void OnDestroy()
    {
        UnlinkEvents();
    }

    private void FixedUpdate()
    {
        Credits += BaseCreditIncrement * coefficient * Time.timeScale;
    }    


    /// 
    /// Event Linkage.
    /// 


    private void LinkEvents()
    {
        LinkPlaythoughStopwatchEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkPlaythoughStopwatchEvents();
    }


    /// 
    /// Playthrough Stopwatch Events
    /// 


    private void LinkPlaythoughStopwatchEvents()
    {
        PlaythroughStopwatch.Singleton.ElapsedMinute += OnElapsedMinute;
    }

    private void UnlinkPlaythoughStopwatchEvents()
    {
        if (PlaythroughStopwatch.Singleton != null)
        {
            PlaythroughStopwatch.Singleton.ElapsedMinute -= OnElapsedMinute;        
        }
    }

    private void OnElapsedMinute(int elapsedMinutes)
    {
        coefficientIncrement += coefficientIncrement;
    }
}
