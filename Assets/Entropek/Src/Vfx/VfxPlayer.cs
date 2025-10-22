using UnityEngine;

namespace Entropek.Vfx
{
    public abstract class VfxPlayer : MonoBehaviour
    {
        public abstract void Play();
        public abstract void Play(Vector3 worldSpacePosition);
        public abstract void Play(Vector3 worldSpacePosition, Vector3 rotationEuler);
        public abstract bool IsFinished();
    }
    
}

