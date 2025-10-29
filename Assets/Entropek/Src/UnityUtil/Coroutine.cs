using System.Collections;
using UnityEngine;

namespace Entropek.UnityUtils
{
    public static class Coroutine{
        
        /// <summary>
        /// Replaces a coroutines IEnumerator with a specified IEnumerator.
        /// </summary>
        /// <param name="monoBehaviour">The monobehavoiur that is running the coroutine.</param>
        /// <param name="coroutine">The specified coroutine to replace.</param>
        /// <param name="iEnumerator">The IEnumerator to replace the currrent coroutines stored IEnumerator with.</param>

        public static void Replace(MonoBehaviour monoBehaviour, ref UnityEngine.Coroutine coroutine, IEnumerator iEnumerator)
        {
            if (coroutine != null)
            {
                monoBehaviour.StopCoroutine(coroutine);
            }
            coroutine = iEnumerator != null ? monoBehaviour.StartCoroutine(iEnumerator) : null;
        }

    }

}

