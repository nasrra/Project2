using System;
using System.Diagnostics;
using Entropek.Exceptions;
using Entropek.UnityUtils.Attributes;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class PlaythroughStopwatch : MonoBehaviour
{

    public event Action Stopped;
    public event Action Started;
    public event Action<int> ElapsedMinute;
    public event Action<int> ElapsedSecond;

    public static PlaythroughStopwatch Singleton{get; private set;}

    [Header("Components")]
    Stopwatch stopwatch;
    [SerializeField] TextMeshProUGUI primaryTimeText;
    [SerializeField] TextMeshProUGUI millisecondsTimeText;

    [Header("Data")]
    [RuntimeField] private int elapsedMinutes; 
    [RuntimeField] private int elapsedSeconds;


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

        LinkEvents();
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
        UpdateElapsedTime();
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

        UnlinkEvents();
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

    private void UpdateElapsedTime()
    {
        int stopwatchElapsedMinutes = stopwatch.Elapsed.Minutes;
        if(stopwatchElapsedMinutes > elapsedMinutes)
        {
            elapsedMinutes = stopwatchElapsedMinutes;
            ElapsedMinute?.Invoke(elapsedMinutes);
        }

        int stopwatchElapsedSeconds = stopwatch.Elapsed.Seconds;
        if(stopwatchElapsedSeconds > elapsedSeconds)
        {
            elapsedSeconds = stopwatchElapsedSeconds;
            ElapsedSecond?.Invoke(elapsedSeconds);
        }
    }


    ///
    /// Event Linkage.
    /// 


    private void LinkEvents()
    {
        LinkGameManagerEvents();
    }

    private void UnlinkEvents()
    {
        UnlinkGameManagerEvents();
    }

    private void LinkGameManagerEvents()
    {
        GameManager.Singleton.GamePaused += OnGamePaused;
        GameManager.Singleton.GameResumed += OnGameResumed;
    }

    private void UnlinkGameManagerEvents()
    {
        GameManager.Singleton.GamePaused -= OnGamePaused;
        GameManager.Singleton.GameResumed -= OnGameResumed;        
    }

    private void OnGamePaused()
    {
        StopStopwatch();
    }

    private void OnGameResumed()
    {
        StartStopwatch();
    }

}
