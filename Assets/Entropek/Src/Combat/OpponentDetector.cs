using System;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Combat
{
    public class OpponentDetector : MonoBehaviour
    {

        public event Action<Transform> EngagedOpponent;
        public event Action<Transform> DisengagedOpponent;


        ///
        /// Data.
        /// 


        [SerializeField] private LayerMask oppponentLayer;


        /// 
        /// Runtime Data.
        /// 

        [RuntimeField] private Transform opponentTransform;
        [RuntimeField] private bool isEngaged = false;


        /// <summary>
        /// Used to validate if the engaged opponent hasnt been destroyed in a given frame.
        /// </summary>
        /// <returns>true, if the objct is still allocated in memory.</returns>

        protected bool ValidateEngagedOpponent()
        {
            return opponentTransform != null;
        }

        /// <summary>
        /// Clears the currently engaged opponent and halts the evaluation loop.
        /// </summary>

        public virtual void DisengageOpponent()
        {
            isEngaged = false;
            Transform disengagedOpponent = opponentTransform;
            opponentTransform = null;
            DisengagedOpponent?.Invoke(disengagedOpponent);
        }

        /// <summary>
        /// Sets the target of this agent to the transform of an opponent and starts the evaluation loop.
        /// </summary>
        /// <param name="opponentTransform">The specified transform.</param>

        public virtual void EngageOpponent(Transform opponentTransform)
        {
            isEngaged = true;
            this.opponentTransform = opponentTransform;
            EngagedOpponent?.Invoke(opponentTransform);
        }  

        private void OnTriggerEnter(Collider other)
        {

            // Get the other's bitwise layer mask.
            // Get our bitwise layer mask for opponents. 

            int otherLayerValue = 1 << other.gameObject.layer;
            int opponentLayerValue = oppponentLayer.value;

            // evaluate if the other is an opponent.

            if ((otherLayerValue & opponentLayerValue) != 0)
            {

                Transform otherTransform = other.transform;

                // if (ValidateEngagedOpponent() == true)
                // {
                    EngageOpponent(otherTransform);
                // }
            }
        }
    }
}

