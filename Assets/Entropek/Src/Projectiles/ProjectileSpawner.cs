using System.Collections;
using UnityEngine;

namespace Entropek.Projectiles
{
    [RequireComponent(typeof(ProjectilePool))]
    public class ProjectileSpawner : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] Transform[] shootPoints;
        [SerializeField] ProjectilePool projectilePool;
        
        void Awake()
        {
            // StartCoroutine(TestFire());
        }

        /// <summary>
        /// Spawn a projectile copying a spawn points position and rotate (both in world-space).
        /// </summary>
        /// <param name="projectilePrefabId">The id of the projectile prefab to spawn.</param>
        /// <param name="shootPointId">The id of the point to spawn at.</param>

        public void Fire(int projectilePrefabId, int shootPointId)
        {
            Projectile projectile = projectilePool.ActivateFromPool(projectilePrefabId);
            
            Transform projectileTransform = projectile.transform;
            Transform shootPoint = shootPoints[shootPointId];

            projectileTransform.position = shootPoint.position;
            projectileTransform.rotation = shootPoint.rotation;
        }

        /// <summary>
        /// Spawn a projectile copying a spawn points position (in world-space).
        /// </summary>
        /// <param name="projectilePrefabId">The id of the projectile prefab to spawn.</param>
        /// <param name="shootPointId">The id of the point to spawn at.</param>
        /// <param name="direction">The direction for the projectile to move in.</param>

        public void Fire(int projectilePrefabId, int shootPointId, Vector3 direction)
        {
            Projectile projectile = projectilePool.ActivateFromPool(projectilePrefabId);
            
            Transform projectileTransform = projectile.transform;
            Transform shootPoint = shootPoints[shootPointId];

            projectileTransform.position = shootPoint.position;
            projectileTransform.rotation = Quaternion.LookRotation(direction);
        }

        /// <summary>
        /// Spawn a projectile copying a spawn points position (in world-space).
        /// </summary>
        /// <param name="projectilePrefabId">The id of the projectile prefab to spawn.</param>
        /// <param name="shootPointId">The id of the point to spawn at.</param>
        /// <param name="transform">A transform component to fire towards.</param>

        public void Fire(int projectilePrefabId, int shootPointId, Transform transform)
        {
            Projectile projectile = projectilePool.ActivateFromPool(projectilePrefabId);
            
            Transform projectileTransform = projectile.transform;
            Transform shootPoint = shootPoints[shootPointId];

            projectileTransform.position = shootPoint.position;
            projectileTransform.LookAt(transform); 
        }

        private IEnumerator TestFire()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.167f);
                Fire(0,0);                
            }
        }
    }
}

