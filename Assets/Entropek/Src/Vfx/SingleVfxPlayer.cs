using UnityEngine;
using UnityEngine.VFX;

namespace Entropek.Vfx
{

    public class SingleVfxPlayer : VfxPlayer
    {
        [SerializeField] private VisualEffect vfx;
        public VisualEffect Vfx => vfx;

        public override bool IsFinished()
        {
            return vfx.aliveParticleCount == 0 && !vfx.HasAnySystemAwake();
        }


        public override void Play()
        {
            vfx.Play();
        }

        public override void Play(Vector3 worldSpacePosition)
        {
            transform.position = worldSpacePosition;
            Play();
        }

        public override void Play(Vector3 worldSpacePosition, Vector3 rotationEuler)
        {
            transform.rotation = Quaternion.LookRotation(rotationEuler);
            Play(worldSpacePosition);
        }
    }
    
}

