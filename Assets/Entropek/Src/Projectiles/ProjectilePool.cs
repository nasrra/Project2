using Entropek.Collections;
using UnityEngine;

namespace Entropek.Projectiles
{    
    public class ProjectilePool : GameObjectPool<Projectile>
    {
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
                    name = nameof(ProjectilePool)
                };

                poolContainerParent = g.transform;

                DontDestroyOnLoad(poolContainerParent);
            }
        }
    }
}

