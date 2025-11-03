using UnityEngine;

namespace Entropek.Projectiles
{
    public class Projectile : MonoBehaviour{
        [SerializeField] private float speed;
        public float Speed => speed;
        [SerializeField] private float damage;
        public float Damage => damage;

        private void LateUpdate(){
            transform.position += transform.forward * speed;
        }

        private void OnTriggerEnter(Collider other)
        {
        }

        private void OnTriggerExit(Collider other)
        {
            
        }
    }
}

