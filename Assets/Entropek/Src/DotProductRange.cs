using UnityEngine;

namespace Entropek
{
    [System.Serializable]
    public struct DotProductRange
    {
        [SerializeField][Range(-1,1)] private float min;
        public float Min
        {
            get
            {
                return min;
            }
            set
            {
                min = Mathf.Clamp01(value);
            }
        }
        
        [SerializeField][Range(-1,1)] private float max;
        public float Max
        {
            get
            {
                return max;
            }
            set
            {
                max = Mathf.Clamp01(value);
            }
        }

        public DotProductRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// Gets the angle value of the min dot product value stored by this struct.
        /// </summary>
        /// <returns>The angle value</returns>

        public float GetMinAngle()
        {
            return GetAngle(min);
        }

        /// <summary>
        /// Gets the angle value of the max dot product value stored by this struct.
        /// </summary>
        /// <returns>The angle value</returns>

        public float GetMaxAngle()
        {
            return GetAngle(max);
        }

        /// <summary>
        /// Gets the angle value of a dot product value.
        /// </summary>
        /// <returns>The angle value</returns>

        public float GetAngle(float dot)
        {
            return Mathf.Acos(dot) * Mathf.Rad2Deg;            
        }


        /// <summary>
        /// Ensures this structs min is never greater than max; or vice versa.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>a new DotProductRange with the regulated min and max values</returns>

        public DotProductRange RegulateValues()
        {
            float min = Min > Max? Max : Min;
            float max = Max < Min? Min : Max;
            return new DotProductRange(min, max);
        }

        /// <summary>
        /// Ensures that min is never greater than max; or vice versa.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>a new DotProductRange with the regulated min and max values</returns>

        public static DotProductRange RegulateValues(float min, float max)
        {
            float newMin = min > max? max : min;
            float newMax = max < min? min : max;
            return new DotProductRange(newMin, newMax);
        }
    }
}
