using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Spawning.Network
{
    [CreateAssetMenu(fileName = "CrateSpawnConfig", menuName = "Configs/CrateSpawnConfig")]
    public sealed class CrateSpawnConfig : ScriptableObject
    {
        public NetworkObject Prefab;
        [Min(0)] public float RespawnDelay = 15f;
        [Min(0.1f)] public float RetryDelay = 1f;
        public LayerMask Blockers = (1 << 14) | (1 << 15) | (1 << 17);
    }
}
