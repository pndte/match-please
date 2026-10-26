using Bw.Entities.Physics;
using Bw.Entities.Simulation;
using Bw.UseCases.Mortality.Guarding.Extensions;
using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Mortality.Guarding
{
    public sealed class Guard : IReadonlyGuard, IGuardCover, IGuardBlows, ISteppable<GuardStance>, IReadonlySimulation<GuardState>, IRestorable<GuardState>
    {
        public float Max => _config.MaxHealth;
        public IReadonlyProperty<GuardState> State => _state;
        public GuardState Previous { get; private set; }
        public ISource<GuardState> Stepped => _stepped;

        private int RestTicks =>
            Mathf.CeilToInt(Mathf.Max(Mathf.Max(_config.RegenDelay, _config.BrokenRegenDelay), _config.RegenInterval) / _step.Duration);

        private readonly ViewableProperty<GuardState> _state;
        private readonly Signal<GuardState> _stepped = new();
        private readonly GuardConfig _config;
        private readonly ISimulationStep _step;
        private readonly BodyBox _box;

        public Guard(GuardConfig config, ISimulationStep step, BodyBox box)
        {
            _config = config;
            _step = step;
            _box = box;
            _state = new ViewableProperty<GuardState>(new GuardState(config.MaxHealth, false, false, 0f, 0, 0));
            Previous = _state.Value;
        }

        public void Apply(GuardState state) =>
            _state.Value = state;

        public void Step(GuardStance stance)
        {
            Previous = _state.Value;
            _state.Value = Recovered(Previous.Counted(RestTicks).Faced(stance));
            _stepped.Fire(_state.Value);
        }

        public bool InCover(Contact contact) =>
            _state.Value.Up && Vector2.Angle(contact.Point - _box.Offset, Facing()) <= _config.CoverAngle * 0.5f;

        public void Absorb(Impacts impacts)
        {
            var damage = this.CoveredDamage(impacts);
            if (damage <= 0f)
                return;

            _state.Value = _state.Value.AfterBlow(damage * (1f - _config.Armor));
        }

        private GuardState Recovered(GuardState state)
        {
            if (state.Broken)
                return Seconds(state.TicksSinceBlow) >= _config.BrokenRegenDelay ? state.Mended(Mathf.Min(Max, _config.RegenAmount)) : state;

            if (state.Health <= 0f)
                return state.AsBroken();

            return Regenerating(state) ? state.AfterRegen(_config.RegenAmount, Max) : state;
        }

        private bool Regenerating(GuardState state) =>
            state.Health < Max
            && Seconds(state.TicksSinceBlow) >= _config.RegenDelay
            && Seconds(state.TicksSinceRegen) >= _config.RegenInterval;

        private Vector2 Facing()
        {
            var radians = _state.Value.Aim * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        private float Seconds(int ticks) =>
            ticks * _step.Duration;
    }
}
