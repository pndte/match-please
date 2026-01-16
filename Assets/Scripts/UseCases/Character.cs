using Entities;
using Entities.Network;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public class Character : MonoBehaviour, ICharacter
    {
        public IHealth Health { get; private set; }
        IReadonlyHealth IReadonlyCharacter.Health => Health;

        private NetworkHealthData _networkHealthData;

        [Inject]
        private void Construct(IHealth health, NetworkHealthData networkHealthData)
        {
            Health = health;
            _networkHealthData = networkHealthData;
        }

        private void OnMouseDown()
        {
            if (!_networkHealthData.IsServer)
            {
                Debug.LogWarning($"[Client] No authority to modify health. IsServer: {_networkHealthData.IsServer}, IsOwner: {_networkHealthData.IsOwner}");
                return;
            }

            Debug.Log($"[{((_networkHealthData.IsServer) ? "Server" : "Client")}] Before Hit: {Health.Value}");
            Health.Value -= 10;
            Debug.Log($"[{((_networkHealthData.IsServer) ? "Server" : "Client")}] After Hit: {Health.Value}");
        }

    }
}