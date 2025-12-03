using System.Diagnostics;
using System.Timers;
using TMPro;
using UnityEngine;

public class PlaythroughStopwatch : MonoBehaviour
{

    [Header("Components")]
    Stopwatch stopwatch;
    [SerializeField] TextMeshProUGUI primaryTimeText;
    [SerializeField] TextMeshProUGUI millisecondsTimeText;


    /// 
    /// Base.
    /// 


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
    }

    public void StopStopwatch()
    {
        stopwatch.Stop();
    }
}
