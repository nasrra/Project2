using UnityEngine;

namespace Entropek.Ai
{
    [System.Serializable]
    public abstract class AiState : AiOutcome
    {
    
        [Header("Score Probability Modifier")]

        [SerializeField] AnimationCurve stateSwitchChanceOverLifetime;
        public AnimationCurve StateSwitchChanceOverLifetime => stateSwitchChanceOverLifetime;

#if UNITY_EDITOR
        public virtual void OnValidate()
        {
            UnityUtils.AnimationCurve.Clamp01KeyValues(stateSwitchChanceOverLifetime);
        }
#endif

    }    
}

