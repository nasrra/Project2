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

        public void Fire(int projectilePrefabId, int shootPointId)
        {
            Projectile projectile = projectilePool.ActivateFromPool(projectilePrefabId);
            
            Transform projectileTransform = projectile.transform;
            Transform shootPoint = shootPoints[shootPointId];

            projectileTransform.position = shootPoint.position;
            projectileTransform.rotation = shootPoint.rotation;
        }

        public void Fire(int projectilePrefabId, int shootPointId, Vector3 direction)
        {
            Projectile projectile = projectilePool.ActivateFromPool(projectilePrefabId);
            
            Transform projectileTransform = projectile.transform;
            Transform shootPoint = shootPoints[shootPointId];

            projectileTransform.position = shootPoint.position;
            projectileTransform.rotation = Quaternion.LookRotation(direction);
        }

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

