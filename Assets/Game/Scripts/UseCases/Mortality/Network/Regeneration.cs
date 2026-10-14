using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Mortality.Network
{
    public sealed class Regeneration
    {
        private readonly IReadonlyMortal _mortal;
        private readonly IAffectable<Heal> _healable;
        private readonly INetworkTicks _ticks;
        private readonly RegenerationConfig _config;

        private int _lastHitTick;
        private int _lastHealTick;

        public Regeneration(
            Lifetime lifetime,
            IReadonlyMortal mortal,
            IAffectable<Heal> healable,
            INetworkTicks ticks,
            RegenerationConfig config)
        {
            _mortal = mortal;
            _healable = healable;
            _ticks = ticks;
            _config = config;

            mortal.Health.AdviseDamage(lifetime, _ => _lastHitTick = ticks.Current);
            mortal.State.WhenAlive(lifetime, aliveLifetime => ticks.Ticked(TickPhase.Default).Advise(aliveLifetime, Step));
        }

        private void Step(int tick)
        {
            if (_mortal.Current.Health.Current >= _mortal.Health.Max)
                return;

            if (Seconds(tick - _lastHitTick) < _config.Delay || Seconds(tick - _lastHealTick) < _config.Interval)
                return;

            _lastHealTick = tick;
            _healable.Affect(StateCause.None, new Heal(_config.Amount));
        }

        private float Seconds(int ticks) =>
            ticks * _ticks.Duration;
    }
}
