using UnityEngine;

namespace Entropek.Combat
{

    public class Hurtbox : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private EntityStats.HealthSystem health;
        public EntityStats.HealthSystem Health => health;
    }

}

