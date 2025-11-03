using UnityEngine;
using UnityEngine.VFX;


namespace Entropek.Vfx
{

    /// <summary>
    /// A Vfx Player that can play multiple vfx instances at once.
    /// </summary>

    public class CompositeVfxPlayer : VfxPlayer
    {

        [Header("Data")]
        [SerializeField] private VisualEffect[] vfx;

        protected override bool IsFinished()
        {
            for (int i = 0; i < vfx.Length; i++)
            {
                VisualEffect current = vfx[i];

                if (current.aliveParticleCount > 0 || current.HasAnySystemAwake())
                {
                    return false;
                }
            }

            return true;
        }

        public override void Play()
        {
            base.Play();
            for (int i = 0; i < vfx.Length; i++)
            {
                vfx[i].Play();
            }
        }
    }
}


