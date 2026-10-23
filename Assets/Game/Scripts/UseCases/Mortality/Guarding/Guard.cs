using Bw.Entities.Physics;
using Bw.Entities.Simulation;
using Bw.UseCases.Mortality.Guarding.Extensions;
using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Mortality.Guarding
{
    public sealed class Guard : IGuard, IGuardCover, IGuardBlows, ISteppable<GuardStance>
    {
        public float Max => _config.MaxHealth;
        public IReadonlyProperty<GuardState> State => _state;

        private readonly ViewableProperty<GuardState> _state;
        private readonly GuardConfig _config;
        private readonly ISimulationStep _step;
        private readonly BodyBox _box;

        private float _aim;
        private int _ticksSinceBlow = int.MaxValue;
        private int _ticksSinceRegen = int.MaxValue;

        public Guard(GuardConfig config, ISimulationStep step, BodyBox box)
        {
            _config = config;
            _step = step;
            _box = box;
            _state = new ViewableProperty<GuardState>(new GuardState(config.MaxHealth, false, false));
        }

        public void Apply(GuardState state) =>
            _state.Value = state;

        public void Step(GuardStance stance)
        {
            _aim = stance.Aim;
            _ticksSinceBlow = Counted(_ticksSinceBlow);
            _ticksSinceRegen = Counted(_ticksSinceRegen);
            var raised = _state.Value.WithRaised(stance.Raised);
            var recovered = Recovered(raised);
            if (recovered.Health > raised.Health)
                _ticksSinceRegen = 0;

            _state.Value = recovered;
        }

        public bool Covers(Contact contact) => //TODO: прикрытие считается по нынешней стойке и прицелу блока, а не по тику, который видел стрелок: откат выстрела (лагкомпенсация) блок не откатывает
            _state.Value.Up && Vector2.Angle(contact.Point - _box.Offset, Facing()) <= _config.CoverAngle * 0.5f;

        public void Absorb(Impacts impacts)
        {
            var damage = this.CoveredDamage(impacts);
            if (damage <= 0f)
                return;

            _ticksSinceBlow = 0;
            _state.Value = _state.Value.AfterBlow(damage * (1f - _config.Armor));
        }

        private GuardState Recovered(GuardState state)
        {
            if (state.Broken)
                return Seconds(_ticksSinceBlow) >= _config.BrokenRegenDelay ? state.Mended(Mathf.Min(Max, _config.RegenAmount)) : state;

            if (state.Health <= 0f)
                return state.AsBroken();

            return Regenerating(state) ? state.AfterRegen(_config.RegenAmount, Max) : state;
        }

        private bool Regenerating(GuardState state) =>
            state.Health < Max
            && Seconds(_ticksSinceBlow) >= _config.RegenDelay
            && Seconds(_ticksSinceRegen) >= _config.RegenInterval;

        private Vector2 Facing()
        {
            var radians = _aim * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        private float Seconds(int ticks) =>
            ticks * _step.Duration;

        private static int Counted(int ticks) =>
            ticks == int.MaxValue ? ticks : ticks + 1;
    }
}
