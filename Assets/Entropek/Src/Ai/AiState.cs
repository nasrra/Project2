using UnityEngine;

namespace Entropek.Ai
{
    public abstract class AiState
    {
        [Header("State Name")]
        
        [SerializeField] protected string name;
        public string Name => name;

        [Header("Score Probability Modifier")]

        [SerializeField] AnimationCurve stateSwitchChanceOverLifetime;
        public AnimationCurve StateSwitchChanceOverLifetime => stateSwitchChanceOverLifetime;

        [Header("Parameters")]
        
        public bool Enabled;

        public float GetMaxScore() => 1;

#if UNITY_EDITOR
        public virtual void OnValidate()
        {
            UnityUtils.AnimationCurve.Clamp01AnimationCurveKeyValues(stateSwitchChanceOverLifetime);
        }
#endif

    }    
}

