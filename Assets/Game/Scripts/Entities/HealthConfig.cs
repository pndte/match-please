using UnityEngine;

namespace Bw.Entities
{
    [CreateAssetMenu(fileName = "HealthConfig", menuName = "Configs/HealthConfig")]
    public class HealthConfig : ScriptableObject
    {
        public float Max;
    }
}