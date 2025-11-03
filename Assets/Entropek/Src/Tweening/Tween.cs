using System;
using System.Collections;

namespace Entropek.Tweening
{    

    /// <summary>
    /// Utility class providing coroutine functionality for tweening values in scaled time.
    /// </summary>

    public class Tween{

        /// <summary>
        /// An IEnumerator (Unity Coroutine) that returns a normalised value from 0 to the specified time duration;
        /// calculated by the amount of elapsed time (Unity Scaled Time) since starting. 
        /// </summary>
        /// <param name="duration">The amount of time required to pass before completing.</param>
        /// <param name="stepCallback">The callback returning the normalised progress value.</param>
        /// <param name="completedCallback">An optional callback for when the tween has completed.</param>
        /// <returns></returns>

        public static IEnumerator IEnumerator(float duration, Action<float> stepCallback, Action completedCallback = null)
        {
            // increment elapsed timer per update.

            for (float elpasedTime = 0; elpasedTime < duration; elpasedTime += UnityEngine.Time.deltaTime)
            {
                // return the current progress (normalised value) from 0 to duration.

                stepCallback(elpasedTime / duration);

                // repeat until elpasedTime is equal to or greater than duration.

                yield return null;
            }

            // return 1, indicating that the tween has completed;
            // as this function returns normalised values (0-1)

            stepCallback(1f);

            // call the completed call back if there is one.

            completedCallback?.Invoke();
        }
    }
}
