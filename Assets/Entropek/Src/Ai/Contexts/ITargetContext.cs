using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Ai.Contexts
{
    public interface ITargetContext
    {
        /// <summary>
        /// The transform component of the target an AiAgent is evaluating against.
        /// </summary>

        [RuntimeField] public Transform Target{get; set;}

        /// <summary>
        /// The vector distance from this AiAgent to the target.
        /// </summary>

        [RuntimeField] public Vector3 VectorDistanceToTarget {get; protected set;}

        /// <summary>
        /// The float distance from this AiAgent to the engaged opponent.
        /// </summary>

        [RuntimeField] public float DistanceToTarget {get; protected set;}

        /// <summary>
        /// The dot product that represents if the AiAgent is facing (1) or looking away (-1) from the engaged opponent.
        /// </summary>

        [RuntimeField] public float DotDirectionToTarget {get; protected set;}

        /// <summary>
        /// Gets the relative data for this frame between this AiAgent and the engaged opponent.
        /// </summary>
        /// <param name="transform">This gameobject transform</param>

        public void CalculateRelativeData(Transform transform)
        {
            VectorDistanceToTarget = Target.position - transform.position;
            DistanceToTarget = VectorDistanceToTarget.magnitude;
            DotDirectionToTarget = Vector3.Dot(VectorDistanceToTarget.normalized, transform.forward);
        }
    }    
}

