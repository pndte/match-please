using UnityEngine;

namespace Entities
{
    [CreateAssetMenu(fileName = "HealthConfig", menuName = "Configs/HealthConfig")]
    public class HealthConfig : ScriptableObject
    {
        public float MaxHealth;
    }
}