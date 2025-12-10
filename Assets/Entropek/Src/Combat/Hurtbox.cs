using UnityEngine;

namespace Entropek.Combat
{

    public class Hurtbox : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private EntityStats.Health health;
        public EntityStats.Health Health => health;
    }

}

