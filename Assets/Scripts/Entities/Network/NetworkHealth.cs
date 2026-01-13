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

        public NetworkHealth(Lifetime lifetime, HealthConfig config, NetworkHealthData networkData)
        {
            _config = config;
            Advise(lifetime, health => Debug.Log("Health changed: " + health));

            networkData.SpawnedLifetime.WhenAlive(lifetime, aliveLifetime =>
            {
                Advise(aliveLifetime, newHealth =>
                {
                    if (!_isSynchronizing && NetworkManager.Singleton.IsServer /* TODO: исправить */) 
                        networkData.Health.Value = newHealth;
                });

                aliveLifetime.Bracket(
                    () => networkData.Health.OnValueChanged += SynchronizeHealth,
                    () => networkData.Health.OnValueChanged -= SynchronizeHealth);
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