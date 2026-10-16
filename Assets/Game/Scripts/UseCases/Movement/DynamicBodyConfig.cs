using UnityEngine;

namespace Bw.UseCases.Movement
{
    [CreateAssetMenu(fileName = "DynamicBodyConfig", menuName = "Configs/DynamicBodyConfig")]
    public sealed class DynamicBodyConfig : ScriptableObject
    {
        public float GravityScale = 6f;
        public LayerMask Contacts;

        [Header("Material")]
        [Min(0.01f)] public float Density = 1f;
        [Min(0f)] public float Friction = 0.6f;
        [Range(0f, 1f)] public float Bounciness;
        [Min(0f)] public float LinearDamping;
        [Min(0f)] public float AngularDamping;
    }
}
