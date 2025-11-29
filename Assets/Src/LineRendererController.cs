using System;
using UnityEngine;

public class LineRendererController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private LineRenderer lineRenderer;
    public LineRenderer LineRenderer => lineRenderer;

    Coroutine lerpColorCoroutine;

    /// <summary>
    /// Begins a tween from the current color gradient of the line renderer to a new one.
    /// </summary>
    /// <param name="startColor">The color gradients start color to tween to.</param>
    /// <param name="endColor">The color gradients end color to tween to.</param>
    /// <param name="duration">The duration of the tween.</param>
    /// <param name="completedCallback">An optional callback for when the tween completes.</param>

    public void LerpColor(Color startColor, Color endColor, float duration, Action completedCallback = null)
    {
        
        Color initialStartColor = lineRenderer.startColor;
        Color initialEndColor = lineRenderer.endColor;
        
        Entropek.UnityUtils.Coroutine.Replace(
            this,
            ref lerpColorCoroutine,
            Entropek.Tweening.Tween.IEnumerator(
                duration,
                step =>
                {
                    lineRenderer.startColor = Color.Lerp(initialStartColor, startColor, step);
                    lineRenderer.endColor = Color.Lerp(initialEndColor, endColor, step);    
                },
                completedCallback
            )
        );
    }

    /// <summary>
    /// Begins a tween from the current color gradient alpha of the line renderer to a new one.
    /// </summary>
    /// <param name="startAlpha">The color gradients start alpha tween to.</param>
    /// <param name="endAlpha">The color gradients end alpha tween to.</param>
    /// <param name="duration">The duration of the tween.</param>
    /// <param name="completedCallback">An optional callback for when the tween completes.</param>

    public void LerpColorAlpha(float startAlpha, float endAlpha, float duration, Action completedCallback = null)
    {
        
        float initialStartAlpha = lineRenderer.startColor.a;
        float initialEndAlpha = lineRenderer.endColor.a;

        Entropek.UnityUtils.Coroutine.Replace(
            this,
            ref lerpColorCoroutine,
            Entropek.Tweening.Tween.IEnumerator(
                duration,
                step =>
                {
                    float newStartAlpha = Mathf.Lerp(initialStartAlpha, startAlpha, step);
                    Color currentStartColor = lineRenderer.startColor;
                    lineRenderer.startColor = new Color(currentStartColor.r, currentStartColor.g, currentStartColor.b, newStartAlpha);

                    float newEndAlpha = Mathf.Lerp(initialEndAlpha, endAlpha, step);
                    Color currentEndColor = lineRenderer.endColor;
                    lineRenderer.endColor = new Color(currentEndColor.r, currentEndColor.g, currentEndColor.b, newEndAlpha);
                },
                completedCallback
            )
        );
    }
}
