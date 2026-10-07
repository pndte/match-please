using System;
using JetBrains.Collections.Viewable;

namespace Bw.Entities
{
    public interface IReadonlyHealth
    {
        public float Max { get; }
        public IReadonlyProperty<HealthState> State { get; }
        public ISource<float> Changed { get; }
    }

    public interface IHealth : IReadonlyHealth
    {
        public void Apply(HealthState state);
    }

    public sealed class Health : IHealth
    {
        public float Max => _config.Max;
        public IReadonlyProperty<HealthState> State => _state;
        public ISource<float> Changed => _changed;

        private readonly HealthConfig _config;
        private readonly ViewableProperty<HealthState> _state;
        private readonly Signal<float> _changed = new();

        public Health(HealthConfig config)
        {
            _config = config;
            _state = new ViewableProperty<HealthState>(new HealthState(config.Max, 0f));
        }

        public void Apply(HealthState state)
        {
            if (state.Current < 0f || state.Current > Max)
                throw new ArgumentOutOfRangeException(nameof(state), "Health must be between zero and the maximum.");

            if (state.Change != 0f)
                _changed.Fire(state.Change);

            _state.Value = state;
        }
    }
}
