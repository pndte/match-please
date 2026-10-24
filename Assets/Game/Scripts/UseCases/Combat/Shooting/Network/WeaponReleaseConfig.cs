using UnityEngine;

namespace Bw.UseCases.Combat.Shooting.Network
{
    [CreateAssetMenu(fileName = "WeaponReleaseConfig", menuName = "Configs/WeaponReleaseConfig")]
    public sealed class WeaponReleaseConfig : ScriptableObject
    {
        [Min(0f)] public float DropSpeed = 2f;
        [Min(0.01f)] public float PullStep = 0.05f;
        public LayerMask Blockers = (1 << 14) | (1 << 16);
    }
}
