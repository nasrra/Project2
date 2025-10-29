using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Entropek.Camera
{
    public class CameraPostProcessingController : MonoBehaviour
    {
        [SerializeField] Volume globalVolume;

        Vignette vignette;
        LensDistortion lensDistortion;
        MotionBlur motionBlur;

        Coroutine vignetteIntensityCoroutine;
        Coroutine lensDistortionIntensityCoroutine;
        Coroutine motionBlurIntensityCoroutine;


        private void Awake()
        {
            GetVolumeComponents();
        }

        /// <summary>
        /// Gets the required to components from the global volume to perform modifications on. 
        /// </summary>
        /// <exception cref="InvalidOperationException">If an required component is not implemented.</exception>

        private void GetVolumeComponents()
        {
            // Get Vignette.

            if (globalVolume.sharedProfile.TryGet(out Vignette vignette))
            {
                this.vignette = vignette;
            }
            else
            {
                throw new InvalidOperationException("Camera Effects Volume does not contain override: Vignette.");
            }

            // Get Lens Distortion.

            if (globalVolume.sharedProfile.TryGet(out LensDistortion lensDistortion))
            {
                this.lensDistortion = lensDistortion;
            }
            else
            {
                throw new InvalidOperationException("Camera Effects Volume does not contain override: LensDistortion.");
            }

            // Get Motion Blur.

            if (globalVolume.sharedProfile.TryGet(out MotionBlur motionBlur))
            {
                this.motionBlur = motionBlur;
            }
            else
            {
                throw new InvalidOperationException("Camera Effects Volume does not contain override: Motion Blur.");
            }
        }

        /// <summary>
        /// Begins a tween from the current intensity value to the specified intensity value.
        /// </summary>
        /// <param name="duration">The amount of time in seconds (scaled time) for the tween to complete.</param>
        /// <param name="intensity">The specified intensity to tween to.</param>
        /// <param name="completedCallback">An optional callback for when the tween completes.</param>

        public void LerpVignetteIntensity(float duration, float intensity, Action completedCallback = null)
        {
            LerpClampedFloatParameter(vignetteIntensityCoroutine, vignette.intensity, duration, intensity, completedCallback);
        }

        /// <summary>
        /// Begins a two way tween that lerps from the current intensity value to the specified intensity and back to zero.
        /// </summary>
        /// <param name="duration">The amount of time in seconds (scaled time) for the tween to complete.</param>
        /// <param name="intensity">The specified intensity to tween to.</param>
        /// <param name="completedCallback">An optional callback for when the tween completes.</param>

        public void PulseVignetteIntensity(float duration, float intensity, Action completedCallback = null)
        {
            PulseClampedFloatParameter(vignetteIntensityCoroutine, vignette.intensity, duration, intensity, completedCallback);
        }

        /// <summary>
        /// Begins a two-way tween that lerps to two specified intensities.
        /// </summary>
        /// <param name="pulseInDuration">The amount of time in seconds (scaled time) for the lerp in.</param>
        /// <param name="pulseOutDuration">The amount of time in seconds (scaled time) for the lerp out.</param>
        /// <param name="pulseInIntensity">The specified intensity to lerp-in to.</param>
        /// <param name="pulseOutIntensity">The specified intensity to lerp-out to.</param>
        /// <param name="completedCallback">An optional callback for when the tween completes.</param>

        public void PulseVignetteIntensity(float pulseInDuration, float pulseOutDuration, float pulseInIntensity, float pulseOutIntensity, Action completedCallback = null)
        {
            PulseClampedFloatParameter(vignetteIntensityCoroutine, vignette.intensity, pulseInDuration, pulseOutDuration, pulseInIntensity, pulseOutIntensity, completedCallback);
        }

        /// <summary>
        /// Begins a tween from the current intensity value to the specified intensity value.
        /// </summary>
        /// <param name="duration">The amount of time in seconds (scaled time) for the tween to complete.</param>
        /// <param name="intensity">The specified intensity to lerp-in to.</param>
        /// <param name="completedCallback">An optional callback for when the tween completes.</param>

        public void LerpLensDistortionIntensity(float duration, float intensity, Action completedCallback = null)
        {
            LerpClampedFloatParameter(lensDistortionIntensityCoroutine, lensDistortion.intensity, duration, intensity, completedCallback);
        }

        /// <summary>
        /// Begins a two-way tween from the current intensity value to the specified intensity then back to zero.
        /// </summary>
        /// <param name="duration">The amount of time in seconds (scaled time) for the tween to complete.</param>
        /// <param name="intensity">The specified intensity to lerp to.</param>
        /// <param name="completedCallback">An optional callback for when the tween completes.</param>

        public void PulseLensDistortionIntensity(float duration, float intensity, Action completedCallback = null)
        {
            PulseClampedFloatParameter(lensDistortionIntensityCoroutine, lensDistortion.intensity, duration, intensity, completedCallback);
        }

        /// <summary>
        /// Begins a tween from the current intensity value to the specified intensity value.
        /// </summary>
        /// <param name="duration">The amount of time in seconds (scaled time) for the tween to complete.</param>
        /// <param name="intensity">The specified intensity to lerp-in to.</param>
        /// <param name="completedCallback">An optional callback for when the tween completes.</param>

        public void LerpMotionBlurIntensity(float duration, float intensity, Action completedCallback = null)
        {
            LerpClampedFloatParameter(motionBlurIntensityCoroutine, motionBlur.intensity, duration, intensity, completedCallback);
        }

        /// <summary>
        /// Begins a two-way tween from the current intensity value to the specified intensity then back to zero.
        /// </summary>
        /// <param name="duration">The amount of time in seconds (scaled time) for the tween to complete.</param>
        /// <param name="intensity">The specified intensity to lerp to.</param>
        /// <param name="completedCallback">An optional callback for when the tween completes.</param>

        public void PulseMotionBlurIntensity(float duration, float intensity, Action completedCallback = null)
        {
            PulseClampedFloatParameter(motionBlurIntensityCoroutine, motionBlur.intensity, duration, intensity, completedCallback);
        }

        /// <summary>
        /// Begins a two-way tween from the current ClampedFloatParameter value to the specified value then back to zero.
        /// </summary>
        /// <param name="lerpCoroutine">The coroutine to store the tween IEnumerator.</param>
        /// <param name="clampedFloatParameter">The ClampedFloatParameter to tween.</param>
        /// <param name="duration">The amount of time in seconds (scaled time) for the tween to complete.</param>
        /// <param name="endValue">The value to lerp to.</param>
        /// <param name="completedCallback">An optional callback for when the tween completes.</param>

        private void PulseClampedFloatParameter(
            Coroutine lerpCoroutine,
            ClampedFloatParameter clampedFloatParameter,
            float duration, float endValue,
            Action completedCallback
        )
        {
            float halfDuration = duration * 0.5f;

            LerpClampedFloatParameter(
                lerpCoroutine,
                clampedFloatParameter,
                halfDuration,
                endValue,
                () =>
                {
                    // lerp back to zero on first complete.
                    // then call the completedCallback once finished pulsing.

                    LerpClampedFloatParameter(lerpCoroutine, clampedFloatParameter, halfDuration, 0, completedCallback);
                }
            );
        }

        /// <summary>
        /// Begins a two-way tween from the current ClampedFloatParameter value to two specified values.
        /// </summary>
        /// <param name="lerpCoroutine">The coroutine to store the tween IEnumerator.</param>
        /// <param name="clampedFloatParameter">The ClampedFloatParameter to tween.</param>
        /// <param name="pulseInDuration">The amount of time in seconds (scaled time) for the lerp-in.</param>
        /// <param name="pulseOutDuration">The amount of time in seconds (scaled time) for the lerp-out.</param>
        /// <param name="pulseInEndValue">The specified value to lerp-in to.</param>
        /// <param name="pulseOutEndValue">The specified value to lerp-out to.</param>
        /// <param name="completedCallback">An optional callback for when the tween completes.</param>

        private void PulseClampedFloatParameter(
            Coroutine lerpCoroutine,
            ClampedFloatParameter clampedFloatParameter,
            float pulseInDuration,
            float pulseOutDuration,
            float pulseInEndValue,
            float pulseOutEndValue,
            Action completedCallback
        )
        {
            LerpClampedFloatParameter(
                lerpCoroutine,
                clampedFloatParameter,
                pulseInDuration,
                pulseInEndValue,
                () =>
                {
                    // lerp back to the specified end value when pulsing out.
                    // then call the completedCallback once finished pulsing.

                    LerpClampedFloatParameter(lerpCoroutine, clampedFloatParameter, pulseOutDuration, pulseOutEndValue, completedCallback);
                }
            );
        }
        
        /// <summary>
        /// Begins a tween from the current ClampedFloatParamter value to a specified value.
        /// </summary>
        /// <param name="lerpCoroutine">The coroutine to store the tween IEnumerator.</param>
        /// <param name="clampedFloatParameter">The ClampedFloatParamter to tween.</param>
        /// <param name="duration">The amount of time in seconds (scaled time) for the tween to complete.</param>
        /// <param name="endValue">The value to tween to.</param>
        /// <param name="completedCallback">An optional callback for when the tween completes.</param>

        private void LerpClampedFloatParameter(Coroutine lerpCoroutine, ClampedFloatParameter clampedFloatParameter, float duration, float endValue, Action completedCallback)
        {
            float initialValue = clampedFloatParameter.value;
            UnityUtils.Coroutine.Replace(
                this,
                ref lerpCoroutine,
                Tweening.Tween.IEnumerator(
                    duration,
                    step => clampedFloatParameter.value = Mathf.Lerp(initialValue, endValue, step),
                    completedCallback
                )
            );
        }
    }
}

