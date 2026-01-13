using Entities;
using Entities.Network;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    public class Character : MonoBehaviour
    {
        private IHealth _health;
        private NetworkHealthData _networkHealthData;

        [Inject]
        private void Construct(IHealth health, NetworkHealthData networkHealthData)
        {
            _health = health;
            _networkHealthData = networkHealthData;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnMouseDown();
            }
        }

        private void OnMouseDown()
        {
            if (!_networkHealthData.IsServer)
            {
                Debug.LogWarning($"[Client] No authority to modify health. IsServer: {_networkHealthData.IsServer}, IsOwner: {_networkHealthData.IsOwner}");
                return;
            }

            Debug.Log($"[{((_networkHealthData.IsServer) ? "Server" : "Client")}] Before Hit: {_health.Value}");
            _health.Value -= 10;
            Debug.Log($"[{((_networkHealthData.IsServer) ? "Server" : "Client")}] After Hit: {_health.Value}");
        }
    }
}