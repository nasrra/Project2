using UnityEngine;
using UnityEngine.VFX;

namespace Entropek.Vfx
{

    /// <summary>
    /// A Vfx player that plays a single vfx instance.
    /// </summary>

    public class SingleVfxPlayer : VfxPlayer
    {
        [Header("Data")]
        [SerializeField] private VisualEffect vfx;
        public VisualEffect Vfx => vfx;

        protected override bool IsFinished()
        {
            return vfx.aliveParticleCount == 0 && !vfx.HasAnySystemAwake();
        }

        public override void Play()
        {
            base.Play();
            vfx.Play();
        }

        protected override void StopInternal()
        {
            vfx.Stop();
        }
    }
    
}

