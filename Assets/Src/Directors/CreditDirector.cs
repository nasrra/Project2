using Entropek.UnityUtils.Attributes;
using UnityEngine;

public class CreditDirector : MonoBehaviour
{
    [RuntimeField] public float Credits;
    private const float BaseCreditIncrement = 0.0668f;
    [RuntimeField] private float coefficient = 1;
    [RuntimeField] private float coefficientIncrement = 0.334f;


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

    private void FixedUpdate()
    {
        Credits += CalculateCreditIncrement();
    }    


    ///
    /// Unique Functions.
    /// 


    /// <summary>
    /// Calculates the amount of credits to add to this directors credit score after a specified interval of time.
    /// </summary>
    /// <param name="timeInSeconds">The speccified time in seconds.</param>
    /// <returns>The amount.</returns>
    
    public float CalculateCreditIncrement(float timeInSeconds)
    {
        // times 60 for one second as increments are done in fixed update
        // which is roughly 60 times per second, as project settings is set to 0.167 for fixed update time.

        return CalculateCreditIncrement() * timeInSeconds * 60; 
    }

    /// <summary>
    /// Calculates the amount (per FixedUpdate) of credits to add to this directors credit score.
    /// </summary>
    /// <returns>The amount.</returns>

    public float CalculateCreditIncrement()
    {
        return BaseCreditIncrement * coefficient * Time.timeScale;
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
        coefficient += coefficientIncrement;
    }
}
