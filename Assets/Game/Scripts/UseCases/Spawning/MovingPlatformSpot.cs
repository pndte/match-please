using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Spawning
{
    public sealed class MovingPlatformSpot : MonoBehaviour
    {
        public NetworkObject Platform => _platform;

        [SerializeField] private NetworkObject _platform;
    }
}
