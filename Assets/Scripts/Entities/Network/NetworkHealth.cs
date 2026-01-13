using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Entities.Network
{
    public sealed class NetworkHealth : ViewableProperty<float>, IHealth
    {
        public float MaxHealth => _config.MaxHealth;
        private readonly HealthConfig _config;
        private bool _isSynchronizing;

        public NetworkHealth(Lifetime lifetime, HealthConfig config, NetworkHealthData networkData)
        {
            _config = config;
            Value = _config.MaxHealth;

            networkData.SpawnedLifetime.WhenAlive(lifetime, aliveLifetime =>
            {
                Advise(aliveLifetime, newHealth =>
                {
                    if (!_isSynchronizing)
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