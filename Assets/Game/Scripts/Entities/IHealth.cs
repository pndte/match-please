using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.Entities
{
    public interface IReadonlyHealth
    {
        public float Max { get; }
        public IReadonlyProperty<float> Current { get; }
    }

    public interface IHealth : IReadonlyHealth
    {
        public new IViewableProperty<float> Current { get; }
    }
    
    public sealed class Health : IHealth
    {
        public float Max => _config.Max;
        public IViewableProperty<float> Current { get; }

        private readonly HealthConfig _config;
        private bool _isSynchronizing;
        IReadonlyProperty<float> IReadonlyHealth.Current => Current;

        public Health(Lifetime lifetime, HealthConfig config, IViewableProperty<float> valueProperty)
        {
            _config = config;
            Current = valueProperty;
            OnAlive(lifetime);
        }

        private void OnAlive(Lifetime lifetime)
        {
            Current.Advise(lifetime, health => Debug.Log("Health changed: " + health));
        }
    }
}