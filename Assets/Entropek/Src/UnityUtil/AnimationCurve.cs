using System;
using UnityEngine;

namespace Entropek.UnityUtils
{
    public class AnimationCurve : MonoBehaviour
    {
        /// <summary>
        /// Clamps all values on an animation curve to be betwen 0 and 1.
        /// </summary>
        /// <param name="animationCurve">The animation curve to clamp</param>
        /// <exception cref="NullReferenceException"></exception>

        public static void Clamp01KeyValues(UnityEngine.AnimationCurve animationCurve)
        {
            #if UNITY_EDITOR
            if(animationCurve == null)
            {
                throw new NullReferenceException($"Cannot clamp an Animation Curve that is null.");
            }
            #endif

            // get a copy of the key frames.

            // Note that the array is "by value", i.e. getting keys returns a copy of all keys and setting keys copies them into the curve.
            // https://docs.unity3d.com/6000.2/Documentation/ScriptReference/AnimationCurve-keys.html

            Keyframe[] keys = animationCurve.keys;

            for(int i = 0; i < keys.Length; i++)
            {
                keys[i].value = Mathf.Clamp01(keys[i].value);
            }

            // Reassign.

            animationCurve.keys = keys;
        }

        public static void MultiplyKeyValues(UnityEngine.AnimationCurve animationCurve, float factor)
        {

            #if UNITY_EDITOR
            if(animationCurve == null)
            {
                throw new NullReferenceException($"Cannot clamp an Animation Curve that is null.");
            }
            #endif

            // get a copy of the key frames.

            // Note that the array is "by value", i.e. getting keys returns a copy of all keys and setting keys copies them into the curve.
            // https://docs.unity3d.com/6000.2/Documentation/ScriptReference/AnimationCurve-keys.html

            Keyframe[] keys = animationCurve.keys;

            for(int i = 0; i < keys.Length; i++)
            {
                keys[i].value *= factor;
            }

            // reassign.
            animationCurve.keys = keys;
        }
    }    
}


