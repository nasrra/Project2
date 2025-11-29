using Entropek.Combat;
using Entropek.EntityStats;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

namespace Entropek.Projectiles
{    
    public class HitScanner : MonoBehaviour
    {
        private const int MaximumHits = 100;

        [SerializeField] private HitScanData[] hitScans;
        [RuntimeField] private RaycastHit[] hits = new RaycastHit[MaximumHits]; 
        private Ray ray;

        private void Awake()
        {
            ray = new Ray();
        }

        /// <summary>
        /// Fires a hitscan at a position in world space.
        /// </summary>
        /// <param name="position">The position in world space to fire at.</param>
        /// <param name="hitScanId">The Id of the hitscan stored in the internal hitScans array.</param>

        public void FireAt(Vector3 position, int hitScanId)
        {
            ref HitScanData hitScan = ref hitScans[hitScanId];

            Vector3 vectorDistance = position - transform.position;
            float distance = vectorDistance.magnitude;

            ray.origin = transform.position;
            ray.direction = vectorDistance.normalized;
            
            if(UnityEngine.Physics.SphereCastNonAlloc(transform.position, hitScan.SphereCastRadius,  vectorDistance.normalized, hits, distance, hitScan.HitLayers, QueryTriggerInteraction.Collide) > 0)
            {
                GameObject hit = hits[0].transform.gameObject;

                // check if the hit gameobjects layer is any one of the
                // set obstruction layers and does not have a tag too ignore.

                int hitLayer = 1 << hit.layer;
                if((hitLayer & hitScan.ObstructionLayers) == 0
                && IgnoreHit(hit, ref hitScan) == false)
                {

                    // damage the hit gameobject if it wasnt an obstruction.

                    DamageContext damageContext = new DamageContext(transform.position, hitScan.DamageAmount, hitScan.DamageType);

                    if(hit.TryGetComponent(out Hurtbox hurtbox))
                    {
                        hurtbox.Health.Damage(damageContext);
                    }
                    else if(hit.TryGetComponent(out HealthSystem healthSystem))
                    {
                        healthSystem.Damage(damageContext);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether or not a GameObject should be ignored.
        /// </summary>
        /// <param name="hitObject">The gameobject that was hit.</param>
        /// <param name="hitScan">The hitscan data used to hit the gameobject.</param>
        /// <returns>true, if the gameobject should be ignored; otherwise false.</returns>

        private bool IgnoreHit(GameObject hitObject, ref HitScanData hitScan)
        {
            for(int i = 0; i < hitScan.IgnoreTags.Length; i++)
            {
                if(hitObject.tag == hitScan.IgnoreTags[i])
                {
                    return true;
                }
            }

            return false;
        }
    }
}

