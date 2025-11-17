using System.Collections.Generic;
using Entropek.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace Entropek.Vfx
{
    [RequireComponent(typeof(VfxPlayerPool))]
    public class VfxPlayerSpawner : MonoBehaviour
    {

        // the Singleton object that holds all vfx containers of every vfx player.

        [Header("Components")]
        [SerializeField] VfxPlayerPool vfxPool;        

        public void PlayVfx(int vfxId, Vector3 worldSpacePosition, Vector3 rotationEuler)
        {
            VfxPlayer vfx = vfxPool.ActivateFromPool(vfxId);
            vfx.Play(worldSpacePosition, rotationEuler);
        }

        public void PlayVfx(int vfxId, Vector3 worldSpacePosition)
        {
            PlayVfx(vfxId, worldSpacePosition, Quaternion.identity);
        }

        public void PlayVfx(int vfxId, Vector3 worldSpacePosition, Quaternion rotation)
        {
            VfxPlayer vfx = vfxPool.ActivateFromPool(vfxId);
            vfx.Play(worldSpacePosition, rotation);
        }
    }

}

