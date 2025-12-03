using System;
using System.Diagnostics;
using System.Timers;
using Entropek.Exceptions;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class PlaythroughStopwatch : MonoBehaviour
{

    public event Action Stopped;
    public event Action Started;

    public static PlaythroughStopwatch Singleton{get; private set;}

    [Header("Components")]
    Stopwatch stopwatch;
    [SerializeField] TextMeshProUGUI primaryTimeText;
    [SerializeField] TextMeshProUGUI millisecondsTimeText;


    /// 
    /// Base.
    /// 


    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        else if(Singleton != this)
        {
            throw new SingletonException("Only one Playthrough Stopwatch can exist at a time.");
        }
    }

    private void OnEnable()
    {
        if (stopwatch == null)
        {
            stopwatch = new();
        }
        stopwatch.Restart();
    }

    private void LateUpdate()
    {
        UpdateUiText();
    }

    private void OnDisable()
    {
        stopwatch.Stop();
    }

    private void OnDestroy()
    {
        if(Singleton == this)
        {
            Singleton = null;
        }
    }


    /// 
    /// Unique Functions.
    /// 


    private void UpdateUiText()
    {        
        primaryTimeText.text = $"{stopwatch.Elapsed.Minutes.ToString("D2")}:{stopwatch.Elapsed.Seconds.ToString("D2")}";
        millisecondsTimeText.text = $"{(stopwatch.Elapsed.Milliseconds/10).ToString("D2")}";
    }

    public void StartStopwatch()
    {
        stopwatch.Start();
        Started?.Invoke();
    }

    public void StopStopwatch()
    {
        stopwatch.Stop();
        Stopped?.Invoke();
    }
}
