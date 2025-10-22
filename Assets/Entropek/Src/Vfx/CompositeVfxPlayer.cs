using UnityEngine;
using UnityEngine.VFX;


namespace Entropek.Vfx
{

    public class CompositeVfxPlayer : VfxPlayer
    {

        [SerializeField] private VisualEffect[] vfx;

        public override bool IsFinished()
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
            for (int i = 0; i < vfx.Length; i++)
            {
                vfx[i].Play();
            }
        }

        public override void Play(Vector3 worldSpacePosition)
        {
            transform.position = worldSpacePosition;
            Play();
        }
        
        public override void Play(Vector3 worldSpacePosition, Vector3 rotationEuler)
        {
            transform.rotation = Quaternion.Euler(rotationEuler);
            Play(worldSpacePosition);
        }

    }
}


