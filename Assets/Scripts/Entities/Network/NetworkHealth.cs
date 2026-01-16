using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Entities.Network
{
    public sealed class NetworkHealth : ViewableProperty<float>, IHealth
    {
        public float Max => _config.Max;
        private readonly HealthConfig _config;
        private bool _isSynchronizing;
        private readonly NetworkHealthData _networkData;

        public NetworkHealth(ILifetimed lifetimed, HealthConfig config, NetworkHealthData networkData)
        {
            _config = config;
            _networkData = networkData;
            lifetimed.WhenAlive(OnAlive);
        }

        private void OnAlive(Lifetime lifetime)
        {
            Advise(lifetime, health => Debug.Log("Health changed: " + health));

            _networkData.SpawnedLifetime.WhenAlive(lifetime, aliveLifetime =>
            {
                Advise(aliveLifetime, newHealth =>
                {
                    if (!_isSynchronizing && NetworkManager.Singleton.IsServer /* TODO: исправить */) 
                        _networkData.Health.Value = newHealth;
                });

                aliveLifetime.Bracket(
                    () => _networkData.Health.OnValueChanged += SynchronizeHealth,
                    () => _networkData.Health.OnValueChanged -= SynchronizeHealth);
            });
            
            return;

            void SynchronizeHealth(float _, float newValue)
            {
                _isSynchronizing = true;
                Value = newValue;
                _isSynchronizing = false;
            }
        }
    }
}