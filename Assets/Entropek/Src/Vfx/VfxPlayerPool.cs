using Entropek.Vfx;
using UnityEngine;

namespace Entropek.Collections
{
    public class VfxPlayerPool : GameObjectPool<VfxPlayer>{
        
        static Transform poolContainerParent;
        
        protected override Transform GetPoolContainerParent()
        {
            return poolContainerParent;
        }

        static class Bootstrap
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            static void Initialise()
            {
                GameObject g = new()
                {
                    name = nameof(VfxPlayerPool)
                };

                poolContainerParent = g.transform;

                DontDestroyOnLoad(poolContainerParent);
            }
        }
    }
}

